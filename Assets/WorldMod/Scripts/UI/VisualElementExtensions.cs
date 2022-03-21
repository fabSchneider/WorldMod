using UnityEngine;
using UnityEngine.UIElements;

namespace Fab.WorldMod.UI
{
    public static class VisualElementExtensions
    {
        public static T WithName<T>(this T element, string name) where T : VisualElement
        {
            element.name = name;
            return element;
        }

        public static T WithParent<T>(this T element, VisualElement parent) where T : VisualElement
        {
            parent.Add(element);
            return element;
        }

        public static T WithParent<T>(this T element, VisualElement parent, int index) where T : VisualElement
        {
            parent.Insert(index, element);
            return element;
        }

        public static T WithAbsoluteFill<T>(this T element) where T : VisualElement
        {
            element.style.position = Position.Absolute;
            element.style.left = 0f;
            element.style.right = 0f;
            element.style.top = 0f;
            element.style.bottom = 0f;

            return element;
        }

        public static T WithAbsoluteOverflow<T>(this T element, float overflow) where T : VisualElement
        {
            element.style.position = Position.Absolute;
            element.style.left = -overflow;
            element.style.right = -overflow;
            element.style.top = -overflow;
            element.style.bottom = -overflow;

            return element;
        }

        public static T AsLayer<T>(this T element, bool blocking = false) where T : VisualElement
        {
            element.usageHints = UsageHints.GroupTransform;
            element.pickingMode = blocking ? PickingMode.Position : PickingMode.Ignore;
            element.focusable = true;
            element.style.position = Position.Absolute;
            element.style.left = 0f;
            element.style.right = 0f;
            element.style.top = 0f;
            element.style.bottom = 0f;

            return element;
        }

        public static void SetInClassList<T>(this T element, string className, bool state) where T : VisualElement
        {
            if (state)
                element.AddToClassList(className);
            else
                element.RemoveFromClassList(className);
        }

        public static T WithClass<T>(this T element, string className) where T : VisualElement
        {
            element.AddToClassList(className);
            return element;
        }

        public static T WithPickingMode<T>(this T element, PickingMode mode) where T : VisualElement
        {
            element.pickingMode = mode;
            return element;
        }


        public static T WithAbsoluteRight<T>(this T element, Length width) where T : VisualElement
        {
            element.style.position = Position.Absolute;
            element.style.right = 0f;
            element.style.left = StyleKeyword.Auto;
            element.style.top = 0f;
            element.style.bottom = 0f;
            element.style.width = width;
            element.style.height = StyleKeyword.Auto;
            return element;
        }

        public static T WithAbsoluteLeft<T>(this T element, Length width) where T : VisualElement
        {
            element.style.position = Position.Absolute;
            element.style.right = StyleKeyword.Auto;
            element.style.left = 0f;
            element.style.top = 0f;
            element.style.bottom = 0f;
            element.style.width = width;
            element.style.height = StyleKeyword.Auto;
            return element;
        }

        public static T WithAbsoluteTop<T>(this T element, Length height) where T : VisualElement
        {
            element.style.position = Position.Absolute;
            element.style.right = 0f;
            element.style.left = 0f;
            element.style.top = 0f;
            element.style.bottom = StyleKeyword.Auto;
            element.style.width = StyleKeyword.Auto;
            element.style.height = height;
            return element;
        }

        public static T WithAbsoluteBottom<T>(this T element, Length height) where T : VisualElement
        {
            element.style.position = Position.Absolute;
            element.style.right = 0f;
            element.style.left = 0f;
            element.style.top = StyleKeyword.Auto;
            element.style.bottom = 0f;
            element.style.width = StyleKeyword.Auto;
            element.style.height = height;
            return element;
        }

        public static T WithAbsoluteRect<T>(this T element, Rect rect) where T : VisualElement
        {
            element.style.position = Position.Absolute;
            element.style.width = rect.width;
            element.style.height = rect.height;
            element.style.left = rect.xMin;
            element.style.right = StyleKeyword.Auto;
            element.style.top = rect.yMin;
            element.style.bottom = StyleKeyword.Auto;
            return element;
        }

        public static T WithText<T>(this T btn, string text) where T : TextElement
        {
            btn.text = text;
            return btn;
        }

        public static (bool left, bool right, bool top, bool bottom) GetEdgeProximity(this Rect rect, Vector2 localPos, float maxDistX, float maxDistY)
        {
            bool left = localPos.x < maxDistX;
            bool right = (rect.width - localPos.x) < maxDistX;

            bool top = localPos.y < maxDistY;
            bool bottom = (rect.height - localPos.y) < maxDistY;

            return (left, right, top, bottom);
        }


    }
}
