using Fab.Common;

namespace Fab.WorldMod
{
	public class DatasetUpdatedSignal : ASignal<Dataset> { }

	public class Dataset
	{
		private DatasetStock owner;
		public DatasetStock Owner => owner;

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

		public Dataset(string name, DatasetStock owner)
		{
			this.name = name;
			this.owner = owner;
		}
	}
}
