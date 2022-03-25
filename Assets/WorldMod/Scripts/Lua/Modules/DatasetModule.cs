using System.Linq;
using Fab.Lua.Core;
using Fab.WorldMod;

namespace WorldMod.Lua
{
	[LuaHelpInfo("Module to access the world mod datasets")]
	[LuaName("datasets")]
	public class DatasetModule : LuaObject, ILuaObjectInitialize
	{
		private Datasets datasets;

		public void Initialize()
		{
			var comp = UnityEngine.Object.FindObjectOfType<DatasetsComponent>();

			if (comp == null)
				throw new LuaObjectInitializationException("Could not find dataset component");

			datasets = comp.Datasets;
		}

		[LuaHelpInfo("Adds a dataset to the list of available datasets")]
		public DatasetProxy add(string name)
		{
			Dataset dataset =  datasets.AddDataset(name);
			return new DatasetProxy(dataset);
		}

		[LuaHelpInfo("Returns a collection of all datasets")]
		public DatasetProxy[] all()
		{
			return datasets.Stock.Select(ds => new DatasetProxy(ds)).ToArray();
		}

		[LuaHelpInfo("Finds the dataset with the specified name")]
		public DatasetProxy find(string name)
		{
			Dataset dataset = datasets.Stock.FirstOrDefault(ds => ds.Name == name);
			return new DatasetProxy(dataset);
		}
	}
}
