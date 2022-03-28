using System.Collections.Generic;
using System.Linq;
using Fab.Lua.Core;
using Fab.WorldMod.Localization;
using MoonSharp.Interpreter;

namespace WorldMod.Lua
{
	[LuaHelpInfo("Module to access localization functions")]
	[LuaName("localization")]
	public class LocalizationModule : LuaObject, ILuaObjectInitialize
	{
		private LocalizationComponent localization;

		public void Initialize()
		{
			localization = UnityEngine.Object.FindObjectOfType<LocalizationComponent>();

			if (localization == null)
				throw new LuaObjectInitializationException("Could not find localization component");
		}

		[LuaHelpInfo("Returns the currently active locale (Read only)")]
		public string current_locale => localization.CurrentLocale.ToString();

		[LuaHelpInfo("Returns a list of all available locales")]
		public string[] get_locales()
		{
			return localization.AvailableLocales.Select(l => l.ToString()).ToArray();
		}

		[LuaHelpInfo("Activates the specified locale (e.g. en-US).")]
		public void avtivate_locale(string locale_code)
		{
			Locale locale = GetLocaleFromCode(locale_code);
			if (locale.Equals(Locale.None))
				throw new System.Exception($"Cannot set locale to \"{locale_code}\". No matching locale is available. " +
					$"Use {nameof(get_locales)}() to see which locales are available.");

			localization.ActivateLocale(locale);
		}

		[LuaHelpInfo("Adds a locale to the localization system")]
		public void add_locale(string name, string code)
		{
			localization.AddLocale(new Locale(name, code));
		}

		[LuaHelpInfo("Sets a localized string in the localization system")]
		public void set_string(string key, string locale_code, string local_string)
		{
			Locale locale = GetLocaleFromCode(locale_code);
			if (locale.Equals(Locale.None))
				throw new System.Exception($"Cannot add string for \"{locale_code}\". No matching locale is available." +
					$"Use {nameof(get_locales)}() to see which locales are available.");
			else
			{
				localization.LocalizationTables.SetLocalString(key, locale, local_string);
			}
		}

		[LuaHelpInfo("Sets a collection of localized string in the localization system")]
		public void set_strings(string key, Table local_strings)
		{
			List<(Locale, string)> ls = new List<(Locale, string)>(local_strings.Length);
			foreach (var item in local_strings.Pairs)
			{
				Locale locale = GetLocaleFromCode(item.Key.String);
				string value = item.Value.String;
				if (!locale.Equals(Locale.None))
					ls.Add((locale, value));
			}

			localization.LocalizationTables.SetLocalStrings(key, ls);
		}

		[LuaHelpInfo("updates all localized content")]
		public void update()
		{
			localization.UpdateContent();
		}

		private Locale GetLocaleFromCode(string code)
		{
			return localization.AvailableLocales.FirstOrDefault(l => l.Code == code);
		}
	}
}
