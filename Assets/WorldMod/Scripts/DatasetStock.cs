using System.Collections;
using System.Collections.Generic;
using Fab.Common;

namespace Fab.WorldMod
{
	public class DatasetStock : IEnumerable<Dataset>
	{
		private List<Dataset> datasets;

		public int Count => datasets.Count;

		public Dataset this[int index] => datasets[index];
		public IEnumerator<Dataset> GetEnumerator()
		{
			return datasets.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return datasets.GetEnumerator();
		}

		public DatasetStock()
		{
			datasets = new List<Dataset>();
		}

		public Dataset AddDataset(string name)
		{
			Dataset ds = new Dataset(name, this);
			datasets.Add(ds);
			Signals.Get<DatasetUpdatedSignal>().Dispatch(ds);
			return ds;
		}

		public int GetIndex(Dataset dataset)
		{
			return datasets.IndexOf(dataset);
		}
	}
}
