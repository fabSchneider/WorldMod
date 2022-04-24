using Fab.Common;
using Fab.Geo;
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

		private VisualElement root;

		public VisualElement Root => root;

		private double lastTimeClick;

		public double TimeSinceLastClick => Time.unscaledTimeAsDouble - lastTimeClick;

		void Start()
		{
			document = GetComponent<UIDocument>();
			root = document.rootVisualElement;

			SetupTrackpad();

			mainBarController = new MainbarController(document.rootVisualElement, LocalizationComponent.Localization);

			infoPanelController = new InfoPanelController(document.rootVisualElement);

			if (datasets)
				dataPanelController = new DataPanelController(document.rootVisualElement, datasets.Stock, datasets.Sequence);

			//SetupMarkerUI();

			SetupFooter();

			root.RegisterCallback<PointerDownEvent>(evt => lastTimeClick = Time.unscaledTimeAsDouble);

		}

		private void OnEnable()
		{
			//Signals.Get<DatasetUpdatedSignal>().AddListener(OnDatasetUpdated);
		}

		private void OnDisable()
		{
			//Signals.Get<DatasetUpdatedSignal>().RemoveListener(OnDatasetUpdated);
		}

		private void SetupTrackpad()
		{
			cameraController = FindObjectOfType<WorldCameraController>();

			if (cameraController != null)
			{
				root.Q<Trackpad>().RegisterCallback<ChangeEvent<Vector4>>(OnTrackpadAxis);
				Label trackpadLabel = document.rootVisualElement.Q<Label>(name = "trackpad-output");
				trackpadLabel.schedule.Execute(() =>
				{
					Coordinate coord = cameraController.GetCoordinate();
					trackpadLabel.text = string.Format("lon: {0:0.000}°  lat: {1:0.000}°  heading: {2:0.0}°",
						Mathf.Rad2Deg * coord.longitude,
						Mathf.Rad2Deg * coord.latitude,
						cameraController.GetHeading());
				}).Every(100);
			}
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

		private void SetupFooter()
		{
			var footer = root.Q(name: "footer");

			var versionLabel = footer.Q<Label>(name: "version");
			versionLabel.text = $"{{ world mod - v{Application.version} }}";

			var fpsLabel = footer.Q<Label>(name: "fps");
			fpsLabel.schedule.Execute(() => fpsLabel.text = $"{{ FPS: {(1f / Time.smoothDeltaTime):0.00} }}").Every(40);

			//Label lastClickTimeLabel = new Label();
			//lastClickTimeLabel.schedule.Execute(() => lastClickTimeLabel.text = $"{{ time since last interaction: {TimeSinceLastClick:0s} }}").Every(1000);
			//footer.Add(lastClickTimeLabel);
		}

		public void OnTrackpadAxis(ChangeEvent<Vector4> evt)
		{
			cameraController.SpinCamera(evt.newValue);
		}

		//private void OnDatasetUpdated(Dataset dataset)
		//{
		//	if (dataset.Owner == datasets.Stock)
		//		dataPanelDirty = true;
		//}

		//private void Update()
		//{
		//	if (dataPanelDirty)
		//	{
		//		dataPanelController?.RefreshView();
		//		dataPanelDirty = false;
		//	}
		//}

	}
}
