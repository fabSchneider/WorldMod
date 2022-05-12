using System;
using System.Collections.Generic;
using Fab.Common;
using Fab.WorldMod.Localization;
using UnityEngine;
using UnityEngine.UIElements;

namespace Fab.WorldMod.UI
{
	/// <summary>
	/// Controls the data panel.
	/// </summary>
	public class DataPanelController
	{
		private static readonly string className = "data-panel";
		private static readonly string stockContainerClassname = className + "__stock-container";
		private static readonly string datasetContainerClassname = className + "__dataset-container";
		private static readonly string sequenceContainerClassname = className + "__sequence-container";
		private static readonly string controlsContainerClassname = className + "__controls-container";

		private static readonly string datasetColorKey = "color";

		private VisualElement stockContainer;
		private VisualElement datasetContainer;

		private VisualElement sequenceContainer;
		private VisualElement controlsContainer;

		public VisualElement SequenceContainer => sequenceContainer;
		public VisualElement StockContainer => datasetContainer;
		public VisualElement ControlsContainer => controlsContainer;

		private ObjectPool<DatasetElement> datasetElementsPool;

		public IList<Dataset> Stock { get; private set; }
		public Sequence<Dataset> Sequence { get; private set; }

		private int selectedDataset = -1;

		private Dictionary<Dataset, DatasetControlView> datasetControlsByDataset;

		public DataPanelController(VisualElement root, IList<Dataset> stock, Sequence<Dataset> sequence)
		{
			Stock = stock;
			Sequence = sequence;

			stockContainer = root.Q(className: stockContainerClassname);
			datasetContainer = stockContainer.Q(className: datasetContainerClassname);

			sequenceContainer = root.Q(className: sequenceContainerClassname);

			sequenceContainer.RegisterCallback<PointerDownEvent>(evt =>
			{
				SelectDataset(-1);
				Signals.Get<DatasetSelectedSignal>().Dispatch(null);
			});

			controlsContainer = root.Q(className: controlsContainerClassname);

			datasetElementsPool = new ObjectPool<DatasetElement>(8, true, () => new DatasetElement(LocalizationComponent.Localization), DatasetElement.Reset);

			datasetControlsByDataset = new Dictionary<Dataset, DatasetControlView>();

			Signals.Get<OnChangeLocaleSignal>().AddListener(RefreshOnLocalChange);

			RefreshView();
		}

		public void RebuildControlView(Dataset dataset)
		{
			var view = GetOrCreateControlView(dataset);
			view.RebuildView();
		}

		private void RefreshOnLocalChange(Locale locale)
		{
			RefreshView();
		}

		public void RefreshView()
		{
			ClearContainers();
			ResetControlsColor();

			for (int i = 0; i < Stock.Count; i++)
			{
				Dataset dataset = Stock[i];
				DatasetElement item = datasetElementsPool.GetPooled();
				item.Set(this, i);
				item.SetEnabled(!Sequence.Contains(dataset));
				datasetContainer.Add(item);
			}

			for (int i = 0; i < Sequence.Count; i++)
			{
				DatasetElement item = datasetElementsPool.GetPooled();
				int id = Stock.IndexOf(Sequence[i]);
				item.Set(this, id);
				sequenceContainer.Add(item);
			}

			SelectDataset(selectedDataset);
		}

		public void SelectDataset(int id)
		{
			if (id == -1)
			{
				//just deselect currently active
				if (selectedDataset != -1)
					DeselectDatasetWithoutNotify();

				ResetControlsColor();
				Signals.Get<DatasetSelectedSignal>().Dispatch(null);
				return;
			}

			Dataset dataset = Stock[id];
			if (selectedDataset == id)
			{
				// new selection is equal to current selection
				// just make sure to show controls if they are not already visible
				if (Sequence.Contains(dataset))
					SetControlsForDataset(dataset);
				else
					SetControlsForDataset(null);
				return;
			}

			if (selectedDataset != -1)
				DeselectDatasetWithoutNotify();

			var element = sequenceContainer.Query<DatasetElement>().Where(de => de.Id == id).First();
			if (element != null)
			{

				element.SetActive(true);

				if (dataset.TryGetData(datasetColorKey, out Color color))
					element.SetColor(color);

				if (Sequence.Contains(dataset))
					SetControlsForDataset(dataset);			
			}

			selectedDataset = id;
			Signals.Get<DatasetSelectedSignal>().Dispatch(dataset);
		}

		private void DeselectDatasetWithoutNotify()
		{
			if (selectedDataset == -1)
				return;

			// deselect currently selected
			sequenceContainer.Query<DatasetElement>().Where(de => de.Id == selectedDataset).ForEach(elem =>
			{
				elem.SetActive(false);
				elem.ResetColor();
			});
			selectedDataset = -1;
			controlsContainer.Clear();
			ResetControlsColor();
		}


		private void SetControlsForDataset(Dataset dataset)
		{
			if (dataset == null)
			{
				controlsContainer.Clear();
				ResetControlsColor();
				return;
			}

			var controlView = GetOrCreateControlView(dataset);
			if (dataset.TryGetData(datasetColorKey, out Color color))
				SetControlsColor(color);
			else
				ResetControlsColor();
			controlsContainer.Add(controlView);
		}

		private void SetControlsColor(Color color)
		{
			controlsContainer.style.borderTopColor = color;
			controlsContainer.style.borderTopWidth = 3f;
		}

		private void ResetControlsColor()
		{
			controlsContainer.style.borderTopColor = StyleKeyword.Null;
			controlsContainer.style.borderTopWidth = StyleKeyword.Null;
			controlsContainer.style.marginTop = StyleKeyword.Null;
		}


		private DatasetControlView GetOrCreateControlView(Dataset dataset)
		{
			if (!Stock.Contains(dataset))
				throw new ArgumentException("The dataset has not been added to the stock.");

			if (!datasetControlsByDataset.TryGetValue(dataset, out DatasetControlView controlView))
			{
				controlView = new DatasetControlView(dataset);
				controlView.RebuildView();
				datasetControlsByDataset.Add(dataset, controlView);
			}

			return controlView;
		}

		private void ClearContainers()
		{
			stockContainer.Query<DatasetElement>().ForEach(item => datasetElementsPool.ReturnToPool(item));
			sequenceContainer.Query<DatasetElement>().ForEach(item => datasetElementsPool.ReturnToPool(item));

			controlsContainer.Clear();
		}
	}
}
