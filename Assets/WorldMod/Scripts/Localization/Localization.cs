using System;
using System.Collections.Generic;

namespace Fab.WorldMod.Localization
{
	public interface ILocalization
	{
		bool TryGetLocalizedString(string key, out string localString);
		bool TryGetLocalizedString(int id, out string localString);
		bool TryGetStringID(string key, out int id);
		bool TryGetStringKey(int id, out string key);

		event Action LocaleChanged;

		Locale ActiveLocale { get; }

		LocaleFormat ActiveFormat { get; }
		IEnumerable<Locale> Locales { get; }
	}

	public class Localization : ILocalization
	{
		private Locale activeLocale;
		public Locale ActiveLocale => activeLocale;

		private event Action localeChanged;

		public event Action LocaleChanged
		{
			add => localeChanged += value;
			remove => localeChanged -= value;
		}

		private StringTableCollection localizationTables;

		public StringTableCollection LocalizationTables => localizationTables;

		public IEnumerable<Locale> Locales => localizationTables.Locales;

		public LocaleFormat ActiveFormat => localizationTables[activeLocale].LocaleFormat;

		public Localization(StringTableCollection localizationTables)
		{
			this.localizationTables = localizationTables;
		}

		/// <summary>
		/// Sets the current locale.
		/// </summary>
		/// <param name="locale"></param>
		/// <exception cref="NotSupportedException"></exception>
		public void ActivateLocale(Locale locale)
		{
			if (activeLocale.Equals(locale))
				return;

			if (localizationTables.HasLocale(locale))
			{
				activeLocale = locale;
				localeChanged?.Invoke();
			}
			else
			{
				throw new NotSupportedException($"Locale \"{locale}\" is not available.");
			}
		}

		public void AddLocale(Locale locale)
		{
			if (localizationTables.HasLocale(locale))
			{
				throw new Exception("A locale with the same code already exists");
			}
			else
			{
				StringTable table = new StringTable(locale);
				localizationTables.AddStringTable(table);
			}
		}

		/// <summary>
		/// Tries to get the localized string for the specified key and the currently active locale
		/// </summary>
		/// <param name="key"></param>
		/// <param name="localString"></param>
		/// <returns></returns>
		public bool TryGetLocalizedString(string key, out string localString)
		{
			if (localizationTables.TryGetStringID(key, out int id))
				return localizationTables[activeLocale].TryGetLocalString(id, out localString);

			localString = null;
			return false;
		}

		/// <summary>
		/// Tries to get the localized string for the id and the currently active locale
		/// </summary>
		/// <param name="id"></param>
		/// <param name="localString"></param>
		/// <returns></returns>
		public bool TryGetLocalizedString(int id, out string localString)
		{
			return localizationTables[activeLocale].TryGetLocalString(id, out localString);
		}

		public bool TryGetStringID(string key, out int id)
		{
			return localizationTables.TryGetStringID(key, out id);
		}

		public bool TryGetStringKey(int id, out string key)
		{
			return localizationTables.TyrGetStringKey(id, out key);
		}

		public void ImportFromCSV(string filePath)
		{
			LocalizationImportUtility.ImportFromCSV(filePath, localizationTables);
		}
	}
}
