
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Fab.WorldMod.UI
{
	public class WireTest : MonoBehaviour
	{
		public UIDocument document;

		public Vector2 start, end;
		public float thickness = 1f;
		public Color tint = Color.white;
		public Texture texture;
		public bool useGradient;
		public Gradient gradient;
		public bool useAdaptiveResolution;
		public int resolution = 16;


		private Wire wire;

		void Start()
		{
			wire = new Wire();
			wire.Thickness = thickness;
			wire.Resolution = resolution;
			wire.SetEndpoints(start, end);
			wire.UseAdaptiveResolution = useAdaptiveResolution;
			wire.Texture = texture;
			wire.Gradient = gradient;
			wire.Tint = tint;

			document.rootVisualElement.Add(wire);
		}


		private void OnValidate()
		{
			if (wire != null)
			{
				wire.Thickness = thickness;
				wire.Start = start;
				wire.SetEndpoints(start, end);
				wire.UseAdaptiveResolution = useAdaptiveResolution;
				wire.Gradient = gradient;
				wire.Tint = tint;

				wire.Texture = texture;
				wire.Resolution = resolution;
			}
		}

		public void Update()
		{
			if (Mouse.current.leftButton.isPressed)
			{
				Vector2 pos = Mouse.current.position.ReadValue();
				pos = new Vector2(pos.x, Screen.height - pos.y) / 2f;
				pos =  document.rootVisualElement.WorldToLocal(pos);
				wire.End = pos;
			}
		}

	}
}
