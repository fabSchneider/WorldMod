using System.Collections.Generic;
using System.Linq;
using Fab.Common;
using Fab.Lua.Core;

namespace Fab.WorldMod.Lua
{
	[LuaHelpInfo("Module to access the world mod datasets")]
	[LuaName("datasets")]
	public class DatasetModule : LuaObject, ILuaObjectInitialize
	{
		private IList<Dataset> stock;

		public void Initialize()
		{
			var comp = UnityEngine.Object.FindObjectOfType<DatasetsComponent>();

			if (comp == null)
				throw new LuaObjectInitializationException("Could not find dataset component");

			stock = comp.Stock;
		}

		[LuaHelpInfo("Adds a dataset to the list of available datasets")]
		public DatasetProxy add(string name)
		{
			Dataset dataset = new Dataset(name);
			stock.Add(dataset);
			Signals.Get<DatasetUpdatedSignal>().Dispatch(dataset);
			return new DatasetProxy(dataset);
		}

		[LuaHelpInfo("Returns a collection of all datasets")]
		public DatasetProxy[] all()
		{
			return stock.Select(ds => new DatasetProxy(ds)).ToArray();
		}

		[LuaHelpInfo("Gets the dataset with the specified name")]
		public DatasetProxy get(string name)
		{
			Dataset dataset = stock.FirstOrDefault(ds => ds.Name == name);
			return new DatasetProxy(dataset);
		}
	}
}
