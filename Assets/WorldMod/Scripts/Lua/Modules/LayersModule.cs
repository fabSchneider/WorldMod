using Fab.Common;
using Fab.Lua.Core;
using Fab.WorldMod;

namespace WorldMod.Lua
{
	[LuaHelpInfo("Module to alter the data layers")]
	[LuaName("layers")]
	public class LayersModule : LuaObject, ILuaObjectInitialize
	{
		private DatasetLayers layers;
		private LayersManager layersManager;

		public void Initialize()
		{
			var comp = UnityEngine.Object.FindObjectOfType<DatasetsComponent>();

			if (comp == null)
				throw new LuaObjectInitializationException("Could not find dataset component");

			layersManager = UnityEngine.Object.FindObjectOfType<LayersManager>();

			layers = comp.Layers;
		}

		[LuaHelpInfo("Add a dataset as a topmost layer")]
		public void add(DatasetProxy dataset)
		{
			layers.InsertLayer(dataset.Target, layers.Count);
			Signals.Get<DatasetUpdatedSignal>().Dispatch(dataset.Target);
		}

		public void update()
		{
			if (layersManager)
				layersManager.UpdateLayers();
		}
	}
}
