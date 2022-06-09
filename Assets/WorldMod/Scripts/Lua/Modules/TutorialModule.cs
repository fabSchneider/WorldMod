using Fab.Geo.Lua.Interop;
using Fab.Lua.Core;
using Fab.Localization;
using Fab.WorldMod.UI;
using UnityEngine;

namespace Fab.WorldMod.Lua
{
	[LuaHelpInfo("Module to set the tutorial overlay")]
	[LuaName("tutorial")]
	public class TutorialModule : LuaObject, ILuaObjectInitialize
	{
		private UiController ui;

		public void Initialize()
		{
			ui = Object.FindObjectOfType<UiController>();

			if (ui == null)
				throw new LuaObjectInitializationException("Could not find ui controller");
		}

		public void add_tutorial(ImageProxy img, string locale)
		{
			if(LocalizationModule.FuzzyMatchLocale(LocalizationComponent.Localization, locale, out Locale loc))
			{
				ui.AddTutorialOverlayImg(img.Target, loc);
			}
			else
			{
				throw new System.Exception($"Could not add tutorial. Locale {locale} does not exist");
			}
		}

	}
}
