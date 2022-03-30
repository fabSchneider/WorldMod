using System;
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
		private Localization localization;

		public void Initialize()
		{
			localization = LocalizationComponent.Localization;

			if (localization == null)
				throw new LuaObjectInitializationException("Could not find initialized localization");
		}

		[LuaHelpInfo("Returns the currently active locale (Read only)")]
		public string current_locale => localization.ActiveLocale.ToString();

		[LuaHelpInfo("Returns a list of all available locales")]
		public string[] get_locales()
		{
			return localization.LocalizationTables.Locales.Select(l => l.ToString()).ToArray();
		}

		[LuaHelpInfo("Activates the specified locale (e.g. en-US).")]
		public void avtivate_locale(string locale)
		{
			if (FuzzyMatchLocale(locale, out Locale loc))
				localization.ActivateLocale(loc);
			else
				throw new Exception($"Cannot set locale to \"{locale}\". No matching locale is available. " +
						$"Use {nameof(get_locales)}() to see which locales are available.");

		}

		[LuaHelpInfo("Adds a locale to the localization system")]
		public void add_locale(string name, string language, string territory)
		{
			localization.AddLocale(new Locale(name, language, territory));
		}

		[LuaHelpInfo("Creates a localized string")]
		public string @string(string key, Table local_strings)
		{
			foreach (var item in local_strings.Pairs)
			{
				if (FuzzyMatchLocale(item.Key.String, out Locale locale))
					localization.LocalizationTables.SetLocalString(key, locale, item.Value.String);
			}
			return key;
		}

		private bool FuzzyMatchLocale(string localeString, out Locale locale)
		{
			IEnumerable<Locale> locales = localization.LocalizationTables.Locales.Where(l => l.ToString().Contains(localeString)).ToArray();

			locale = locales.FirstOrDefault(l => string.Equals(l.Code, localeString, StringComparison.InvariantCultureIgnoreCase));

			if (!locale.Equals(Locale.None))
				return true;

			// try to match the first matching language
			locale = locales.FirstOrDefault(l => string.Equals(l.Language, localeString, StringComparison.InvariantCultureIgnoreCase));

			if (!locale.Equals(Locale.None))
				return true;

			// try to match the first matching name
			locale = locales.FirstOrDefault(l => string.Equals(l.Name, localeString, StringComparison.InvariantCultureIgnoreCase));

			return !locale.Equals(Locale.None);

		}
	}
}
