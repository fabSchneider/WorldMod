using Fab.Lua.Core;
using Fab.WorldMod.UI;

namespace Fab.WorldMod.Lua
{
	[LuaHelpInfo("Module to print messages to the log")]
	[LuaName("log")]
	public class LogModule : LuaObject, ILuaObjectInitialize
	{

		private UiController ui;

		public void Initialize()
		{
			ui = UnityEngine.Object.FindObjectOfType<UiController>();

			if (ui == null)
				throw new LuaObjectInitializationException("Ui controller was not found.");

		}

		[LuaHelpInfo("Prints a messages to the log")]
		public void print(string message)
		{
			ui.LogOutput.Log(message);
		}

		[LuaHelpInfo("Clears the log")]
		public void clear()
		{
			ui.LogOutput.Clear();
		}
	}
}
