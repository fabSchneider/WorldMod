using Fab.Lua.Core;
using MoonSharp.Interpreter;
using UnityEngine;

namespace WorldMod.Lua
{
	internal static class RegisterAssembly
	{
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
		private static void Register()
		{
			UserData.RegisterAssembly();
			LuaEnvironment.Registry.RegisterAssembly();
			ClrConversion.RegisterConverters();
		}
	}
}
