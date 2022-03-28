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

		private VisualElement mainBar;
		private VisualElement infoPanel;

		private bool dataPanelDirty;

		private Modal markerModal;

		void Start()
		{
			document = GetComponent<UIDocument>();

			cameraController = FindObjectOfType<WorldCameraController>();
			if (cameraController != null)
				document.rootVisualElement.Q<Trackpad>().RegisterCallback<ChangeEvent<Vector2>>(OnTrackpadAxis);

			mainBarController = new MainbarController(document.rootVisualElement);

			infoPanel = document.rootVisualElement.Q(name: "info-panel");
			infoPanel.Add(new Label("Info Text").WithLocalizable("INFO_TEXT"));

			if (datasets)
				dataPanelController = new DataPanelController(document.rootVisualElement, datasets.Stock, datasets.Layers);

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
			markerModal.Title = " Add a marker";
			markerModal.TitleLabel.WithLocalizable("MARKER_TITLE");
			markerModal.VisibleCloseButton = false;
			var text = new Label("Do you want to add a marker at the current position?").WithLocalizable("MARKER_TEXT");
			markerModal.Add(text);
			markerModal.AddButton("Yes", () =>
			{
				Debug.Log("Marker added at " + cameraController.GetCoordinate());
				markerModal.RemoveFromHierarchy();
			}).WithLocalizable("YES");
			markerModal.AddButton("No", () =>
			{
				Debug.Log("Canceled adding marker");
				markerModal.RemoveFromHierarchy();
			}).WithLocalizable("NO");

			document.rootVisualElement.Q<Button>(name: "add-marker-btn").clicked += () => document.rootVisualElement.Add(markerModal);
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
