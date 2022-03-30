using System;
using System.Collections.Generic;

namespace Fab.WorldMod
{
	public class DatasetLayers
	{
		private DatasetStock stock;

		private List<Dataset> layers;
		public IReadOnlyList<Dataset> Datasets => layers;

		public int Count => layers.Count;

		public DatasetLayers(DatasetStock stock)
		{
			this.stock = stock;
			layers = new List<Dataset>();
		}
		public void InsertLayer(int itemId, int index)
		{
			if (itemId < 0 || itemId >= stock.Datasets.Count)
				throw new ArgumentOutOfRangeException(nameof(itemId));

			if (index < 0 || index > layers.Count)
				throw new ArgumentOutOfRangeException(nameof(index));

			Dataset data = stock.Datasets[itemId];
			// do not allow duplicates
			// change existing items position instead
			int existingIndex = layers.IndexOf(data);
			layers.Insert(index, data);

			if (existingIndex != -1)
			{
				if (index <= existingIndex)
					layers.RemoveAt(existingIndex + 1);
				else
					layers.RemoveAt(existingIndex);
			}
		}

		public void InsertLayer(Dataset dataset, int index)
		{
			int id = stock.GetIndex(dataset);
			if(id != -1)
				InsertLayer(id, index);
		} 


		public bool RemoveFromLayers(int itemId)
		{
			if (itemId < 0 || itemId >= stock.Datasets.Count)
				throw new ArgumentOutOfRangeException(nameof(itemId));

			Dataset data = stock.Datasets[itemId];
			return layers.Remove(data);
		}

		public bool IsLayer(Dataset data)
		{
			return layers.Contains(data);
		}
	}
}
