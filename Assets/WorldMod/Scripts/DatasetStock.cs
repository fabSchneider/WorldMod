using System.Collections.Generic;
using Fab.Common;

namespace Fab.WorldMod
{
	public class DatasetStock 
	{
		private List<Dataset> datasets;

		public IReadOnlyList<Dataset> Datasets => datasets;

		public int Count => datasets.Count;

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
