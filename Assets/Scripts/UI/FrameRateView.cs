using UnityEngine;
using UnityEngine.UIElements;

namespace Fab.WorldMod.UI
{
    public class FrameRateView : MonoBehaviour
    {
		[SerializeField]
		private UIDocument document;

		[SerializeField]
		private Vector2 positionOffset;

		private Label frameRateLabel;

		private void Start()
		{
			frameRateLabel = new Label();
			frameRateLabel.style.position = Position.Absolute;
			frameRateLabel.pickingMode = PickingMode.Ignore;
			frameRateLabel.style.bottom = positionOffset.y;
			frameRateLabel.style.left = positionOffset.x;
			document.rootVisualElement.Add(frameRateLabel);
		}


		public void Update()
		{
			float frameRate = 1f / Time.smoothDeltaTime;
			frameRateLabel.text = $"FPS: { frameRate:0.00}";
		}
	}
}
