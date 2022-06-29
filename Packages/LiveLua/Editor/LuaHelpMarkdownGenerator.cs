using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Fab.Lua.Core;
using MoonSharp.Interpreter.Interop;
using System;

namespace Fab.Lua.Editor
{
	public class LuaHelpMarkdownGenerator
	{
		/// <summary>
		/// Generates the Lua Help Markdown file and prompts the save dialog
		/// </summary>
		[MenuItem("Lua/Generate Help Docs")]
		public static void GenerateAndSaveHelp()
		{
			LuaHelpMarkdownFormatter formatter = new LuaHelpMarkdownFormatter();
			LuaHelpInfoCache extactor = new LuaHelpInfoCache();

			StringBuilder sb = new StringBuilder();

			foreach (StandardUserDataDescriptor descriptor in LuaEnvironment.Registry.GetRegisteredTypes(true))
			{
				LuaHelpInfo info = extactor.GetHelpInfoForUserData(descriptor);

				sb.Append(formatter.Format(info));
			}

			string path = EditorUtility.SaveFilePanel("Save Help Doc", "", "LuaHelp.md", "md");

			if (path.Length != 0)
			{
				File.WriteAllText(path, sb.ToString());
				Debug.Log("Lua Help Documentation successfully written to " + path);
			}
		}
	}
}
