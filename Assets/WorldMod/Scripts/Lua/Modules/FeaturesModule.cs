using Fab.Geo;
using Fab.Lua.Core;
using UnityEngine;

namespace Fab.WorldMod.Lua
{
	[LuaHelpInfo("Module to control the world features")]
	[LuaName("features")]
	public class FeaturesModule : LuaObject, ILuaObjectInitialize
	{
		private WorldFeaturesComponent featuresComponent;

		public void Initialize()
		{
			featuresComponent = Object.FindObjectOfType<WorldFeaturesComponent>();
			if (featuresComponent == null)
				throw new LuaObjectInitializationException("Missing world feature component");
		}

		[LuaHelpInfo("Creates a feature collection")]
		public FeatureCollectionProxy create(string name)
		{
			WorldFeatureCollection features = new WorldFeatureCollection(name);
			FeatureCollectionProxy proxy = new FeatureCollectionProxy();
			proxy.SetTarget(features);
			return proxy;
		}

		[LuaHelpInfo("Adds a feature collection to the world")]
		public void add(FeatureCollectionProxy features)
		{
			featuresComponent.AddFeatures(features.Target);
		}

		[LuaHelpInfo("Adds a feature collection to the world")]
		public void remove(FeatureCollectionProxy features)
		{
			featuresComponent.RemoveFeatures(features.Target);
		}
	}
}
