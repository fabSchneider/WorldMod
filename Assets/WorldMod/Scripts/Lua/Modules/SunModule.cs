using System.Collections;
using System.Collections.Generic;
using Fab.Geo;
using Fab.Lua.Core;
using UnityEngine;

namespace Fab.WorldMod.Lua
{
	[LuaHelpInfo("Module to alter the sun position")]
	[LuaName("sun")]
	public class SunModule : LuaObject, ILuaObjectInitialize
	{
		private SunController sun;
		public void Initialize()
		{
			sun = Object.FindObjectOfType<SunController>();
			if (sun == null)
				throw new LuaObjectInitializationException("Sun controller was not found.");
		}

		public float x_pos => sun.SunX;
		public float y_pos => sun.SunY;

		[LuaHelpInfo("Returns the coordinate that is currently in zenith")]
		public Coordinate zenith => sun.Zenith;

		[LuaHelpInfo("Binds the suns position to the current view")]
		public void follow_view()
		{
			sun.FollowCamera = true;
		}

		public void set_x(float x)
		{
			sun.SunX = x;
		}

		public void set_y(float y)
		{
			sun.SunY = y;
		}

    }
}
