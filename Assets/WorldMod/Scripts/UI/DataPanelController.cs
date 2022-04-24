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
		private static readonly string filterContainerClassname = className + "__filter-container";
		private static readonly string sequenceContainerClassname = className + "__sequence-container";
		private static readonly string controlsContainerClassname = className + "__controls-container";

		private static readonly string stockDropClassname = "container-drop";
		private static readonly string layersDropClassname = "layers-drop";
		private static readonly string insertLineClassname = layersDropClassname + "__line";

		private VisualElement stockContainer;
		private VisualElement datasetContainer;
		private VisualElement filterContainer;

		private VisualElement sequenceContainer;
		private VisualElement controlsContainer;

		public VisualElement SequenceContainer => sequenceContainer;
		public VisualElement StockContainer => datasetContainer;
		public VisualElement FilterContainer => filterContainer;
		public VisualElement ControlsContainer => controlsContainer;

		private ObjectPool<DatasetElement> dragItemPool;
		private ObjectPool<LayerDropArea> dragInserAreaPool;

		public DragDrop DragDrop { get; private set; }

		private VisualElement dataPanel;
		public IList<Dataset> Stock { get; private set; }
		public Sequence<Dataset> Sequence { get; private set; }


		private LayerDropArea emptySequenceDropArea;

		private DatasetControlView datasetControls;

		private DatasetElement activeDatasetElement;

		private Dictionary<int, DatasetControlView> datasetControlsById;

		public DataPanelController(VisualElement root, IList<Dataset> stock, Sequence<Dataset> sequence)
		{
			dataPanel = root.Q(name: "data-panel");
			Stock = stock;
			Sequence = sequence;

			VisualElement dragLayer = new VisualElement().AsLayer(blocking: false).WithName("drag-layer");
			dragLayer.focusable = true;
			root.Add(dragLayer);
			DragDrop = new DragDrop(dragLayer);

			stockContainer = root.Q(className: stockContainerClassname);

			datasetContainer = stockContainer.Q(className: datasetContainerClassname);

			filterContainer = stockContainer.Q(className: filterContainerClassname);

			LayerDropArea stockDropArea = new LayerDropArea(DragDrop, HandleStockDrop).WithClass(stockDropClassname);
			stockDropArea.Set(-1);
			stockContainer.Add(stockDropArea);

			sequenceContainer = root.Q(className: sequenceContainerClassname);
			emptySequenceDropArea = new LayerDropArea(DragDrop, HandleSequenceDrop).WithClass(stockDropClassname);
			emptySequenceDropArea.Set(0);

			controlsContainer = root.Q(className: controlsContainerClassname);

			dragItemPool = new ObjectPool<DatasetElement>(8, true, () => new DatasetElement(LocalizationComponent.Localization), DatasetElement.Reset);
			dragInserAreaPool = new ObjectPool<LayerDropArea>(8, true, CreateLayersDropArea, LayerDropArea.Reset);

			DragDrop.dragStarted += OnDragStarted;

			root.RegisterCallback<FabDragPerformEvent>(OnDropPerformed);
			root.RegisterCallback<FabDragExitedEvent>(OnDragExited);

			datasetControlsById = new Dictionary<int, DatasetControlView>();
			foreach (Dataset dataset in stock)
				datasetControlsById.Add(stock.IndexOf(dataset), new DatasetControlView(dataset));

			Signals.Get<OnChangeLocaleSignal>().AddListener(RefreshOnLocalChange);

			RefreshView();
		}

		public void RefreshOnLocalChange(Locale locale)
		{
			RefreshView();
		}

		public void SetActiveDatasetElement(DatasetElement element)
		{
			if (activeDatasetElement != element)
			{
				if (activeDatasetElement != null)
				{
					activeDatasetElement.SetActive(false);
					controlsContainer.Clear();
				}
			}

			if (element == null)
			{
				activeDatasetElement = null;
				Signals.Get<DatasetActivatedSignal>().Dispatch(null);
				return;
			}

			activeDatasetElement = element;
			activeDatasetElement.SetActive(true);

			if (Sequence.Contains(Stock[element.Id]))
			{
				controlsContainer.Clear();
				controlsContainer.Add(datasetControlsById[activeDatasetElement.Id]);
			}
			else
				controlsContainer.Clear();

			Signals.Get<DatasetActivatedSignal>().Dispatch(Stock[element.Id]);
		}

		private bool HandleStockDrop(VisualElement item, LayerDropArea area)
		{
			if (item is DragPreview dragPreview && dragPreview.Owner is DatasetElement datasetElement)
				return Sequence.Remove(Stock[datasetElement.Id]);

			return false;
		}

		private LayerDropArea CreateLayersDropArea()
		{
			LayerDropArea area = new LayerDropArea(DragDrop, HandleSequenceDrop);
			area.AddToClassList(layersDropClassname);
			VisualElement insertLine = new VisualElement();
			insertLine.AddToClassList(insertLineClassname);
			insertLine.pickingMode = PickingMode.Ignore;
			area.Add(insertLine);
			return area;
		}

		private bool HandleSequenceDrop(VisualElement item, LayerDropArea area)
		{
			if (item is DragPreview dragPreview && dragPreview.Owner is DatasetElement datasetElement)
			{
				Sequence.Insert(Stock[datasetElement.Id], area.Index);
				return true;
			}

			return false;
		}

		private void OnDragStarted()
		{
			//enable all drop areas
			dataPanel.Query<LayerDropArea>().ForEach(a => a.SetEnabled(true));
		}


		private void OnDropPerformed(FabDragPerformEvent evt)
		{
			dataPanel.Query<LayerDropArea>().ForEach(a => a.SetEnabled(false));
			RefreshView();
		}

		private void OnDragExited(FabDragExitedEvent evt)
		{
			dataPanel.Query<LayerDropArea>().ForEach(a => a.SetEnabled(false));
		}

		private static readonly string datasetTypeKey = "type";
		private static readonly string datasetFiltereKey = "filter";

		public void RefreshView()
		{
			Debug.Log("Refresh View");

			ClearContainers();

			for (int i = 0; i < Stock.Count; i++)
			{
				Dataset dataset = Stock[i];
				DatasetElement item = dragItemPool.GetPooled();
				item.Set(this, i);
				item.SetEnabled(!Sequence.Contains(dataset));

				if (dataset.TryGetData(datasetTypeKey, out string type) &&
					type.Equals(datasetFiltereKey, StringComparison.CurrentCultureIgnoreCase))
				{
					filterContainer.Add(item);
				}
				else
				{
					datasetContainer.Add(item);
				}

			}

			if (Sequence.Count == 0)
			{
				emptySequenceDropArea.Set(0);
				sequenceContainer.Add(emptySequenceDropArea);
			}
			else
			{
				LayerDropArea insertArea = dragInserAreaPool.GetPooled();
				insertArea.Set(0);
				sequenceContainer.Add(insertArea);
				for (int i = 0; i < Sequence.Count; i++)
				{
					DatasetElement item = dragItemPool.GetPooled();

					item.Set(this, Stock.IndexOf(Sequence[i]));
					sequenceContainer.Add(item);
					insertArea = dragInserAreaPool.GetPooled();
					insertArea.Set(i + 1);
					sequenceContainer.Add(insertArea);
				}
			}
		}

		private void ClearContainers()
		{
			stockContainer.Query<DatasetElement>().ForEach(item => dragItemPool.ReturnToPool(item));

			emptySequenceDropArea.RemoveFromHierarchy();
			LayerDropArea.Reset(emptySequenceDropArea);
			sequenceContainer.Query<DatasetElement>().ForEach(item => dragItemPool.ReturnToPool(item));
			sequenceContainer.Query<LayerDropArea>().ForEach(item => dragInserAreaPool.ReturnToPool(item));

			controlsContainer.Clear();
		}
	}
}
