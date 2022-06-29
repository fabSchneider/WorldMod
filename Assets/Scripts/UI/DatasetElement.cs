using Fab.Localization;
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
		private static readonly string removeBtnClassname = classname + "__remove-button";

		public new class UxmlFactory : UxmlFactory<DatasetElement, UxmlTraits> { }

		public DataPanelController Controller { get; private set; }
		public int Id { get; private set; }

		private Label label;
		private Localizable localizable;
		private Button removeButton;

		public DatasetElement()
		{
			AddToClassList(classname);
			focusable = true;

			label = new Label();
			label.AddToClassList(labelClassname);
			Add(label);

			removeButton = new Button(RemoveFromSequence);
			removeButton.text = "";
			removeButton.AddToClassList(removeBtnClassname);
			Add(removeButton);

			RegisterCallback<PointerDownEvent>(OnPointerDown);
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
			removeButton.style.color = color;
			style.borderTopColor = color;
			style.borderBottomColor = color;
			style.borderLeftColor = color;
			style.borderRightColor = color;
		}

		public void ResetColor()
		{
			label.style.color = StyleKeyword.Null;
			removeButton.style.color = StyleKeyword.Null;
			style.borderTopColor = StyleKeyword.Null;
			style.borderBottomColor = StyleKeyword.Null;
			style.borderLeftColor = StyleKeyword.Null;
			style.borderRightColor = StyleKeyword.Null;
		}

		private void RemoveFromSequence()
		{
			Dataset dataset = Controller.Stock[Id];
			Controller.Sequence.Remove(dataset);
			Controller.RefreshView();
		}

		private void OnPointerDown(PointerDownEvent evt)
		{
			if (evt.button == 0)
			{
				if (Controller != null)
				{
					Dataset dataset = Controller.Stock[Id];
					if (Controller.Sequence.IndexOf(dataset) == -1)
					{
						int id = Id;
						Controller.Sequence.Insert(dataset, Controller.Sequence.Count);
						Controller.RefreshView();
						Controller.SelectDataset(id);
					}
					else
					{
						Controller.SelectDataset(Id);
					}

				}
				evt.StopPropagation();
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
			element.ResetColor();
		}
	}


}
