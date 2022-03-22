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

		void Start()
		{
			document = GetComponent<UIDocument>();

			model = new DragModel(new string[] { "World Lights", "Population Density", "Climate", "Land", "Infrastructure" });

			var dataPanelContainer = document.rootVisualElement.Q(name: "data-panel");
			dataPanelController = new DataPanelController(dataPanelContainer, model);
			trackpadController = new TrackpadController(document.rootVisualElement);
		}


	}
}
