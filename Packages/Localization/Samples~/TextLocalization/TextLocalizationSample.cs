using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Fab.Localization.Samples
{

	[RequireComponent(typeof(UIDocument))]
	public class TextLocalizationSample : MonoBehaviour
	{
		void Start()
		{
			ILocalization localization = LocalizationComponent.Localization;

			UIDocument doc = GetComponent<UIDocument>();

			var dropdown = doc.rootVisualElement.Q<DropdownField>(name: "language-dropdown");

			dropdown.choices = localization.Locales.Select(l => l.Name).ToList();
			dropdown.SetValueWithoutNotify(localization.ActiveLocale.Name);

			dropdown.RegisterValueChangedCallback(evt =>
			{
				int localeIndex = dropdown.choices.IndexOf(evt.newValue);
				Locale locale = localization.Locales.ElementAt(localeIndex);
				localization.ActivateLocale(locale);
			});

			var localText = doc.rootVisualElement.Q<TextElement>(name: "local-text");
			localText.AddManipulator(new Localizable(localization));
		}
	}
}
