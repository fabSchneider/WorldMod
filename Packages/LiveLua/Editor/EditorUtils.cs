using Fab.Lua.Core;
using System.Diagnostics;
using System.IO;
using UnityEditor;


namespace Fab.Lua.Editor
{
	public static class EditorUtils
	{
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

        [MenuItem("Lua/Open scripts folder")]
        public static void OpenScriptFolder()
        {
            if (!Directory.Exists(LuaEnvironment.ScriptsDirectory))
                return;

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                Arguments = LuaEnvironment.ScriptsDirectory,
                FileName = "explorer.exe"
            };

            Process.Start(startInfo);
        }

        [MenuItem("Lua/Open data folder")]
        public static void OpenDataFolder()
        {
            if (!Directory.Exists(LuaEnvironment.DataDirectory))
                return;

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                Arguments = LuaEnvironment.DataDirectory,
                FileName = "explorer.exe"
            };

            Process.Start(startInfo);
        }

#endif


	}
}
