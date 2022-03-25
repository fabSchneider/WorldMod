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

		private bool dataPanelDirty;

		void Start()
		{
			document = GetComponent<UIDocument>();

			var dataPanelContainer = document.rootVisualElement.Q(name: "data-panel");
			if(datasets)
				dataPanelController = new DataPanelController(dataPanelContainer, datasets.Datasets);

			cameraController = FindObjectOfType<WorldCameraController>();
			if (cameraController != null)
				document.rootVisualElement.Q<Trackpad>().RegisterCallback<ChangeEvent<Vector2>>(OnTrackpadAxis);
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
			if(dataset.Owner == datasets.Datasets)
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
