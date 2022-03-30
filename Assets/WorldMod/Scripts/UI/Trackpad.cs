using UnityEngine;
using UnityEngine.UIElements;

namespace Fab.WorldMod.UI
{
    public class Trackpad : VisualElement
    {
		public new class UxmlFactory : UxmlFactory<Trackpad, UxmlTraits> { }


		private static readonly string classname = "trackpad";
		private static readonly string cursorClassname = classname + "__cursor";

		private VisualElement cursor;


		private Vector2 value;
		public Vector2 Value => value;

		private Vector2 previousAxis;
		public Trackpad()
		{
			AddToClassList(classname);

			cursor = new VisualElement().WithClass(cursorClassname);
			cursor.usageHints = UsageHints.DynamicTransform;
			cursor.style.display = DisplayStyle.None;
			cursor.pickingMode = PickingMode.Ignore;
			Add(cursor);

			RegisterCallback<PointerDownEvent>(OnPointerDown);
			RegisterCallback<PointerUpEvent>(OnPointerUp);
		}

		private void OnPointerDown(PointerDownEvent evt)
		{
			Vector3 axis = CalculateAxis(evt.localPosition);
			cursor.style.display = DisplayStyle.Flex;
			SetCursorPos(axis);
			this.CapturePointer(evt.pointerId);

			previousAxis = CalculateAxis(evt.localPosition);
			SetValue(Vector2.zero);
			RegisterCallback<PointerMoveEvent>(OnPointerMove);
		}
		private void OnPointerUp(PointerUpEvent evt)
		{
			cursor.style.display = DisplayStyle.None;
			this.ReleasePointer(evt.pointerId);
			UnregisterCallback<PointerMoveEvent>(OnPointerMove);
		}

		private void OnPointerMove(PointerMoveEvent evt)
		{
			
			Vector2 axis = CalculateAxis(evt.localPosition);
			SetCursorPos(axis);

			SetValue(axis - previousAxis);
			previousAxis = axis;
		}


		private void SetValue(Vector2 value)
		{
			Vector2 previousValue = value;
			this.value = value;

			using (ChangeEvent<Vector2> changeEvent = ChangeEvent<Vector2>.GetPooled(previousValue, value))
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

		private void SetCursorPos(Vector2 axis)
		{
			Vector2 radius = localBound.size / 2f;
			cursor.transform.position = Vector2.Scale(axis, radius) + radius;
		}
	}
}
