using System;
using System.Collections;
using System.Collections.Generic;

namespace Fab.WorldMod
{
	public class DatasetLayers : IEnumerable<Dataset>
	{
		private DatasetStock stock;

		private List<Dataset> layers;
		public Dataset this[int index] => layers[index];
		public int Count => layers.Count;

		public event Action<Dataset, ChangeEventType> layerChanged;

		public enum ChangeEventType
		{
			Added,
			Moved,
			Removed
		}

		public IEnumerator<Dataset> GetEnumerator()
		{
			return layers.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return layers.GetEnumerator();
		}

		public DatasetLayers(DatasetStock stock)
		{
			this.stock = stock;
			layers = new List<Dataset>();
		}
		public void InsertLayer(int itemId, int index)
		{
			if (itemId < 0 || itemId >= stock.Count)
				throw new ArgumentOutOfRangeException(nameof(itemId));

			if (index < 0 || index > layers.Count)
				throw new ArgumentOutOfRangeException(nameof(index));

			Dataset data = stock[itemId];
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

				layerChanged?.Invoke(data, ChangeEventType.Moved);
			}
			else
			{
				layerChanged?.Invoke(data, ChangeEventType.Added);
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
			if (itemId < 0 || itemId >= stock.Count)
				throw new ArgumentOutOfRangeException(nameof(itemId));

			Dataset data = stock[itemId];
			if (layers.Remove(data))
			{
				layerChanged?.Invoke(data, ChangeEventType.Removed);
				return true;
			}
			return false;
		}

		public bool IsLayer(Dataset data)
		{
			return layers.Contains(data);
		}
	}
}
