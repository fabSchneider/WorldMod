using System.Collections.Generic;
using UnityEngine;

namespace Fab.Localization
{
	public class OnChangeLocaleSignal { }

	[AddComponentMenu("Localization/Localization")]
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

		public IEnumerable<Locale> AvailableLocales => instance.LocalizationTables.Locales;
	}
}
