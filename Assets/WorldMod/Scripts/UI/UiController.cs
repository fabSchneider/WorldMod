using System.Collections.Generic;
using Cysharp.Threading.Tasks;
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
		private LogOutputController logOutputController;

		public DataPanelController DataPanel => dataPanelController;
		public LogOutputController LogOutput => logOutputController;

		private VisualElement infoPanel;

		private Modal markerModal;

		private VisualElement root;

		public VisualElement Root => root;

		private VisualElement tutorialOverlay;
		private static readonly string tutorialOverlayClassname = "tutorial-overlay";

		private double lastTimeClick;
		public double TimeSinceLastClick => Time.unscaledTimeAsDouble - lastTimeClick;




		void Start()
		{
			document = GetComponent<UIDocument>();
			root = document.rootVisualElement;

			SetupTrackpad();

			mainBarController = new MainbarController(root, LocalizationComponent.Localization);

			infoPanelController = new InfoPanelController(root);

			logOutputController = new LogOutputController(root);

			if (datasets)
				dataPanelController = new DataPanelController(document.rootVisualElement, datasets.Stock, datasets.Sequence);

			SetupFooter();

			root.RegisterCallback<PointerDownEvent>(evt => lastTimeClick = Time.unscaledTimeAsDouble);

			SetupShutdownButtons();

			SetupTutorialOverlay();
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

		private long shutdownPressTime = 3000;
		private bool btn1Down;
		private bool btn2Down;

		private readonly static string shutdownBtn1Name = "shutdown-btn-1";
		private readonly static string shutdownBtn2Name = "shutdown-btn-2";

		private void SetupShutdownButtons()
		{
			var btn1 = root.Q(name: shutdownBtn1Name);
			var btn2 = root.Q(name: shutdownBtn2Name);

			btn1.RegisterCallback<PointerDownEvent>(OnShutdownButtonDown);
			btn2.RegisterCallback<PointerDownEvent>(OnShutdownButtonDown);

			btn1.RegisterCallback<PointerUpEvent>(OnShutdownButtonUp);
			btn2.RegisterCallback<PointerUpEvent>(OnShutdownButtonUp);
		}

		private void OnShutdownButtonDown(PointerDownEvent evt)
		{
			VisualElement elem = ((VisualElement)evt.target);

			elem.CapturePointer(evt.pointerId);
			if (elem.name == shutdownBtn1Name)
				btn1Down = true;
			else if(elem.name == shutdownBtn2Name)
				btn2Down = true;

			if (btn1Down && btn2Down)
				elem.schedule.Execute(ExectuteShutdown).ExecuteLater(shutdownPressTime);

			Debug.Log("Down " + elem.name);
		}

		private void OnShutdownButtonUp(PointerUpEvent evt)
		{
			VisualElement elem = ((VisualElement)evt.target);
			if (elem.name == shutdownBtn1Name)
				btn1Down = false;
			else if (elem.name == shutdownBtn2Name)
				btn2Down = false;
			Debug.Log("Up " + elem.name);
			elem.ReleasePointer(evt.pointerId);
		}

		private void ExectuteShutdown()
		{
			if (btn1Down && btn2Down)
			{
				Debug.Log("Quitting...");
#if UNITY_EDITOR
				UnityEditor.EditorApplication.ExitPlaymode();
#else
				Application.Quit();
#endif
			}

		}

		private Dictionary<Locale, Texture2D> tutorialImgsByLocale = new Dictionary<Locale, Texture2D>();

		public void AddTutorialOverlayImg(Texture2D tex, Locale locale)
		{
			tutorialImgsByLocale.Add(locale, tex);
		}

		private void SetupTutorialOverlay()
		{
			tutorialOverlay = new VisualElement();
			tutorialOverlay.AddToClassList(tutorialOverlayClassname);
			tutorialOverlay.RegisterCallback<PointerUpEvent>(evt => CloseTutorialoverlay().Forget());

			var infobtn = mainBarController.MainBar.Q<Button>(name: "info-btn");
			infobtn.clicked += () =>
			{
				if(tutorialImgsByLocale.TryGetValue(LocalizationComponent.Localization.ActiveLocale, out Texture2D tex))
				{
					tutorialOverlay.style.backgroundImage = tex;
					root.Add(tutorialOverlay);
				}
			};
		}

		async UniTaskVoid CloseTutorialoverlay()
		{
			await UniTask.DelayFrame(1);
			tutorialOverlay.style.backgroundImage = null;
			tutorialOverlay.RemoveFromHierarchy();
		}

		//private void SetupMarkerUI()
		//{
		//	markerModal = new Modal();
		//	markerModal.Title = "MARKER_TITLE";
		//	markerModal.TitleLabel.WithLocalizable();
		//	markerModal.VisibleCloseButton = false;
		//	var text = new Label("MARKER_TEXT").WithLocalizable();
		//	markerModal.Add(text);
		//	markerModal.AddButton("YES", () =>
		//	{
		//		Debug.Log("Marker added at " + cameraController.GetCoordinate());
		//		markerModal.RemoveFromHierarchy();
		//	}).WithLocalizable();
		//	markerModal.AddButton("NO", () =>
		//	{
		//		Debug.Log("Canceled adding marker");
		//		markerModal.RemoveFromHierarchy();
		//	}).WithLocalizable();

		//	document.rootVisualElement.Q<Button>(name: "add-marker-btn").clicked += () => document.rootVisualElement.Add(markerModal);
		//}

		private void SetupFooter()
		{
			var footer = root.Q(name: "footer");

			var versionLabel = footer.Q<Label>(name: "version");
			versionLabel.text = $"{{ world mod - v{Application.version} }}";

			var fpsLabel = footer.Q<Label>(name: "fps");
			fpsLabel.schedule.Execute(() => fpsLabel.text = $"{{ FPS: {(1f / Time.smoothDeltaTime):0.00} }}").Every(40);
		}

		public void OnTrackpadAxis(ChangeEvent<Vector4> evt)
		{
			cameraController.SpinCamera(evt.newValue);
		}


	}
}
