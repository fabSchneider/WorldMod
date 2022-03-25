using System;
using System.Collections.Generic;
using Fab.Common;

namespace Fab.WorldMod
{
	public class DatasetUpdatedSignal : ASignal<Dataset> { }

	public class Dataset
	{
		private Datasets owner;
		public Datasets Owner => owner;

		private string name;
		public string Name
		{
			get => name;
			set
			{
				name = value;
				Signals.Get<DatasetUpdatedSignal>().Dispatch(this);
			}
		}

		public Dataset(string name, Datasets owner)
		{
			this.name = name;
			this.owner = owner;
		}
	}

	public class Datasets
	{
		private List<Dataset> stock;
		private List<Dataset> sequence;

		public IReadOnlyList<Dataset> Stock => stock;

		public IReadOnlyList<Dataset> Sequence => sequence;

		public Datasets()
		{
			stock = new List<Dataset>();
			sequence = new List<Dataset>();
		}

		public Dataset AddDataset(string name)
		{
			Dataset ds = new Dataset(name, this);
			stock.Add(ds);
			Signals.Get<DatasetUpdatedSignal>().Dispatch(ds);
			return ds;
		}

		public int GetIndex(Dataset dataset)
		{
			return stock.IndexOf(dataset);
		}

		public bool InsertIntoSequence(int itemId, int index)
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
			}

			return true;
		}

		public bool RemoveFromSequence(int itemId)
		{
			if (itemId < 0 || itemId >= stock.Count)
				throw new ArgumentOutOfRangeException(nameof(itemId));

			Dataset data = stock[itemId];
			return sequence.Remove(data);
		}

		public bool IsInSequence(Dataset data)
		{
			return sequence.Contains(data);
		}
	}
}
