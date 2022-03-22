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
			Vector3 axis = CalculateAxis(this.WorldToLocal(evt.position));
			cursor.style.display = DisplayStyle.Flex;
			SetCursorPos(axis);
			this.CapturePointer(evt.pointerId);
			SetAxisValue(axis);

			RegisterCallback<PointerMoveEvent>(OnPointerMove);
		}
		private void OnPointerUp(PointerUpEvent evt)
		{
			cursor.style.display = DisplayStyle.None;
			UnregisterCallback<PointerMoveEvent>(OnPointerMove);
			this.ReleasePointer(evt.pointerId);
			SetAxisValue(Vector2.zero);
		}

		private void OnPointerMove(PointerMoveEvent evt)
		{
			Vector3 axis = CalculateAxis(this.WorldToLocal(evt.position));
			SetCursorPos(axis);
			SetAxisValue(axis);
		}


		private void SetAxisValue(Vector2 axis)
		{
			Vector2 previousValue = value;
			value = new Vector2(axis.x, -axis.y);

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
