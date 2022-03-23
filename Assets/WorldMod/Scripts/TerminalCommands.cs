using UnityEngine;
using CommandTerminal;
using UnityEngine.Rendering;

namespace Fab.WorldMod
{
    public class TerminalCommands : MonoBehaviour
    {
		[SerializeField]
		private Material projectionMaterial;

        void Start()
        {
			Terminal.Shell.AddCommand("Set_Lensing", SetLensing, 1, 1, help: "Sets the projection lensing [0 to 1]");
			Terminal.Shell.AddCommand("Show_Checker", ToggleChecker, 1, 1, help: "Shows/hides the checker pattern overlay");
		}

		private void SetLensing(CommandArg[] args)
		{
			float value = args[0].Float;

			if (Terminal.IssuedError)
				return;

			projectionMaterial.SetFloat("_Lensing", value);
		}

		private void ToggleChecker(CommandArg[] args)
		{
			bool value = args[0].Bool;

			if (Terminal.IssuedError)
				return; 

			projectionMaterial.SetKeyword(new LocalKeyword(projectionMaterial.shader, "_CHECKEROVERLAY"), value);
		}
	}
}
