using System;
using Fab.Lua.Core;
using Fab.Synth;

namespace Fab.WorldMod.Lua
{
	[LuaHelpInfo("Module to alter the data layers")]
	[LuaName("synth")]
	public class SynthModule : LuaObject, ILuaObjectInitialize
	{
		private SynthComponent synthComponent;

		public void Initialize()
		{
			synthComponent = UnityEngine.Object.FindObjectOfType<SynthComponent>();

			if (synthComponent == null)
				throw new LuaObjectInitializationException("Could not find synth component");
		}

		[LuaHelpInfo("Creates a synth layer for a specified channel")]
		public SynthLayerProxy create(string channel)
		{
			if (!synthComponent.HasChannel(channel))
				throw new Exception("The specified channel does not exist");

			SynthLayer layer = new SynthLayer(channel);

			var proxy = new SynthLayerProxy(synthComponent.NodeFactory);
			proxy.SetTarget(layer);
			return proxy;
		}

		[LuaHelpInfo("Tells the synthesizer to update the specified channel for the next frame")]
		public void update(string channel)
		{
			synthComponent.SetChannelDirty(channel);
		}
	}
}
