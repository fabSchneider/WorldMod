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

		[LuaHelpInfo("Adds a dataset to the list of available datasets")]
		public void add(string name, Coordinate coord)
		{
			WorldFeature feature = new WorldFeature(coord);
			feature.SetData("name", name);
			featuresComponent.FeatureCollection.Features.Add(feature);
		}
	}
}
