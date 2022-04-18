using System;
using UnityEngine.UIElements;

namespace Fab.WorldMod.UI
{
	// Make this a Manipulator instead
	public class LayerDropArea : VisualElement
	{
		public new class UxmlFactory : UxmlFactory<LayerDropArea, UxmlTraits> { }

		private static readonly string classname = "drop-target";
		private static readonly string dropClassname = classname + "--drop";

		public int Index { get; private set; }

		private DragDrop dragDrop;

		private Func<VisualElement, LayerDropArea, bool> handleDragFunc;

		public LayerDropArea()
		{
			SetEnabled(false);
			AddToClassList(classname);

			RegisterCallback<FabDragEnterEvent>(OnDragEnter);
			RegisterCallback<FabDragLeaveEvent>(OnDragLeave);
			RegisterCallback<FabDragPerformEvent>(OnDragPerform);
		}

		public LayerDropArea(DragDrop dragDrop, Func<VisualElement, LayerDropArea, bool> handleDragFunc)
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

		public static void Reset(LayerDropArea area)
		{
			area.RemoveFromHierarchy();
			area.Index = -1;
			area.SetEnabled(false);
			area.RemoveFromClassList(dropClassname);
			area.dragDrop.RemoveDropTarget(area);
		}
	}
}
