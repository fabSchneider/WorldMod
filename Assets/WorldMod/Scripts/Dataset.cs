using System;
using System.Collections.Generic;
using Fab.Common;

namespace Fab.WorldMod
{
	public class DatasetUpdatedSignal : ASignal<Dataset> { }
	public class DatasetActivatedSignal : ASignal<Dataset> { }
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

		private Dictionary<string, object> dataDict;

		public IEnumerable<string> DataKeys => dataDict.Keys;

		public Dataset(string name, DatasetStock owner)
		{
			this.name = name;
			this.owner = owner;
			dataDict = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
		}

		public void SetData(string key, object data)
		{
			if (!dataDict.TryAdd(key, data))
			{
				dataDict[key] = data;
			}
		}

		public bool TryGetData<T>(string key, out T data)
		{
			if(dataDict.TryGetValue(key, out object obj))
			{
				if (obj is T objT)
				{
					data = objT;
					return true;
				}
			}
			data = default(T);
			return false;
		}
	}
}
