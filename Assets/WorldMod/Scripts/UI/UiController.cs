using Fab.Common;
using Fab.WorldMod.Localization;
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

		private MainbarController mainBarController;
		private DataPanelController dataPanelController;
		private InfoPanelController infoPanelController;

		private VisualElement infoPanel;

		private bool dataPanelDirty;

		private Modal markerModal;

		void Start()
		{
			document = GetComponent<UIDocument>();

			cameraController = FindObjectOfType<WorldCameraController>();
			if (cameraController != null)
				document.rootVisualElement.Q<Trackpad>().RegisterCallback<ChangeEvent<Vector3>>(OnTrackpadAxis);

			mainBarController = new MainbarController(document.rootVisualElement, LocalizationComponent.Localization);

			infoPanelController = new InfoPanelController(document.rootVisualElement);

			if (datasets)
				dataPanelController = new DataPanelController(document.rootVisualElement, datasets.Stock, datasets.Sequence);

			SetupMarkerUI();
		}

		private void OnEnable()
		{
			Signals.Get<DatasetUpdatedSignal>().AddListener(OnDatasetUpdated);
		}

		private void OnDisable()
		{
			Signals.Get<DatasetUpdatedSignal>().RemoveListener(OnDatasetUpdated);
		}

		private void SetupMarkerUI()
		{
			markerModal = new Modal();
			markerModal.Title = "MARKER_TITLE";
			markerModal.TitleLabel.WithLocalizable();
			markerModal.VisibleCloseButton = false;
			var text = new Label("MARKER_TEXT").WithLocalizable();
			markerModal.Add(text);
			markerModal.AddButton("YES", () =>
			{
				Debug.Log("Marker added at " + cameraController.GetCoordinate());
				markerModal.RemoveFromHierarchy();
			}).WithLocalizable();
			markerModal.AddButton("NO", () =>
			{
				Debug.Log("Canceled adding marker");
				markerModal.RemoveFromHierarchy();
			}).WithLocalizable();

			document.rootVisualElement.Q<Button>(name: "add-marker-btn").clicked += () => document.rootVisualElement.Add(markerModal);
		}

		public void OnTrackpadAxis(ChangeEvent<Vector3> evt)
		{
			cameraController.SpinCamera(evt.newValue);
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
