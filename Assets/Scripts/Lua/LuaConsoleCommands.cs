using UnityEngine;
using Fab.Lua.Console;
using CommandTerminal;

namespace Fab.WorldMod.Lua
{
	public class LuaConsoleCommands : MonoBehaviour
	{
		[SerializeField]
		private GameObject consolePrefab;

		ConsoleComponent consoleComp;
		ConsoleView consoleView;

		private GameObject consoleInstance;

		private void Start()
		{
			Terminal.Shell.AddCommand("live_coding", EnableLiveCodingCmd, 1, 1);
		}

		private void EnableLiveCodingCmd(CommandArg[] args)
		{
			if (args.Length == 0)
				return;

			bool enable = args[0].Bool;

			if (enable)
				AddConsole();
			else
				RemoveConsole();
		}

		private void AddConsole()
		{
			consoleInstance = Instantiate(consolePrefab, transform);

			consoleComp = consoleInstance.GetComponent<ConsoleComponent>();
			consoleView = GetComponent<ConsoleView>();
		}

		private void RemoveConsole()
		{
			Destroy(consoleInstance);
			consoleComp = null;
			consoleView = null;
		}
	}
}
