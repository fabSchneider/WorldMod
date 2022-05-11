using Fab.Lua.Core;
using Fab.WorldMod.UI;
using UnityEngine;

namespace Fab.WorldMod.Lua
{
	[LuaName("time")]
	[LuaHelpInfo("Module for accessing time data")]
	public class TimeModule : LuaObject, ILuaObjectInitialize
	{

		UiController uiController;
		public void Initialize()
		{
			uiController = Object.FindObjectOfType<UiController>();

			if (uiController == null)
				throw new LuaObjectInitializationException("No UI module found");
		}

		[LuaHelpInfo("The time at the beginning of this frame")]
		public double time => Time.unscaledTimeAsDouble;

		[LuaHelpInfo("The interval in seconds from the last frame to the current frame")]
		public float delta => Time.unscaledDeltaTime;

		[LuaHelpInfo("The time since the last interaction with the application")]
		public double idle_time => uiController.TimeSinceLastClick;
	}
}
