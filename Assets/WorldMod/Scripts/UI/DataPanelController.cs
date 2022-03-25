using System;
using Fab.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace Fab.WorldMod.UI
{

	public class DataPointItem : VisualElement
	{
		private static readonly string classname = "datapoint-item";
		private static readonly string activeClassname = classname + "--active";
		private static readonly string labelClassname = classname + "__label";
		private static readonly string dragPreviewClassname = classname + "__drag";
		private static readonly string dragPreviewActiveClassname = dragPreviewClassname + "--active";

		public new class UxmlFactory : UxmlFactory<DataPointItem, UxmlTraits> { }

		public class DragPreview : VisualElement
		{
			public DataPointItem DragItem { get; }
			public Vector2 dragOffset;
			public DragPreview(DataPointItem item)
			{
				DragItem = item;

				AddToClassList(dragPreviewClassname);
				pickingMode = PickingMode.Ignore;
				usageHints = UsageHints.DynamicTransform;

				RegisterCallback<FabDragUpdatedEvent>(OnDragUpdated);
				RegisterCallback<FabDragPerformEvent>(OnDragPerformed);
				RegisterCallback<FabDragExitedEvent>(OnDragExited);
			}

			public void PrepareForDrag(Vector2 pos)
			{
				transform.position = parent.WorldToLocal(pos - dragOffset);
				style.width = DragItem.resolvedStyle.width;
				style.height = DragItem.resolvedStyle.height;
			}

			private void OnDragUpdated(FabDragUpdatedEvent evt)
			{
				AddToClassList(dragPreviewActiveClassname);
				transform.position = parent.WorldToLocal((Vector2)evt.position - dragOffset);
			}

			private void OnDragPerformed(FabDragPerformEvent evt)
			{
				DragItem.Controller.DragDrop.EndDrag();
				RemoveFromHierarchy();
				RemoveFromClassList(dragPreviewActiveClassname);
				DragItem.Controller.StockContainer.Query<DropArea>().ForEach(a => a.SetEnabled(false));
				DragItem.Controller.LayersContainer.Query<DropArea>().ForEach(a => a.SetEnabled(false));
			}

			private void OnDragExited(FabDragExitedEvent evt)
			{
				DragItem.Controller.DragDrop.EndDrag();

				RemoveFromHierarchy();
				RemoveFromClassList(dragPreviewActiveClassname);
				DragItem.Controller.StockContainer.Query<DropArea>().ForEach(a => a.SetEnabled(false));
				DragItem.Controller.LayersContainer.Query<DropArea>().ForEach(a => a.SetEnabled(false));
			}
		}

		public DataPanelController Controller { get; private set; }
		public int Id { get; private set; }

		private DragPreview dragPreview;

		public DataPointItem()
		{
			AddToClassList(classname);
			var label = new Label();
			label.AddToClassList(labelClassname);
			Add(label);

			RegisterCallback<PointerDownEvent>(OnPointerDown);

			dragPreview = new DragPreview(this);
		}

		private void OnPointerDown(PointerDownEvent evt)
		{
			if (evt.button == 0)
			{
				dragPreview.dragOffset = this.WorldToLocal(evt.position);
				RegisterCallback<PointerLeaveEvent>(OnPointerDragLeave);
				RegisterCallback<PointerUpEvent>(OnPointerUpEvent);
			}
		}

		private void OnPointerUpEvent(PointerUpEvent evt)
		{
			UnregisterCallback<PointerUpEvent>(OnPointerUpEvent);
			UnregisterCallback<PointerLeaveEvent>(OnPointerDragLeave);
		}

		private void OnPointerDragLeave(PointerLeaveEvent evt)
		{
			UnregisterCallback<PointerUpEvent>(OnPointerUpEvent);
			UnregisterCallback<PointerLeaveEvent>(OnPointerDragLeave);

			// start drag	
			Controller.DragDrop.DragLayer.Add(dragPreview);
			Controller.DragDrop.StartDrag(dragPreview);
			dragPreview.PrepareForDrag(evt.position);

			//enable all drop areas
			Controller.LayersContainer.Query<DropArea>().ForEach(a => a.SetEnabled(true));
			Controller.StockContainer.Query<DropArea>().ForEach(a => a.SetEnabled(true));

			if (parent == Controller.LayersContainer)
			{
				// disable drop areas before and after this element
				int id = parent.IndexOf(this);
				parent[id - 1].SetEnabled(false);
				parent[id + 1].SetEnabled(false);
			}
		}

		public void Set(DataPanelController controller, int index)
		{
			this.Controller = controller;
			Id = index;
			this.Q<Label>(className: labelClassname).text = controller.Stock.Datasets[index].Name;
		}

		public static void Reset(DataPointItem item)
		{
			item.Controller = null;
			item.SetEnabled(true);
			item.RemoveFromHierarchy();
			item.RemoveFromClassList(activeClassname);
			item.Q<Label>(className: labelClassname).text = string.Empty;
			item.dragPreview.RemoveFromHierarchy();
		}
	}

	/// <summary>
	/// A visual element that functions as a drop area for the reorganization of items
	/// as well as a button to insert a new item at a specific position in the deck.
	/// </summary>

	// Make this a Manipulator instead
	public class DropArea : VisualElement
	{
		public new class UxmlFactory : UxmlFactory<DropArea, UxmlTraits> { }

		private static readonly string classname = "drop-target";
		private static readonly string dropClassname = classname + "--drop";

		public int Index { get; private set; }

		private DragDrop dragDrop;

		private Func<VisualElement, DropArea, bool> handleDragFunc;

		public DropArea()
		{
			SetEnabled(false);
			AddToClassList(classname);

			RegisterCallback<FabDragEnterEvent>(OnDragEnter);
			RegisterCallback<FabDragLeaveEvent>(OnDragLeave);
			RegisterCallback<FabDragPerformEvent>(OnDragPerform);
		}

		public DropArea(DragDrop dragDrop, Func<VisualElement, DropArea, bool> handleDragFunc)
			: this()
		{
			this.dragDrop = dragDrop;
			this.handleDragFunc = handleDragFunc;
		}

		private void OnDragEnter(FabDragEnterEvent evt)
		{
			AddToClassList(dropClassname);
		}
		private void OnDragLeave(FabDragLeaveEvent evt)
		{
			RemoveFromClassList(dropClassname);
		}

		private void OnDragPerform(FabDragPerformEvent evt)
		{

			if (handleDragFunc.Invoke(dragDrop.DraggedElement, this))
				dragDrop.AcceptDrop(evt);
			else
				dragDrop.DenyDrop(evt);

			RemoveFromClassList(dropClassname);
		}


		public void Set(int index)
		{
			Index = index;
			dragDrop.AddDropTarget(this);
		}

		public static void Reset(DropArea area)
		{
			area.RemoveFromHierarchy();
			area.Index = -1;
			area.SetEnabled(false);
			area.RemoveFromClassList(dropClassname);
			area.dragDrop.RemoveDropTarget(area);
		}
	}

	/// <summary>
	/// Controls the data panel.
	/// </summary>
	public class DataPanelController
	{
		private static readonly string className = "data-panel";
		private static readonly string stockContainerClassname = className + "__stock-container";
		private static readonly string layersContainerClassname = className + "__layers-container";
		private static readonly string containerDropClassname = "container-drop";
		private static readonly string layersDropClassname = "layers-drop";
		private static readonly string insertLineClassname = layersDropClassname + "__line";

		private VisualElement stockContainer;
		private VisualElement layersContainer;

		public VisualElement StockContainer => stockContainer;
		public VisualElement LayersContainer => layersContainer;

		private ObjectPool<DataPointItem> dragItemPool;
		private ObjectPool<DropArea> dragInserAreaPool;

		public DragDrop DragDrop { get; private set; }

		public VisualElement Root { get; private set; }

		public DatasetStock Stock { get; private set; }
		public DatasetLayers Layers { get; private set; }

		private DropArea layersContainerDropArea;

		public DataPanelController(VisualElement root, DatasetStock stock, DatasetLayers layers)
		{
			Root = root;
			Stock = stock;
			Layers = layers;

			VisualElement dragLayer = new VisualElement().AsLayer(blocking: false).WithName("drag-layer");
			dragLayer.focusable = true;
			root.Add(dragLayer);
			DragDrop = new DragDrop(dragLayer);

			stockContainer = root.Q(className: stockContainerClassname);
			DropArea stockDropArea = new DropArea(DragDrop, HandleStockDrop).WithClass(containerDropClassname);
			stockDropArea.Set(-1);
			stockContainer.Add(stockDropArea);

			layersContainer = root.Q(className: layersContainerClassname);
			layersContainerDropArea = new DropArea(DragDrop, HandleLayersDrop).WithClass(containerDropClassname);
			layersContainerDropArea.Set(0);

			dragItemPool = new ObjectPool<DataPointItem>(8, true, () => new DataPointItem(), DataPointItem.Reset);
			dragInserAreaPool = new ObjectPool<DropArea>(8, true, CreateLayersDropArea, DropArea.Reset);

			root.RegisterCallback<FabDragPerformEvent>(OnDropPerformed);

			RefreshView();
		}

		private bool HandleStockDrop(VisualElement item, DropArea area)
		{
			if (item is DataPointItem.DragPreview dragPreview)
				return Layers.RemoveFromLayers(dragPreview.DragItem.Id);

			return false;
		}

		private DropArea CreateLayersDropArea()
		{
			DropArea area = new DropArea(DragDrop, HandleLayersDrop);
			area.AddToClassList(layersDropClassname);
			VisualElement insertLine = new VisualElement();
			insertLine.AddToClassList(insertLineClassname);
			insertLine.pickingMode = PickingMode.Ignore;
			area.Add(insertLine);
			return area;
		}

		private bool HandleLayersDrop(VisualElement item, DropArea area)
		{
			if (item is DataPointItem.DragPreview dragPreview)
			{
				Layers.InsertLayer(dragPreview.DragItem.Id, area.Index);
				return true;
			}

			return false;
		}

		private void OnDropPerformed(FabDragPerformEvent evt)
		{
			RefreshView();
		}

		public void RefreshView()
		{
			ClearContainers();

			for (int i = 0; i < Stock.Datasets.Count; i++)
			{
				DataPointItem item = dragItemPool.GetPooled();
				item.Set(this, i);
				item.SetEnabled(!Layers.IsLayer(Stock.Datasets[i]));
				stockContainer.Add(item);
			}

			if (Layers.Datasets.Count == 0)
			{
				layersContainerDropArea.Set(0);
				layersContainer.Add(layersContainerDropArea);
			}
			else
			{
				DropArea insertArea = dragInserAreaPool.GetPooled();
				insertArea.Set(0);
				layersContainer.Add(insertArea);
				for (int i = 0; i < Layers.Datasets.Count; i++)
				{
					DataPointItem item = dragItemPool.GetPooled();
					
					item.Set(this, Stock.GetIndex(Layers.Datasets[i]));
					layersContainer.Add(item);
					insertArea = dragInserAreaPool.GetPooled();
					insertArea.Set(i + 1);
					layersContainer.Add(insertArea);
				}
			}
		}

		private void ClearContainers()
		{
			stockContainer.Query<DataPointItem>().ForEach(item => dragItemPool.ReturnToPool(item));

			layersContainerDropArea.RemoveFromHierarchy();
			DropArea.Reset(layersContainerDropArea);
			layersContainer.Query<DataPointItem>().ForEach(item => dragItemPool.ReturnToPool(item));
			layersContainer.Query<DropArea>().ForEach(item => dragInserAreaPool.ReturnToPool(item));
		}
	}
}
