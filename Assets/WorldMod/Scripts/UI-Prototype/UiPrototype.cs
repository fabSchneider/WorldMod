using UnityEngine;
using UnityEngine.UIElements;

namespace Fab.WorldMod.UI
{
	[RequireComponent(typeof(UIDocument))]
	public class UiPrototype : MonoBehaviour
	{
		private UIDocument document;
		private DataPanelController dataPanelController;
		private TrackpadController trackpadController;

		private DragModel model;

		private TestCameraController cameraController;

		void Start()
		{
			document = GetComponent<UIDocument>();

			model = new DragModel(new string[] { "World Lights", "Population Density", "Climate", "Land", "Infrastructure" });

			var dataPanelContainer = document.rootVisualElement.Q(name: "data-panel");
			dataPanelController = new DataPanelController(dataPanelContainer, model);
			//trackpadController = new TrackpadController(document.rootVisualElement);


			cameraController = FindObjectOfType<TestCameraController>();
			if (cameraController != null)
				document.rootVisualElement.Q<Trackpad>().RegisterCallback<ChangeEvent<Vector2>>(OnTrackpadAxis);
		}


		public void OnTrackpadAxis(ChangeEvent<Vector2> evt)
		{
			cameraController.MoveCamera(evt.newValue);
			Debug.Log(evt.newValue);
		}

	}
}
