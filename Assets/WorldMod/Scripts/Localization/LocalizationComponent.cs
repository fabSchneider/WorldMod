using System.Collections.Generic;
using Fab.Common;
using UnityEngine;

namespace Fab.WorldMod.Localization
{
	public class OnChangeLocaleSignal : ASignal<Locale> { }

	[AddComponentMenu("WorldMod/Localization")]
	public class LocalizationComponent : MonoBehaviour
	{
		private static Localization instance;
		public static Localization Localization => instance;

		[SerializeField]
		private StringTableCollectionAsset localizationAsset;

		[SerializeField]
		private StringTableAsset defaultLocale;

		private void Awake()
		{
			if(instance != null)
			{
				Debug.LogError("Duplicate Localization Components are not allowed");
				return;
			}
			var localizationTables = StringTableCollection.CreateFromAsset(localizationAsset);
			instance = new Localization(localizationTables);
		}

		private void OnDestroy()
		{
			instance = null;
		}

		private void Start()
		{
			if (defaultLocale != null && instance.LocalizationTables.HasLocale(defaultLocale.Locale))
				instance.ActivateLocale(defaultLocale.Locale);		
		}

		private void OnEnable()
		{
			Signals.Get<OnChangeLocaleSignal>().AddListener(OnChangeLocale);
		}

		private void OnDisable()
		{
			Signals.Get<OnChangeLocaleSignal>().RemoveListener(OnChangeLocale);
		}

		private void OnChangeLocale(Locale locale)
		{
			Debug.Log("Switching language to " + locale.Name);
			instance.ActivateLocale(locale);
		}

		public IEnumerable<Locale> AvailableLocales => instance.LocalizationTables.Locales;
	}
}
