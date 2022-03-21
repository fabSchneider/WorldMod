using Fab.WorldMod.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Fab.WorldMod
{
	[RequireComponent(typeof(UIDocument))]
	public class UiPrototype : MonoBehaviour
	{
		private UIDocument document;
		private DataPanelController dataPanelController;

		private DragModel model;

		void Start()
		{
			document = GetComponent<UIDocument>();

			model = new DragModel(new string[] { "World Lights", "Population Density", "Climate", "Land", "Infrastructure" });

			var dataPanelContainer = document.rootVisualElement.Q(className: "proto__data-panel");
			dataPanelController = new DataPanelController(dataPanelContainer, model);
		}


	}
}
