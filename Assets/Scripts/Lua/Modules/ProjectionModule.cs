using Fab.Lua.Core;
using UnityEngine;

namespace Fab.WorldMod.Lua
{

	[LuaHelpInfo("Module to manipulate the projection")]
	[LuaName("projection")]
	public class ProjectionModule : LuaObject, ILuaObjectInitialize
	{
		private ProjectionController projection;

		public void Initialize()
		{
			projection = Object.FindObjectOfType<ProjectionController>();

			if (projection == null)
				throw new LuaObjectInitializationException("Could not find projection controller");
		}

		[LuaHelpInfo("Changes the scale of the projection")]
		public void scale(float scale)
		{
			projection.SetProjection(projection.Offset, scale);
		}

		[LuaHelpInfo("Changes the x offset of the projection")]
		public void offset_x(float x)
		{
			projection.SetProjection(new Vector2(x, projection.Offset.y), projection.Scale);
		}

		[LuaHelpInfo("Changes the x offset of the projection")]
		public void offset_y(float y)
		{
			projection.SetProjection(new Vector2(projection.Offset.x, y), projection.Scale);
		}
	}
}
