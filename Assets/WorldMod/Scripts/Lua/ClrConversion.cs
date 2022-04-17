using MoonSharp.Interpreter;

namespace WorldMod.Lua
{
    public static class ClrConversion 
    {
		public static void RegisterConverters()
		{
			Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(LuaNodeProperty), ScriptToNodeProperty);
		}

		static object ScriptToNodeProperty(DynValue dynVal)
		{
			Table table = dynVal.Table;
			string name = table.Get(1).String;
			DynValue val = table.Get(2);
			return new LuaNodeProperty(name, val);
		}
	}
}
