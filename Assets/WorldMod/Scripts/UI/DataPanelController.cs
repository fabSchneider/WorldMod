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

		private static readonly string containerDropClassname = "container-drop";
		private static readonly string layersDropClassname = "layers-drop";
		private static readonly string insertLineClassname = layersDropClassname + "__line";

		private static readonly string datasetColorKey = "color";

		private VisualElement stockContainer;
		private VisualElement datasetContainer;

		private VisualElement sequenceContainer;
		private VisualElement controlsContainer;

		public VisualElement SequenceContainer => sequenceContainer;
		public VisualElement StockContainer => datasetContainer;
		public VisualElement ControlsContainer => controlsContainer;

		private ObjectPool<DatasetElement> dragItemPool;
		private ObjectPool<LayerDropArea> dragInserAreaPool;

		public DragDrop DragDrop { get; private set; }

		private VisualElement dataPanel;
		public IList<Dataset> Stock { get; private set; }
		public Sequence<Dataset> Sequence { get; private set; }

		private LayerDropArea sequenceDropArea;

		private int activeDataset = -1;

		private Dictionary<Dataset, DatasetControlView> datasetControlsByDataset;

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

			LayerDropArea stockDropArea = new LayerDropArea(DragDrop, HandleStockDrop).WithClass(containerDropClassname);
			stockDropArea.Set(-1);
			stockContainer.Add(stockDropArea);

			sequenceContainer = root.Q(className: sequenceContainerClassname);
			sequenceDropArea = new LayerDropArea(DragDrop, HandleSequenceDrop).WithClass(containerDropClassname);
			sequenceDropArea.Set(0);

			sequenceContainer.RegisterCallback<PointerDownEvent>(evt =>
			{
				DeselectActive();
				Signals.Get<DatasetActivatedSignal>().Dispatch(null);
			});

			controlsContainer = root.Q(className: controlsContainerClassname);

			dragItemPool = new ObjectPool<DatasetElement>(8, true, () => new DatasetElement(LocalizationComponent.Localization), DatasetElement.Reset);
			dragInserAreaPool = new ObjectPool<LayerDropArea>(8, true, CreateLayersDropArea, LayerDropArea.Reset);

			DragDrop.dragStarted += OnDragStarted;

			root.RegisterCallback<FabDragPerformEvent>(OnDropPerformed);
			root.RegisterCallback<FabDragExitedEvent>(OnDragExited);

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

		private void DeselectActive()
		{
			if (activeDataset == -1)
				return;

			// deselect currenty active
			dataPanel.Query<DatasetElement>().Where(de => de.Id == activeDataset).ForEach(elem =>
			{
				elem.SetActive(false);
				elem.ResetColor();
			});
			activeDataset = -1;
			controlsContainer.Clear();
			ResetControlsColor();
		}

		public void SetActiveDatasetElement(DatasetElement element)
		{
			if (element == null)
			{
				//just deselect currently active
				if (activeDataset != -1)
					DeselectActive();

				ResetControlsColor();
				Signals.Get<DatasetActivatedSignal>().Dispatch(null);
				return;
			}

			Dataset dataset = Stock[element.Id];
			if (activeDataset == element.Id)
			{
				// new selection is equal to current selection
				// just make sure to show controls if they are not already visible
				if (Sequence.Contains(dataset))
					SetControlsForDataset(dataset);
				else
					SetControlsForDataset(null);
				return;
			}

			if (activeDataset != -1)
				DeselectActive();

			activeDataset = element.Id;
			element.SetActive(true);

			if (dataset.TryGetData(datasetColorKey, out Color color))
				element.SetColor(color);

			if (Sequence.Contains(dataset))
				SetControlsForDataset(dataset);

			Signals.Get<DatasetActivatedSignal>().Dispatch(dataset);
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


		private bool HandleStockDrop(VisualElement item, LayerDropArea area)
		{
			if (item is DragPreview dragPreview && dragPreview.Owner is DatasetElement datasetElement)
			{
				return Sequence.Remove(Stock[datasetElement.Id]);
			}

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
			ClearContainers();
			ResetControlsColor();

			int lastActive = activeDataset;
			DeselectActive();
			Signals.Get<DatasetActivatedSignal>().Dispatch(null);

			for (int i = 0; i < Stock.Count; i++)
			{
				Dataset dataset = Stock[i];
				DatasetElement item = dragItemPool.GetPooled();
				item.Set(this, i);
				item.SetEnabled(!Sequence.Contains(dataset));
				datasetContainer.Add(item);
			}

			sequenceDropArea.Set(Sequence.Count);
			sequenceContainer.Add(sequenceDropArea);


			LayerDropArea insertArea = dragInserAreaPool.GetPooled();
			insertArea.Set(0);
			sequenceContainer.Add(insertArea);
			for (int i = 0; i < Sequence.Count; i++)
			{
				DatasetElement item = dragItemPool.GetPooled();

				int id = Stock.IndexOf(Sequence[i]);
				item.Set(this, id);

				if (id == lastActive)
				{
					SetActiveDatasetElement(item);
				}

				sequenceContainer.Add(item);
				insertArea = dragInserAreaPool.GetPooled();
				insertArea.Set(i + 1);
				sequenceContainer.Add(insertArea);
			}
		}

		private void ClearContainers()
		{
			stockContainer.Query<DatasetElement>().ForEach(item => dragItemPool.ReturnToPool(item));

			sequenceDropArea.RemoveFromHierarchy();
			LayerDropArea.Reset(sequenceDropArea);
			sequenceContainer.Query<DatasetElement>().ForEach(item => dragItemPool.ReturnToPool(item));
			sequenceContainer.Query<LayerDropArea>().ForEach(item => dragInserAreaPool.ReturnToPool(item));

			controlsContainer.Clear();
		}
	}
}
