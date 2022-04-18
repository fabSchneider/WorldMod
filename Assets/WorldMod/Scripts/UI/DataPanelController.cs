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
		public DatasetSequence Layers { get; private set; }

		private LayerDropArea layersContainerDropArea;

		private DatasetControlView datasetControls;

		private DatasetElement activeDataset;

		public DataPanelController(VisualElement root, DatasetStock stock, DatasetSequence layers)
		{
			dataPanel = root.Q(name: "data-panel");
			Stock = stock;
			Layers = layers;

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

			datasetControls = new DatasetControlView(stock);

			RefreshView();
		}

		public void SetActiveDatasetElement(DatasetElement element)
		{
			if (activeDataset != element)
				activeDataset?.SetActive(false);

			if (element == null)
			{
				activeDataset = null;
				datasetControls.SetDatasetItem(null);
				Signals.Get<DatasetActivatedSignal>().Dispatch(null);
				return;
			}

			activeDataset = element;
			activeDataset.SetActive(true);
			Signals.Get<DatasetActivatedSignal>().Dispatch(Stock[element.Id]);

			if (Layers.IsInSequence(Stock[element.Id]))
				datasetControls.SetDatasetItem(element);
			else
				datasetControls.SetDatasetItem(null);
		}

		private bool HandleStockDrop(VisualElement item, LayerDropArea area)
		{
			if (item is DatasetElement.DragPreview dragPreview)
				return Layers.RemoveFromSequence(dragPreview.DatasetElement.Id);

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
				Layers.InsertIntoSequence(dragPreview.DatasetElement.Id, area.Index);
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
				item.SetEnabled(!Layers.IsInSequence(Stock[i]));
				stockContainer.Add(item);
			}

			if (Layers.Count == 0)
			{
				layersContainerDropArea.Set(0);
				layersContainer.Add(layersContainerDropArea);
			}
			else
			{
				LayerDropArea insertArea = dragInserAreaPool.GetPooled();
				insertArea.Set(0);
				layersContainer.Add(insertArea);
				for (int i = 0; i < Layers.Count; i++)
				{
					DatasetElement item = dragItemPool.GetPooled();
					
					item.Set(this, Stock.GetIndex(Layers[i]));
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
