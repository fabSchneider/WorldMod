using Fab.Lua.Core;
using UnityEngine;

namespace WorldMod.Lua
{
	[LuaName("time")]
	[LuaHelpInfo("Module for accessing time data")]
	public class TimeModule : LuaObject, ILuaObjectInitialize
	{
		public void Initialize()
		{
			
		}

		[LuaHelpInfo("The time at the beginning of this frame")]
		public float time => Time.time;

		[LuaHelpInfo("The interval in seconds from the last frame to the current frame")]
		public float delta => Time.deltaTime;
	}
}
