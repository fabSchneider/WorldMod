using System;
using Fab.Common;
using Fab.Lua.Core;
using Fab.WorldMod;
using Fab.WorldMod.Synth;

namespace WorldMod.Lua
{
	[LuaHelpInfo("Module to alter the data layers")]
	[LuaName("layers")]
	public class LayersModule : LuaObject, ILuaObjectInitialize
	{
		private DatasetLayers layers;

		private SynthComponent synthComponent;

		public void Initialize()
		{
			var comp = UnityEngine.Object.FindObjectOfType<DatasetsComponent>();

			if (comp == null)
				throw new LuaObjectInitializationException("Could not find dataset component");

			synthComponent = UnityEngine.Object.FindObjectOfType<SynthComponent>();

			layers = comp.Layers;
		}

		[LuaHelpInfo("Add a dataset as a topmost layer")]
		public void add(DatasetProxy dataset)
		{
			layers.InsertLayer(dataset.Target, layers.Count);
			Signals.Get<DatasetUpdatedSignal>().Dispatch(dataset.Target);
		}

		[LuaHelpInfo("Creates a layer for a specified channel")]
		public SynthLayerProxy create(string channel)
		{
			if (!synthComponent.HasChannel(channel))
				throw new Exception("The specified channel does not exist");

			SynthLayer layer = new SynthLayer(channel);

			var proxy = new SynthLayerProxy(synthComponent.NodeFactory);
			proxy.SetTarget(layer);
			return proxy;
		}
	}
}
