using Fab.WorldMod.Localization;
using UnityEngine;
using UnityEngine.UIElements;

namespace Fab.WorldMod.UI
{
	public class DragPreview : VisualElement
	{
		private static readonly string dragPreviewClassname = "dataset-item__drag";
		private static readonly string dragPreviewActiveClassname = "dataset-item--active";

		private Draggable draggable;

		public VisualElement Owner => draggable.target;

		private Vector2 dragOffset;
		public DragPreview(Draggable draggable)
		{
			this.draggable = draggable;

			AddToClassList(dragPreviewClassname);
			pickingMode = PickingMode.Ignore;
			usageHints = UsageHints.DynamicTransform;

			RegisterCallback<FabDragUpdatedEvent>(OnDragUpdated);
			RegisterCallback<FabDragPerformEvent>(OnDragPerformed);
			RegisterCallback<FabDragExitedEvent>(OnDragExited);
		}

		public void PrePrepareForDrag(Vector2 dragOffset)
		{
			this.dragOffset = dragOffset;
			style.width = draggable.target.resolvedStyle.width;
			style.height = draggable.target.resolvedStyle.height;
		}

		public void PrepareForDrag(Vector2 pos)
		{
			transform.position = parent.WorldToLocal(pos - dragOffset);
		}

		private void OnDragUpdated(FabDragUpdatedEvent evt)
		{
			AddToClassList(dragPreviewActiveClassname);
			transform.position = parent.WorldToLocal((Vector2)evt.position - dragOffset);
		}

		private void OnDragPerformed(FabDragPerformEvent evt)
		{
			draggable.DragDrop.EndDrag();
			RemoveFromHierarchy();
			RemoveFromClassList(dragPreviewActiveClassname);
		}

		private void OnDragExited(FabDragExitedEvent evt)
		{
			draggable.DragDrop.EndDrag();
			RemoveFromHierarchy();
			RemoveFromClassList(dragPreviewActiveClassname);
		}
	}

	public class Draggable : Manipulator
	{
		public DragDrop DragDrop { get; set; }

		protected override void RegisterCallbacksOnTarget()
		{
			
		}

		protected override void UnregisterCallbacksFromTarget()
		{
			
		}
	}

	public class DatasetElement : VisualElement
		{
		private static readonly string classname = "dataset-item";
		private static readonly string activeClassname = classname + "--active";
		private static readonly string labelClassname = classname + "__label";

		public new class UxmlFactory : UxmlFactory<DatasetElement, UxmlTraits> { }

		public DataPanelController Controller { get; private set; }
		public int Id { get; private set; }

		private DragPreview dragPreview;

		private Label label;
		private Localizable localizable;

		private Draggable draggable;

		public DatasetElement()
		{
			AddToClassList(classname);
			focusable = true;

			label = new Label();
			label.AddToClassList(labelClassname);
			Add(label);

			RegisterCallback<PointerDownEvent>(OnPointerDown);

			draggable = new Draggable();
			this.AddManipulator(draggable);
			dragPreview = new DragPreview(draggable);
		}

		public DatasetElement(ILocalization localization) : this()
		{
			localizable = new Localizable(localization);
		}

		public void SetActive(bool value)
		{
			EnableInClassList(activeClassname, value);
		}

		public void SetColor(Color color)
		{
			label.style.color = color;
			style.borderTopColor = color;
			style.borderBottomColor = color;
			style.borderLeftColor = color;
			style.borderRightColor = color;
		}

		public void ResetColor()
		{
			label.style.color = StyleKeyword.Null;
			style.borderTopColor = StyleKeyword.Null;
			style.borderBottomColor = StyleKeyword.Null;
			style.borderLeftColor = StyleKeyword.Null;
			style.borderRightColor = StyleKeyword.Null;
		}


		private void OnPointerDown(PointerDownEvent evt)
		{
			if (evt.button == 0)
			{
				Controller?.SetActiveDatasetElement(this);

				dragPreview.PrePrepareForDrag(evt.localPosition);
				RegisterCallback<PointerLeaveEvent>(OnPointerDragLeave);
				RegisterCallback<PointerUpEvent>(OnPointerUpEvent);

				evt.StopPropagation();
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

			//Controller.SetActiveDatasetElement(null);

			// start drag	
			Controller.DragDrop.DragLayer.Add(dragPreview);
			Controller.DragDrop.StartDrag(dragPreview);
			dragPreview.PrepareForDrag(evt.position);

			////enable all drop areas
			//Controller.SequenceContainer.Query<LayerDropArea>().ForEach(a => a.SetEnabled(true));
			//Controller.StockContainer.Query<LayerDropArea>().ForEach(a => a.SetEnabled(true));

			if (parent == Controller.SequenceContainer)
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
			draggable.DragDrop = controller.DragDrop;
			Id = index;
			label.text = controller.Stock[index].Name;
			label.AddManipulator(localizable);
		}

		public static void Reset(DatasetElement element)
		{
			element.Controller = null;
			element.draggable.DragDrop = null;
			element.SetActive(false);
			element.SetEnabled(true);
			element.RemoveFromHierarchy();
			element.RemoveFromClassList(activeClassname);
			element.label.text = string.Empty;
			element.label.RemoveManipulator(element.localizable);
			element.ResetColor();
			element.dragPreview.RemoveFromHierarchy();
		}
	}


}
