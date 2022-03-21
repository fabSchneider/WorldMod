using System;
using System.Collections.Generic;
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
				DragItem.Controller.SequenceContainer.Query<DropArea>().ForEach(a => a.SetEnabled(false));
			}

			private void OnDragExited(FabDragExitedEvent evt)
			{
				DragItem.Controller.DragDrop.EndDrag();

				RemoveFromHierarchy();
				RemoveFromClassList(dragPreviewActiveClassname);
				DragItem.Controller.StockContainer.Query<DropArea>().ForEach(a => a.SetEnabled(false));
				DragItem.Controller.SequenceContainer.Query<DropArea>().ForEach(a => a.SetEnabled(false));
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
				//start drag
				dragPreview.dragOffset = this.WorldToLocal(evt.position);
				Controller.DragDrop.DragLayer.Add(dragPreview);
				Controller.DragDrop.StartDrag(dragPreview);
				dragPreview.PrepareForDrag(evt.position);
				Controller.SequenceContainer.Query<DropArea>().ForEach(a => a.SetEnabled(true));
				Controller.StockContainer.Query<DropArea>().ForEach(a => a.SetEnabled(true));
			}
		}

		public void Set(DataPanelController controller, int index)
		{
			this.Controller = controller;
			Id = index;
			this.Q<Label>(className: labelClassname).text = controller.Model.DataStock[index].name;
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
		private static readonly string sequenceContainerClassname = className + "__sequence-container";
		private VisualElement stockContainer;
		private VisualElement sequenceContainer;

		public VisualElement StockContainer => stockContainer;
		public VisualElement SequenceContainer => sequenceContainer;

		private ObjectPool<DataPointItem> dragItemPool;
		private ObjectPool<DropArea> dragInserAreaPool;

		public DragDrop DragDrop { get; private set; }

		public VisualElement Root { get; private set; }

		public DragModel Model { get; private set; }

		private DropArea sequenceContainerDropArea;

		private static readonly string containerDropClassname = "container-drop";
		private static readonly string sequenceDropClassname = "sequence-drop";
		private static readonly string insertLineClassname = sequenceDropClassname + "__line";



		public DataPanelController(VisualElement root, DragModel model)
		{
			Root = root;
			Model = model;

			VisualElement dragLayer = new VisualElement().AsLayer(blocking: false).WithName("drag-layer");
			dragLayer.focusable = true;
			root.Add(dragLayer);
			DragDrop = new DragDrop(dragLayer);

			stockContainer = root.Q(className: stockContainerClassname);
			DropArea stockDropArea = new DropArea(DragDrop, HandleStockDrop).WithClass(containerDropClassname);
			stockDropArea.Set(-1);
			stockContainer.Add(stockDropArea);

			sequenceContainer = root.Q(className: sequenceContainerClassname);
			sequenceContainerDropArea = new DropArea(DragDrop, HandleSequenceDrop).WithClass(containerDropClassname);
			sequenceContainerDropArea.Set(0);

			dragItemPool = new ObjectPool<DataPointItem>(8, true, () => new DataPointItem(), DataPointItem.Reset);
			dragInserAreaPool = new ObjectPool<DropArea>(8, true, CreateSequenceDropArea, DropArea.Reset);

			root.RegisterCallback<FabDragPerformEvent>(OnDropPerformed);

			RefreshView();
		}

		private bool HandleStockDrop(VisualElement item, DropArea area)
		{
			if (item is DataPointItem.DragPreview dragPreview)
				return Model.RemoveFromSequence(dragPreview.DragItem.Id);

			return false;
		}


		private DropArea CreateSequenceDropArea()
		{
			DropArea area = new DropArea(DragDrop, HandleSequenceDrop);
			area.AddToClassList(sequenceDropClassname);
			VisualElement insertLine = new VisualElement();
			insertLine.AddToClassList(insertLineClassname);
			insertLine.pickingMode = PickingMode.Ignore;
			area.Add(insertLine);
			return area;
		}

		private bool HandleSequenceDrop(VisualElement item, DropArea area)
		{
			if (item is DataPointItem.DragPreview dragPreview)
				return Model.InsertIntoSequence(dragPreview.DragItem.Id, area.Index);

			return false;
		}

		private void OnDropPerformed(FabDragPerformEvent evt)
		{
			RefreshView();
		}

		public void RefreshView()
		{
			ClearContainers();

			for (int i = 0; i < Model.DataStock.Count; i++)
			{
				DataPointItem item = dragItemPool.GetPooled();
				item.Set(this, i);
				item.SetEnabled(!Model.IsInSequence(Model.DataStock[i]));
				stockContainer.Add(item);
			}

			if(Model.DataSequence.Count == 0)
			{
				sequenceContainerDropArea.Set(0);
				sequenceContainer.Add(sequenceContainerDropArea);
			}
			else
			{
				DropArea insertArea = dragInserAreaPool.GetPooled();
				insertArea.Set(0);
				sequenceContainer.Add(insertArea);
				for (int i = 0; i < Model.DataSequence.Count; i++)
				{
					DataPointItem item = dragItemPool.GetPooled();
					item.Set(this, Model.DataSequence[i].id);
					sequenceContainer.Add(item);
					insertArea = dragInserAreaPool.GetPooled();
					insertArea.Set(i + 1);
					sequenceContainer.Add(insertArea);
				}
			}
		}

		private void ClearContainers()
		{
			stockContainer.Query<DataPointItem>().ForEach(item => dragItemPool.ReturnToPool(item));

			sequenceContainerDropArea.RemoveFromHierarchy();
			DropArea.Reset(sequenceContainerDropArea);
			sequenceContainer.Query<DataPointItem>().ForEach(item => dragItemPool.ReturnToPool(item));
			sequenceContainer.Query<DropArea>().ForEach(item => dragInserAreaPool.ReturnToPool(item));
		}
	}

	public class DragModel
	{
		public class DataPoint
		{
			public readonly string name;
			public readonly int id;

			public DataPoint(int id, string name)
			{
				this.id = id;
				this.name = name;
			}
		}

		private List<DataPoint> dataStock;
		private List<DataPoint> dataSequence;

		public IReadOnlyList<DataPoint> DataStock => dataStock;

		public IReadOnlyList<DataPoint> DataSequence => dataSequence;

		public DragModel(ICollection<string> dataStock)
		{
			this.dataStock = new List<DataPoint>(dataStock.Count);
			int i = 0;
			foreach (var item in dataStock)
			{
				this.dataStock.Add(new DataPoint(i++, item));
			}


			dataSequence = new List<DataPoint>();
		}

		public bool InsertIntoSequence(int itemId, int index)
		{
			if (itemId < 0 || itemId >= dataStock.Count)
				throw new ArgumentOutOfRangeException(nameof(itemId));

			if (index < 0 || index > dataSequence.Count)
				throw new ArgumentOutOfRangeException(nameof(index));

			DataPoint data = dataStock[itemId];
			// do not allow duplicates
			// change existing items position instead
			int existingIndex = dataSequence.IndexOf(data);
			dataSequence.Insert(index, data);

			if (existingIndex != -1)
			{
				if (index <= existingIndex)
					dataSequence.RemoveAt(existingIndex + 1);
				else
					dataSequence.RemoveAt(existingIndex);
			}

			return true;
		}

		public bool RemoveFromSequence(int itemId)
		{
			if (itemId < 0 || itemId >= dataStock.Count)
				throw new ArgumentOutOfRangeException(nameof(itemId));

			DataPoint data = dataStock[itemId];
			return dataSequence.Remove(data);
		}

		public bool IsInSequence(DataPoint data)
		{
			return dataSequence.Contains(data);
		}
	}
}
