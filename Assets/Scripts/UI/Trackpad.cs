using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Fab.WorldMod.UI
{
	public class Trackpad : VisualElement
	{
		public new class UxmlFactory : UxmlFactory<Trackpad, UxmlTraits> { }

		private static readonly string pointerTouchType = "touch";

		private static readonly string classname = "trackpad";
		private static readonly string cursorClassname = classname + "__cursor";

		private VisualElement primaryCursor;
		private VisualElement secondaryCursor;

		private Vector4 value;
		public Vector4 Value => value;

		private Vector2 primaryAxis;
		private Vector2 secondaryAxis;

		private bool isTouch;

		private int primaryPointerID= -1;
		private int secondaryPointerID = -1;

		public Trackpad()
		{
			AddToClassList(classname);

			primaryCursor = new VisualElement().WithClass(cursorClassname);
			primaryCursor.usageHints = UsageHints.DynamicTransform;
			primaryCursor.style.display = DisplayStyle.None;
			primaryCursor.pickingMode = PickingMode.Ignore;
			Add(primaryCursor);

			secondaryCursor = new VisualElement().WithClass(cursorClassname);
			secondaryCursor.usageHints = UsageHints.DynamicTransform;
			secondaryCursor.style.display = DisplayStyle.None;
			secondaryCursor.pickingMode = PickingMode.Ignore;
			Add(secondaryCursor);

			RegisterCallback<PointerDownEvent>(OnPointerDown);
			RegisterCallback<PointerUpEvent>(OnPointerUp);

		}

		private void OnPointerDown(PointerDownEvent evt)
		{
			// ignore event if track pad is already actuated
			// this is necessary to avoid duplicate events
			// from touchscreen that simulate touch as mouse input
			if (isTouch)
				return;

			primaryPointerID = evt.pointerId;
			isTouch = evt.pointerType == pointerTouchType;

			Vector3 axis = CalculateAxis(evt.localPosition);
			primaryCursor.style.display = DisplayStyle.Flex;
			SetCursorPos(axis, primaryCursor);
			this.CapturePointer(evt.pointerId);

			primaryAxis = CalculateAxis(evt.localPosition);
			SetValue(Vector2.zero);
			RegisterCallback<PointerMoveEvent>(OnPointerMove);;
		}

		private void OnPointerMove(PointerMoveEvent evt)
		{
			if (isTouch && evt.pointerType != pointerTouchType)
				return;

			Vector2 axis = CalculateAxis(this.WorldToLocal(evt.position));

			if (secondaryPointerID == -1)
			{
				if (evt.pointerId == primaryPointerID)
				{
					SetCursorPos(axis, primaryCursor);
					SetValue(axis - primaryAxis);
					primaryAxis = axis;
				}
				else
				{
					// Secondary pointer down
					secondaryPointerID = evt.pointerId;
					secondaryCursor.style.display = DisplayStyle.Flex;
					secondaryAxis = axis;
					SetCursorPos(axis, secondaryCursor);
				}
			}
			else
			{
				if(evt.pointerId == primaryPointerID)
				{
					float pinch = (secondaryAxis - primaryAxis).magnitude - (secondaryAxis - axis).magnitude;
					float angle = Vector2.SignedAngle(secondaryAxis - primaryAxis, secondaryAxis - axis);
					SetValue(new Vector4(0f, 0f, angle, pinch));
					primaryAxis = axis;
					SetCursorPos(axis, primaryCursor);
				}
				else if (evt.pointerId == secondaryPointerID)
				{
					float pinch = (secondaryAxis - primaryAxis).magnitude - (axis - primaryAxis).magnitude;
					float angle = Vector2.SignedAngle(secondaryAxis - primaryAxis, axis - primaryAxis);
					SetValue(new Vector4(0f, 0f, angle, pinch));
					secondaryAxis = axis;
					SetCursorPos(axis, secondaryCursor);
				}
			}
		}

		private void OnPointerUp(PointerUpEvent evt)
		{
			// ignore events from non pointers if track pad was actuated by touch
			if (isTouch && evt.pointerType != pointerTouchType)
				return;

			if (evt.pointerId == primaryPointerID)
			{
				if(secondaryPointerID != -1)
				{
					this.ReleasePointer(evt.pointerId);
					primaryPointerID = secondaryPointerID;
					primaryAxis = secondaryAxis;
					this.CapturePointer(primaryPointerID);
					secondaryPointerID = -1;
					secondaryCursor.style.display = DisplayStyle.None;
				}
				else
				{
					primaryCursor.style.display = DisplayStyle.None;
					this.ReleasePointer(evt.pointerId);
					UnregisterCallback<PointerMoveEvent>(OnPointerMove);
					isTouch = false;
					primaryPointerID = -1;
					secondaryPointerID = -1;
					secondaryCursor.style.display = DisplayStyle.None;
				}
			}
			else if (evt.pointerId == secondaryPointerID)
			{
				secondaryPointerID = -1;
				secondaryCursor.style.display = DisplayStyle.None;
			}
		}

		private void SetValue(Vector4 value)
		{
			Vector4 previousValue = value;
			this.value = value;

			using (ChangeEvent<Vector4> changeEvent = ChangeEvent<Vector4>.GetPooled(previousValue, value))
			{
				changeEvent.target = this;
				SendEvent(changeEvent);
			}
		}

		private Vector2 CalculateAxis(Vector2 localPos)
		{
			Vector2 radius = localBound.size / 2f;
			return Vector2.ClampMagnitude((localPos - radius) / radius, 1f);
		}

		private void SetCursorPos(Vector2 axis, VisualElement cursor)
		{
			Vector2 radius = localBound.size / 2f;
			cursor.transform.position = Vector2.Scale(axis, radius) + radius;
		}
	}
}
