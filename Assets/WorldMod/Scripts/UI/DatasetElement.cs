using Fab.Common;
using Fab.WorldMod.Localization;
using UnityEngine;
using UnityEngine.UIElements;

namespace Fab.WorldMod.UI
{
	public class DatasetElement : VisualElement
	{
		private static readonly string classname = "dataset-item";
		private static readonly string activeClassname = classname + "--active";
		private static readonly string labelClassname = classname + "__label";
		private static readonly string dragPreviewClassname = classname + "__drag";
		private static readonly string dragPreviewActiveClassname = dragPreviewClassname + "--active";

		public new class UxmlFactory : UxmlFactory<DatasetElement, UxmlTraits> { }

		public class DragPreview : VisualElement
		{
			public DatasetElement DatasetElement { get; }
			public Vector2 dragOffset;
			public DragPreview(DatasetElement item)
			{
				DatasetElement = item;

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
				style.width = DatasetElement.resolvedStyle.width;
				style.height = DatasetElement.resolvedStyle.height;
			}

			private void OnDragUpdated(FabDragUpdatedEvent evt)
			{
				AddToClassList(dragPreviewActiveClassname);
				transform.position = parent.WorldToLocal((Vector2)evt.position - dragOffset);
			}

			private void OnDragPerformed(FabDragPerformEvent evt)
			{
				DatasetElement.Controller.DragDrop.EndDrag();
				RemoveFromHierarchy();
				RemoveFromClassList(dragPreviewActiveClassname);
				DatasetElement.Controller.StockContainer.Query<LayerDropArea>().ForEach(a => a.SetEnabled(false));
				DatasetElement.Controller.LayersContainer.Query<LayerDropArea>().ForEach(a => a.SetEnabled(false));
			}

			private void OnDragExited(FabDragExitedEvent evt)
			{
				DatasetElement.Controller.DragDrop.EndDrag();

				RemoveFromHierarchy();
				RemoveFromClassList(dragPreviewActiveClassname);
				DatasetElement.Controller.StockContainer.Query<LayerDropArea>().ForEach(a => a.SetEnabled(false));
				DatasetElement.Controller.LayersContainer.Query<LayerDropArea>().ForEach(a => a.SetEnabled(false));
			}
		}

		public DataPanelController Controller { get; private set; }
		public int Id { get; private set; }

		private DragPreview dragPreview;

		private Label label;
		private Localizable localizable;

		public DatasetElement()
		{
			AddToClassList(classname);
			focusable = true;

			label = new Label();
			label.AddToClassList(labelClassname);
			Add(label);

			RegisterCallback<PointerDownEvent>(OnPointerDown);

			dragPreview = new DragPreview(this);
		}

		public DatasetElement(ILocalization localization) : this()
		{
			localizable = new Localizable(localization);
		}

		public void SetActive(bool value)
		{
			EnableInClassList(activeClassname, value);
		}


		private void OnPointerDown(PointerDownEvent evt)
		{
			if (evt.button == 0)
			{
				Controller.SetActiveDatasetElement(this);

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

			Controller.SetActiveDatasetElement(null);

			// start drag	
			Controller.DragDrop.DragLayer.Add(dragPreview);
			Controller.DragDrop.StartDrag(dragPreview);
			dragPreview.PrepareForDrag(evt.position);

			//enable all drop areas
			Controller.LayersContainer.Query<LayerDropArea>().ForEach(a => a.SetEnabled(true));
			Controller.StockContainer.Query<LayerDropArea>().ForEach(a => a.SetEnabled(true));

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
			Controller = controller;
			Id = index;
			label.text = controller.Stock[index].Name;
			label.AddManipulator(localizable);
		}

		public static void Reset(DatasetElement element)
		{
			element.Controller = null;
			element.SetActive(false);
			element.SetEnabled(true);
			element.RemoveFromHierarchy();
			element.RemoveFromClassList(activeClassname);
			element.label.text = string.Empty;
			element.label.RemoveManipulator(element.localizable);
			element.dragPreview.RemoveFromHierarchy();
		}
	}


}
