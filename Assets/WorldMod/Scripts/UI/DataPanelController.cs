using System.Collections.Generic;
using Fab.Common;
using Fab.WorldMod.Localization;
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
		private static readonly string layersContainerClassname = className + "__layers-container";
		private static readonly string containerDropClassname = "container-drop";
		private static readonly string layersDropClassname = "layers-drop";
		private static readonly string insertLineClassname = layersDropClassname + "__line";

		private VisualElement stockContainer;
		private VisualElement layersContainer;

		public VisualElement StockContainer => stockContainer;
		public VisualElement LayersContainer => layersContainer;

		private ObjectPool<DatasetElement> dragItemPool;
		private ObjectPool<LayerDropArea> dragInserAreaPool;

		public DragDrop DragDrop { get; private set; }

		private VisualElement dataPanel;
		public DatasetStock Stock { get; private set; }
		public DatasetSequence Sequence { get; private set; }

		private LayerDropArea layersContainerDropArea;

		private DatasetControlView datasetControls;

		private DatasetElement activeDatasetElement;

		private Dictionary<int, DatasetControlView> datasetControlsById;

		public DataPanelController(VisualElement root, DatasetStock stock, DatasetSequence layers)
		{
			dataPanel = root.Q(name: "data-panel");
			Stock = stock;
			Sequence = layers;

			VisualElement dragLayer = new VisualElement().AsLayer(blocking: false).WithName("drag-layer");
			dragLayer.focusable = true;
			root.Add(dragLayer);
			DragDrop = new DragDrop(dragLayer);

			stockContainer = root.Q(className: stockContainerClassname);
			LayerDropArea stockDropArea = new LayerDropArea(DragDrop, HandleStockDrop).WithClass(containerDropClassname);
			stockDropArea.Set(-1);
			stockContainer.Add(stockDropArea);

			layersContainer = root.Q(className: layersContainerClassname);
			layersContainerDropArea = new LayerDropArea(DragDrop, HandleLayersDrop).WithClass(containerDropClassname);
			layersContainerDropArea.Set(0);

			dragItemPool = new ObjectPool<DatasetElement>(8, true, () => new DatasetElement(LocalizationComponent.Localization), DatasetElement.Reset);
			dragInserAreaPool = new ObjectPool<LayerDropArea>(8, true, CreateLayersDropArea, LayerDropArea.Reset);

			root.RegisterCallback<FabDragPerformEvent>(OnDropPerformed);

			datasetControlsById = new Dictionary<int, DatasetControlView>();
			foreach (Dataset dataset in stock)
				datasetControlsById.Add(stock.GetIndex(dataset), new DatasetControlView(dataset));

			RefreshView();
		}

		public void SetActiveDatasetElement(DatasetElement element)
		{
			if (activeDatasetElement != element)
			{
				if (activeDatasetElement != null)
				{
					activeDatasetElement.SetActive(false);
					datasetControlsById[activeDatasetElement.Id].Hide();
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

			if (Sequence.IsInSequence(Stock[element.Id]))
				datasetControlsById[activeDatasetElement.Id].Show(activeDatasetElement);
			else
				datasetControlsById[activeDatasetElement.Id].Hide();

			Signals.Get<DatasetActivatedSignal>().Dispatch(Stock[element.Id]);
		}

		private bool HandleStockDrop(VisualElement item, LayerDropArea area)
		{
			if (item is DatasetElement.DragPreview dragPreview)
				return Sequence.RemoveFromSequence(dragPreview.DatasetElement.Id);

			return false;
		}

		private LayerDropArea CreateLayersDropArea()
		{
			LayerDropArea area = new LayerDropArea(DragDrop, HandleLayersDrop);
			area.AddToClassList(layersDropClassname);
			VisualElement insertLine = new VisualElement();
			insertLine.AddToClassList(insertLineClassname);
			insertLine.pickingMode = PickingMode.Ignore;
			area.Add(insertLine);
			return area;
		}

		private bool HandleLayersDrop(VisualElement item, LayerDropArea area)
		{
			if (item is DatasetElement.DragPreview dragPreview)
			{
				Sequence.InsertIntoSequence(dragPreview.DatasetElement.Id, area.Index);
				return true;
			}

			return false;
		}

		private void OnDropPerformed(FabDragPerformEvent evt)
		{
			RefreshView();
		}

		public void RefreshView()
		{
			ClearContainers();

			for (int i = 0; i < Stock.Count; i++)
			{
				DatasetElement item = dragItemPool.GetPooled();
				item.Set(this, i);
				item.SetEnabled(!Sequence.IsInSequence(Stock[i]));
				stockContainer.Add(item);
			}

			if (Sequence.Count == 0)
			{
				layersContainerDropArea.Set(0);
				layersContainer.Add(layersContainerDropArea);
			}
			else
			{
				LayerDropArea insertArea = dragInserAreaPool.GetPooled();
				insertArea.Set(0);
				layersContainer.Add(insertArea);
				for (int i = 0; i < Sequence.Count; i++)
				{
					DatasetElement item = dragItemPool.GetPooled();

					item.Set(this, Stock.GetIndex(Sequence[i]));
					layersContainer.Add(item);
					insertArea = dragInserAreaPool.GetPooled();
					insertArea.Set(i + 1);
					layersContainer.Add(insertArea);
				}
			}
		}

		private void ClearContainers()
		{
			stockContainer.Query<DatasetElement>().ForEach(item => dragItemPool.ReturnToPool(item));

			layersContainerDropArea.RemoveFromHierarchy();
			LayerDropArea.Reset(layersContainerDropArea);
			layersContainer.Query<DatasetElement>().ForEach(item => dragItemPool.ReturnToPool(item));
			layersContainer.Query<LayerDropArea>().ForEach(item => dragInserAreaPool.ReturnToPool(item));
		}
	}
}
