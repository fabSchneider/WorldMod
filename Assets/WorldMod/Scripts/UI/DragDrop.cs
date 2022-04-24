using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Fab.WorldMod.UI
{

    public class DragDrop
    {
        private VisualElement dragLayer;
        public VisualElement DragLayer => dragLayer;

        private VisualElement draggedElement;
        public VisualElement DraggedElement => draggedElement;

        private VisualElement currentDropTarget;

        public VisualElement CurrentDropTarget => currentDropTarget;

        private List<VisualElement> dropTargets;

        private bool canceled;
        public bool Canceled => canceled;

		public event Action dragStarted;

        public DragDrop(VisualElement dragLayer)
        {
            this.dragLayer = dragLayer;
            dropTargets = new List<VisualElement>();
        }

        public void AddDropTargets(IEnumerable<VisualElement> elements)
        {
            dropTargets.AddRange(elements);
        }

        public void AddDropTarget(VisualElement element)
        {
            if (!dropTargets.Contains(element))
                dropTargets.Add(element);
        }

        public void RemoveDropTarget(VisualElement element)
        {
            dropTargets.Remove(element);
            if (currentDropTarget == element)
                currentDropTarget = null;

        }

        public void StartDrag(VisualElement dragElement)
        {
            if (draggedElement != null)
            {
                Debug.LogError("Already dragging");
                return;
            }

            draggedElement = dragElement;

			dragLayer.RegisterCallback<PointerMoveEvent>(OnMove);
            dragLayer.RegisterCallback<PointerUpEvent>(OnUp);
			dragLayer.RegisterCallback<KeyDownEvent>(OnCancel);

			// Capture mouse, primary touch and pen pointer
			// This is necessary because otherwise the touch up event won't fire
			dragLayer.CapturePointer(PointerId.mousePointerId);
			dragLayer.CapturePointer(PointerId.touchPointerIdBase);
			dragLayer.CapturePointer(PointerId.penPointerIdBase);
			dragLayer.Focus();

			dragStarted?.Invoke();

		}


        public void EndDrag()
        {
            if (draggedElement == null)
                return;

			dragLayer.UnregisterCallback<PointerUpEvent>(OnUp);
            dragLayer.UnregisterCallback<PointerMoveEvent>(OnMove);
            dragLayer.UnregisterCallback<KeyDownEvent>(OnCancel);

			// Release all pointers
			dragLayer.ReleasePointer(PointerId.mousePointerId);
			dragLayer.ReleasePointer(PointerId.touchPointerIdBase);
			dragLayer.ReleasePointer(PointerId.penPointerIdBase);
            dragLayer.Blur();

            draggedElement = null;
            currentDropTarget = null;
            canceled = false;
        }

        public void AcceptDrop(FabDragPerformEvent evt)
        {
            using FabDragPerformEvent draggedPerformEvent = FabDragPerformEvent.GetPooled(evt);
            draggedPerformEvent.target = draggedElement;
            draggedElement.SendEvent(draggedPerformEvent);
        }

        public void DenyDrop(FabDragPerformEvent evt)
        {
            using FabDragExitedEvent exitedEvent = FabDragExitedEvent.GetPooled(evt);
            exitedEvent.target = draggedElement;
            draggedElement.SendEvent(exitedEvent);
        }

        private void OnUp(PointerUpEvent evt)
        {
            if (evt.button != 0)
                return;

            if (CurrentDropTarget != null)
            {
                //send perform event drop target
                using FabDragPerformEvent dropPerformEvent = FabDragPerformEvent.GetPooled(evt);
                dropPerformEvent.target = CurrentDropTarget;
                CurrentDropTarget.SendEvent(dropPerformEvent);
            }
            else
            {
                using (FabDragExitedEvent exitEvent = FabDragExitedEvent.GetPooled(evt))
                {
                    exitEvent.target = draggedElement;
                    draggedElement.SendEvent(exitEvent);
                }
            }
            evt.StopPropagation();
        }

        private void OnCancel(KeyDownEvent evt)
        {
            Debug.Log("Cancel");

            if (evt.keyCode == KeyCode.Escape)
            {
                canceled = true;

                //drag canceled
                if (CurrentDropTarget != null)
                {
                    using (FabDragLeaveEvent leaveEvent = FabDragLeaveEvent.GetPooled())
                    {
                        leaveEvent.target = CurrentDropTarget;
                        CurrentDropTarget.SendEvent(leaveEvent);
                    }
                }

                using (FabDragExitedEvent exitEvent = FabDragExitedEvent.GetPooled())
                {
                    exitEvent.target = draggedElement;
                    draggedElement.SendEvent(exitEvent);
                }

                evt.StopPropagation();
            }
        }

        private void OnMove(PointerMoveEvent evt)
        {
            evt.StopPropagation();
            VisualElement foundTarget = GetTargetUnderPointer(evt.position);

            //new target
            if (CurrentDropTarget != foundTarget)
            {
                //leave recent target
                if (CurrentDropTarget != null)
                {
                    using (FabDragLeaveEvent leaveEvent = FabDragLeaveEvent.GetPooled(evt))
                    {
                        leaveEvent.target = CurrentDropTarget;
                        CurrentDropTarget.SendEvent(leaveEvent);
                    }
                }

                //set new current target
                currentDropTarget = foundTarget;

                //found new target
                if (CurrentDropTarget != null)
                {
                    //enter new target
                    using (FabDragEnterEvent enterEvent = FabDragEnterEvent.GetPooled(evt))
                    {
                        enterEvent.target = CurrentDropTarget;
                        CurrentDropTarget.SendEvent(enterEvent);
                    }
                }
                //evt.StopPropagation();
            }

            if (CurrentDropTarget != null)
            {
                //update drop target
                using (FabDragUpdatedEvent updatedEvent = FabDragUpdatedEvent.GetPooled(evt))
                {
                    updatedEvent.target = CurrentDropTarget;
                    CurrentDropTarget.SendEvent(updatedEvent);
                }
            }

            using (FabDragUpdatedEvent draggedUpdateEvent = FabDragUpdatedEvent.GetPooled(evt))
            {
                draggedUpdateEvent.target = draggedElement;
                draggedElement.SendEvent(draggedUpdateEvent);
            }
        }

        private VisualElement GetTargetUnderPointer(Vector2 pointerPos)
        {
            foreach (var target in dropTargets)
            {
                if (target.ContainsPoint(target.WorldToLocal(pointerPos)))
                    return target;
            }

            return null;
        }

    }
}
