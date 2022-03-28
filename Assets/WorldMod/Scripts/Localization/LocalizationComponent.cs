using System;
using System.Collections.Generic;
using Fab.Common;
using UnityEngine;

namespace Fab.WorldMod.Localization
{
	[AddComponentMenu("WorldMod/Localization")]
	public class LocalizationComponent : MonoBehaviour, ILocalization
	{
		[SerializeField]
		private StringTableCollection localizationTables;

		public StringTableCollection LocalizationTables
		{
			get
			{
#if UNITY_EDITOR
				if (!UnityEditor.EditorApplication.isPlaying)
					throw new Exception("Cannot access localization tables in edit mode.");
#endif
				return localizationTables;
			}
		}

		private Locale currentLocale;

		public Locale CurrentLocale => currentLocale;

		[SerializeField]
		private StringTable defaultLocale;

		private void Start()
		{
			if (defaultLocale != null && localizationTables.HasLocale(defaultLocale.Locale))
				currentLocale = defaultLocale.Locale;

			UpdateContent();
		}

		private void OnEnable()
		{
			Signals.Get<OnChangeLocaleSignal>().AddListener(ActivateLocale);
		}

		private void OnDisable()
		{
			Signals.Get<OnChangeLocaleSignal>().RemoveListener(ActivateLocale);
		}

		public IEnumerable<Locale> AvailableLocales => localizationTables.Locales;

		public void AddLocale(Locale locale)
		{
#if UNITY_EDITOR
			if (!UnityEditor.EditorApplication.isPlaying)
			{
				throw new Exception("Cannot add locale in edit mode.");
			}
#endif
			if (localizationTables.HasLocale(locale))
			{
				throw new Exception("A locale with the same code already exists");
			}
			else
			{
				StringTable table = StringTable.CreateInstance(locale);
				localizationTables.AddStringTable(table);
			}
		}

		/// <summary>
		/// Sets the current locale.
		/// </summary>
		/// <param name="locale"></param>
		/// <exception cref="NotSupportedException"></exception>
		public void ActivateLocale(Locale locale)
		{
			if (currentLocale.Equals(locale))
				return;

			if (localizationTables.HasLocale(locale))
			{
				currentLocale = locale;
				Debug.Log("Setting locale to " + locale.Name);
				UpdateContent();
			}
			else
			{
				throw new NotSupportedException($"Locale \"{locale}\" is not available.");
			}
		}

		/// <summary>
		/// Updates all localized content
		/// </summary>
		public void UpdateContent()
		{
			Signals.Get<OnLocaleChangedSignal>().Dispatch(this);
		}

		/// <summary>
		/// Tries to get the localized string for the specified key and the currently active locale
		/// </summary>
		/// <param name="key"></param>
		/// <param name="localString"></param>
		/// <returns></returns>
		public bool TryGetLocalizedString(string key, out string localString)
		{
			return localizationTables.TryGetLocalString(key, currentLocale, out localString);
		}

		/// <summary>
		/// Tries to get the localized string for the id and the currently active locale
		/// </summary>
		/// <param name="id"></param>
		/// <param name="localString"></param>
		/// <returns></returns>
		public bool TryGetLocalizedString(int id, out string localString)
		{
			return localizationTables.TryGetLocalString(id, currentLocale, out localString);
		}

		public StringTableCollection.LocalStringKey GetLocaleStringKey(string key)
		{
			return localizationTables.GetLocalStringKey(key);
		}
	}
}
