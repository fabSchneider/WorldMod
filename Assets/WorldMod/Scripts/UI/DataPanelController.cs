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

		private ObjectPool<DatasetItem> dragItemPool;
		private ObjectPool<LayerDropArea> dragInserAreaPool;

		public DragDrop DragDrop { get; private set; }

		private VisualElement dataPanel;
		public DatasetStock Stock { get; private set; }
		public DatasetLayers Layers { get; private set; }

		private LayerDropArea layersContainerDropArea;

		private DatasetControls datasetControls;

		public DataPanelController(VisualElement root, DatasetStock stock, DatasetLayers layers)
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

			dragItemPool = new ObjectPool<DatasetItem>(8, true, () => new DatasetItem(LocalizationComponent.Localization), DatasetItem.Reset);
			dragInserAreaPool = new ObjectPool<LayerDropArea>(8, true, CreateLayersDropArea, LayerDropArea.Reset);

			root.RegisterCallback<FabDragPerformEvent>(OnDropPerformed);

			datasetControls = new DatasetControls(stock);

			dataPanel.RegisterCallback<FocusInEvent>(OnFocus);

			RefreshView();
		}

		private void OnFocus(FocusInEvent evt)
		{
			if (evt.target is DatasetItem datasetItem)
			{
				Signals.Get<DatasetActivatedSignal>().Dispatch(Stock[datasetItem.Id]);

				if (Layers.IsLayer(Stock[datasetItem.Id]))
					datasetControls.SetDatasetItem(datasetItem);
				else
					datasetControls.SetDatasetItem(null);
			}
			else
			{
				datasetControls.SetDatasetItem(null);
				Signals.Get<DatasetActivatedSignal>().Dispatch(null);
			}
		}

		private bool HandleStockDrop(VisualElement item, LayerDropArea area)
		{
			if (item is DatasetItem.DragPreview dragPreview)
				return Layers.RemoveFromLayers(dragPreview.DragItem.Id);

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
			if (item is DatasetItem.DragPreview dragPreview)
			{
				Layers.InsertLayer(dragPreview.DragItem.Id, area.Index);
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
				DatasetItem item = dragItemPool.GetPooled();
				item.Set(this, i);
				item.SetEnabled(!Layers.IsLayer(Stock[i]));
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
					DatasetItem item = dragItemPool.GetPooled();
					
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
			stockContainer.Query<DatasetItem>().ForEach(item => dragItemPool.ReturnToPool(item));

			layersContainerDropArea.RemoveFromHierarchy();
			LayerDropArea.Reset(layersContainerDropArea);
			layersContainer.Query<DatasetItem>().ForEach(item => dragItemPool.ReturnToPool(item));
			layersContainer.Query<LayerDropArea>().ForEach(item => dragInserAreaPool.ReturnToPool(item));
		}
	}
}
