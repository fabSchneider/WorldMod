using System;
using System.Collections;
using System.Collections.Generic;

namespace Fab.WorldMod
{
	public enum SequenceChangeEventType
	{
		Added,
		Moved,
		Removed
	}

	public class Sequence<T> : IEnumerable<T>
	{
		protected List<T> sequence;
		public T this[int index] => sequence[index];
		public int Count => sequence.Count;

		public event Action<SequenceChangedEvent<T>> sequenceChanged;

		public IEnumerator<T> GetEnumerator()
		{
			return sequence.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return sequence.GetEnumerator();
		}

		public Sequence()
		{
			sequence = new List<T>();
		}

		public void Insert(T data, int index)
		{

			if (index < 0 || index > sequence.Count)
				throw new ArgumentOutOfRangeException(nameof(index));

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
				sequenceChanged?.Invoke(new SequenceChangedEvent<T>(this, data, SequenceChangeEventType.Moved, existingIndex));
			}
			else
			{
				sequenceChanged?.Invoke(new SequenceChangedEvent<T>(this, data, SequenceChangeEventType.Added, -1));
			}
		}

		public bool Remove(T data)
		{
			int index = sequence.IndexOf(data);

			if (sequence.Remove(data))
			{
				sequenceChanged?.Invoke(new SequenceChangedEvent<T>(this, data, SequenceChangeEventType.Removed, index));
				return true;
			}
			return false;
		}

		public bool Contains(T data)
		{
			return sequence.Contains(data);
		}

		public int IndexOf(T data)
		{
			return sequence.IndexOf(data);
		}
	}

	public readonly struct SequenceChangedEvent<T>
	{
		public readonly Sequence<T> sequence;
		public readonly T data;
		public readonly SequenceChangeEventType changeType;
		public readonly int lastIndex;

		public SequenceChangedEvent(Sequence<T> sequence, T data, SequenceChangeEventType changeType, int lastIndex)
		{
			this.sequence = sequence;
			this.data = data;
			this.changeType = changeType;
			this.lastIndex = lastIndex;
		}
	}

	public class DatasetGroup : IEnumerable<Dataset>
	{
		private List<Dataset> datasets;

		public Dataset BaseDataset => datasets[0];


		public DatasetGroup(Dataset baseDataset)
		{
			this.datasets = new List<Dataset>() { baseDataset };
		}

		public void AddFilter(Dataset filter)
		{
			datasets.Add(filter);
		}

		public bool RemoveFilter(Dataset filter)
		{
			if (datasets.Count > 1)
				return datasets.Remove(filter);
			return false;
		}

		public IEnumerator<Dataset> GetEnumerator()
		{
			return datasets.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return datasets.GetEnumerator();
		}
	}

	public class DatasetGroupSequence : Sequence<DatasetGroup>
	{
		public bool IsBaseInSequence(Dataset dataset)
		{
			foreach (var group in sequence)
			{
				if (group.BaseDataset == dataset)
					return true;
			}
			return false;
		}

		public bool IsInSequence(Dataset dataset)
		{
			foreach (var group in sequence)
			{
				if (group.BaseDataset == dataset)
					return true;
			}
			return false;
		}
	}

}
