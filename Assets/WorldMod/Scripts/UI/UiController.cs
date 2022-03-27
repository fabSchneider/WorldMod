using Fab.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace Fab.WorldMod.UI
{
	[RequireComponent(typeof(UIDocument))]
	public class UiController : MonoBehaviour
	{
		private UIDocument document;

		[SerializeField]
		private DatasetsComponent datasets;

		private WorldCameraController cameraController;

		private DataPanelController dataPanelController;

		private VisualElement mainBar;
		private VisualElement infoPanel;

		private bool dataPanelDirty;

		private Modal markerModal;

		void Start()
		{
			document = GetComponent<UIDocument>();

			var dataPanelContainer = document.rootVisualElement.Q(name: "data-panel");
			if (datasets)
				dataPanelController = new DataPanelController(dataPanelContainer, datasets.Stock, datasets.Layers);

			cameraController = FindObjectOfType<WorldCameraController>();
			if (cameraController != null)
				document.rootVisualElement.Q<Trackpad>().RegisterCallback<ChangeEvent<Vector2>>(OnTrackpadAxis);

			mainBar = document.rootVisualElement.Q(name: "main-bar");
			mainBar.Q<RadioButtonGroup>(name: "language-toggles").RegisterValueChangedCallback(evt =>
			 {
				 var rbGroup = evt.target as RadioButtonGroup;
				 int i = 0;
				 var signal = Signals.Get<OnChangeLocaleSignal>();
				 foreach (var choice in rbGroup.choices)
				 {
					 if(i == evt.newValue)
					 {
						 switch (choice.ToLower())
						 {
							 case "en":
								 signal.Dispatch(new Locale("en-US"));
								 return;
							 case "de":
								 signal.Dispatch(new Locale("de-DE"));
								 return;
						 }
					 }
					 else
					 {
						 i++;
					 }
				 }				
			 });


			infoPanel = document.rootVisualElement.Q(name: "info-panel");

			infoPanel.Add(new Label("Info Text").WithLocalizable("info-text"));

			markerModal = new Modal();
			markerModal.Title = " Add a marker";
			markerModal.TitleLabel.WithLocalizable("add-marker.title");
			markerModal.VisibleCloseButton = false;
			var text = new Label("Do you want to add a marker at the current position?").WithLocalizable("add.marker.text");
			markerModal.Add(text);
			markerModal.AddButton("Yes", () =>
			{
				Debug.Log("Marker added at " + cameraController.GetCoordinate());
				markerModal.RemoveFromHierarchy();
			}).WithLocalizable("yes");
			markerModal.AddButton("No", () =>
			{
				Debug.Log("Canceled adding marker");
				markerModal.RemoveFromHierarchy();
			}).WithLocalizable("no");

			document.rootVisualElement.Q<Button>(name: "add-marker-btn").clicked += () => document.rootVisualElement.Add(markerModal);
		}

		private void OnEnable()
		{
			Signals.Get<DatasetUpdatedSignal>().AddListener(OnDatasetUpdated);
		}

		private void OnDisable()
		{
			Signals.Get<DatasetUpdatedSignal>().RemoveListener(OnDatasetUpdated);
		}

		public void OnTrackpadAxis(ChangeEvent<Vector2> evt)
		{
			cameraController.MoveCamera(evt.newValue);
		}

		private void OnDatasetUpdated(Dataset dataset)
		{
			if (dataset.Owner == datasets.Stock)
				dataPanelDirty = true;
		}

		private void Update()
		{
			if (dataPanelDirty)
			{
				dataPanelController?.RefreshView();
				dataPanelDirty = false;
			}
		}

	}
}
