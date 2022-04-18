using System;
using System.Collections;
using System.Collections.Generic;

namespace Fab.WorldMod
{
	public class DatasetSequence : IEnumerable<Dataset>
	{
		private DatasetStock stock;

		private List<Dataset> sequence;
		public Dataset this[int index] => sequence[index];
		public int Count => sequence.Count;

		public event Action<Dataset, ChangeEventType> sequenceChanged;

		public enum ChangeEventType
		{
			Added,
			Moved,
			Removed
		}

		public IEnumerator<Dataset> GetEnumerator()
		{
			return sequence.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return sequence.GetEnumerator();
		}

		public DatasetSequence(DatasetStock stock)
		{
			this.stock = stock;
			sequence = new List<Dataset>();
		}
		public void InsertIntoSequence(int itemId, int index)
		{
			if (itemId < 0 || itemId >= stock.Count)
				throw new ArgumentOutOfRangeException(nameof(itemId));

			if (index < 0 || index > sequence.Count)
				throw new ArgumentOutOfRangeException(nameof(index));

			Dataset data = stock[itemId];
			// do not allow duplicates
			// change existing items position instead
			int existingIndex = sequence.IndexOf(data);
			sequence.Insert(index, data);

			if (existingIndex != -1)
			{
				if (index <= existingIndex)
					sequence.RemoveAt(existingIndex + 1);
				else
					sequence.RemoveAt(existingIndex);

				sequenceChanged?.Invoke(data, ChangeEventType.Moved);
			}
			else
			{
				sequenceChanged?.Invoke(data, ChangeEventType.Added);
			}
		}

		public void InsertIntoSequence(Dataset dataset, int index)
		{
			int id = stock.GetIndex(dataset);
			if(id != -1)
				InsertIntoSequence(id, index);
		}

		public bool RemoveFromSequence(Dataset dataset)
		{
			return RemoveFromSequence(stock.GetIndex(dataset));
		}

		public bool RemoveFromSequence(int itemId)
		{
			if (itemId < 0 || itemId >= stock.Count)
				throw new ArgumentOutOfRangeException(nameof(itemId));

			Dataset data = stock[itemId];
			if (sequence.Remove(data))
			{
				sequenceChanged?.Invoke(data, ChangeEventType.Removed);
				return true;
			}
			return false;
		}

		public bool IsInSequence(Dataset data)
		{
			return sequence.Contains(data);
		}
	}
}
