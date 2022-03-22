using UnityEngine;
using UnityEngine.UIElements;

namespace Fab.WorldMod
{
    public class Trackpad : VisualElement
    {
		public new class UxmlFactory : UxmlFactory<Trackpad, UxmlTraits> { }


		private static readonly string classname = "trackpad";


		public Trackpad()
		{
			AddToClassList(classname);

			RegisterCallback<PointerDownEvent>(OnPointerDown);
			RegisterCallback<PointerUpEvent>(OnPointerUp);
			RegisterCallback<PointerMoveEvent>(OnPointerMove);
		}

		private void OnPointerDown(PointerDownEvent evt)
		{

		}
		private void OnPointerUp(PointerUpEvent evt)
		{

		}

		private void OnPointerMove(PointerMoveEvent evt)
		{
			Debug.Log(this.WorldToLocal(evt.position));
		}
	}
}
