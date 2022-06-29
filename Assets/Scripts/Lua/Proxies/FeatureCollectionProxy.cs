using Fab.Geo;
using Fab.Lua.Core;

namespace Fab.WorldMod.Lua
{
	[LuaHelpInfo("A feature collection")]
	[LuaName("feature_collection")]
	public class FeatureCollectionProxy : LuaProxy<WorldFeatureCollection>
	{
		private static readonly string nameKey = "name";

		[LuaHelpInfo("Adds a point to the feature collection")]
		public void point(string name, Coordinate coord)
		{
			WorldFeature feature = new WorldFeature(coord);
			feature.SetData(nameKey, name);
			target.Add(feature);
		}
	}
}
