using UnityEngine.UIElements;

//This is a modified copy of https://github.com/needle-mirror/com.unity.ui/blob/master/Core/Events/DragAndDropEvents.cs
//Unfortunately drag and drop events are currently not accessible for runtime so we need to use this copy for now
namespace Fab.WorldMod.UI
{
    /// <summary>
    /// Interface for drag and drop events.
    /// </summary>
    public interface IFabDragAndDropEvent
    {
    }

    /// <summary>
    /// Base class for drag and drop events.
    /// </summary>
    public abstract class FabDragAndDropEventBase<T> : PointerEventBase<T>, IFabDragAndDropEvent where T : FabDragAndDropEventBase<T>, new()
    {
    }

    /// <summary>
    /// The event sent to a dragged element when the drag and drop process ends.
    /// </summary>
    public class FabDragExitedEvent : FabDragAndDropEventBase<FabDragExitedEvent>
    {
        /// <summary>
        /// Resets the event members to their initial values.
        /// </summary>
        protected override void Init()
        {
            base.Init();
            LocalInit();
        }

        void LocalInit()
        {
            tricklesDown = true;
            bubbles = true;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public FabDragExitedEvent()
        {
            LocalInit();
        }

        ///// <summary>
        ///// Gets an event from the event pool and initializes it with the given values. Use this function instead of creating new events. Events obtained using this method need to be released back to the pool. You can use `Dispose()` to release them.
        ///// </summary>
        ///// <param name="systemEvent">An IMGUI drag exited event.</param>
        ///// <returns>An initialized event.</returns>
        //public new static GraspDragExitedEvent GetPooled(Event systemEvent)
        //{
        //    // We get DragExitedEvent if the drag operation ends or if the mouse exits the window during the drag.
        //    // If drag operation ends, mouse was released, so notify PointerDeviceState about this.
        //    // If mouse exited window, we will eventually get a DragUpdatedEvent that will restore the pressed state.
        //    if (systemEvent != null)
        //    {
        //        //PointerDeviceState.ReleaseButton(PointerId.mousePointerId, systemEvent.button);
        //    }

        //    return GraspDragAndDropEventBase<GraspDragExitedEvent>.GetPooled(systemEvent);
        //}

        //protected internal override void PostDispatch(IPanel panel)
        //{
        //    EventBase pointerEvent = ((IMouseEventInternal)this).sourcePointerEvent as EventBase;
        //    if (pointerEvent == null)
        //    {
        //        // If pointerEvent != null, base.PostDispatch() will take care of this.
        //        (panel as BaseVisualElementPanel)?.CommitElementUnderPointers();
        //    }
        //    base.PostDispatch(panel);
        //}
    }

    /// <summary>
    /// Use the DragEnterEvent class to manage events that occur when dragging enters an element or one of its descendants. The DragEnterEvent is cancellable, it does not trickle down, and it does not bubble up.
    /// </summary>
    public class FabDragEnterEvent : FabDragAndDropEventBase<FabDragEnterEvent>
    {
        /// <summary>
        /// Resets the event members to their initial values.
        /// </summary>
        protected override void Init()
        {
            base.Init();
            LocalInit();
        }

        void LocalInit()
        {
            tricklesDown = true;
        }

        /// <summary>
        /// Constructor. Avoid renewing events. Instead, use GetPooled() to get an event from a pool of reusable events.
        /// </summary>
        public FabDragEnterEvent()
        {
            LocalInit();
        }
    }

    /// <summary>
    /// Use the DragLeaveEvent class to manage events sent when dragging leaves an element or one of its descendants. The DragLeaveEvent is cancellable, it does not trickle down, and it does not bubble up.
    /// </summary>
    public class FabDragLeaveEvent : FabDragAndDropEventBase<FabDragLeaveEvent>
    {
        /// <summary>
        /// Resets the event members to their initial values.
        /// </summary>
        protected override void Init()
        {
            base.Init();
            LocalInit();
        }

        void LocalInit()
        {
            tricklesDown = true;
        }

        /// <summary>
        /// Constructor. Avoid renewing events. Instead, use GetPooled() to get an event from a pool of reusable events.
        /// </summary>
        public FabDragLeaveEvent()
        {
            LocalInit();
        }
    }

    /// <summary>
    /// The event sent when the element being dragged enters a possible drop target.
    /// </summary>
    public class FabDragUpdatedEvent : FabDragAndDropEventBase<FabDragUpdatedEvent>
    {
        ///// <summary>
        ///// Gets an event from the event pool and initializes it with the given values. Use this function instead of creating new events. Events obtained using this method need to be released back to the pool. You can use `Dispose()` to release them.
        ///// </summary>
        ///// <param name="systemEvent">An IMGUI drag updated event.</param>
        ///// <returns>An initialized event.</returns>
        //public new static GraspDragUpdatedEvent GetPooled(Event systemEvent)
        //{
        //    // During a drag operation, if mouse exits window, we get a DragExitedEvent, which releases the mouse button.
        //    // If the mouse comes back in the window, we get DragUpdatedEvents. We thus make sure the button is
        //    // flagged as pressed.
        //    if (systemEvent != null)
        //    {
        //        //PointerDeviceState.PressButton(PointerId.mousePointerId, systemEvent.button);
        //    }

        //    // We adopt the same convention as for MouseMoveEvents.
        //    // We thus reset e.button.
        //    GraspDragUpdatedEvent e = GraspDragAndDropEventBase<GraspDragUpdatedEvent>.GetPooled(systemEvent);
        //    e.button = 0;
        //    return e;
        //}

        //internal static GraspDragUpdatedEvent GetPooled(PointerMoveEvent pointerEvent)
        //{
        //    return GraspDragAndDropEventBase<GraspDragUpdatedEvent>.GetPooled(pointerEvent);
        //}

        //protected internal override void PostDispatch(IPanel panel)
        //{
        //    EventBase pointerEvent = ((IMouseEventInternal)this).sourcePointerEvent as EventBase;
        //    if (pointerEvent == null)
        //    {
        //        // If pointerEvent != null, base.PostDispatch() will take care of this.
        //        (panel as BaseVisualElementPanel)?.CommitElementUnderPointers();
        //    }
        //    base.PostDispatch(panel);
        //}
    }

    /// <summary>
    /// The event sent to an element when another element is dragged and dropped on the element.
    /// </summary>
    public class FabDragPerformEvent : FabDragAndDropEventBase<FabDragPerformEvent>
    {
    }
}
