using Fab.Localization;
using UnityEngine.UIElements;
using System.Linq;
using System.Collections.Generic;

namespace Fab.WorldMod.UI
{
	public class MainbarController
	{
		VisualElement mainBar;
		RadioButtonGroup languageButtonGroup;

		public VisualElement MainBar => mainBar;

		public MainbarController(VisualElement root, ILocalization localization)
		{
			mainBar = root.Q(name: "main-bar");
			languageButtonGroup = mainBar.Q<RadioButtonGroup>(name: "language-toggles");

			List<Locale> locales = new List<Locale>(localization.Locales);
			languageButtonGroup.userData = locales;
			languageButtonGroup.choices = locales.Select(l => l.Language.ToUpper());
			languageButtonGroup.value = locales.IndexOf(localization.ActiveLocale);
			languageButtonGroup.RegisterValueChangedCallback(evt =>
			{
				if (evt.newValue != -1)
				{
					Locale locale = localization.Locales.ElementAt(evt.newValue);
					localization.ActivateLocale(locale);
				}
			});
		}
	}
}
