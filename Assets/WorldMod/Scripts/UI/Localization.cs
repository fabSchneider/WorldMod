using System;
using System.Collections.Generic;
using Fab.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace Fab.WorldMod
{
	public class OnChangeLocaleSignal : ASignal<Locale> { }
	public class OnLocaleChangedSignal : ASignal<ILocalization> { }

	public class Localizable : Manipulator
	{
		private TextElement textElement;
		public string Identifier { get; set; }

		public Localizable(string identifier)
		{
			Identifier = identifier;
		}

		protected override void RegisterCallbacksOnTarget()
		{
			textElement = target as TextElement;
			if (textElement == null)
				throw new Exception("Localizable Manipulator can only be added to TextElements");

			Signals.Get<OnLocaleChangedSignal>().AddListener(OnLocaleChange);

			//target.RegisterCallback<LocaleChangedEvent>(OnLocaleChange);
		}

		protected override void UnregisterCallbacksFromTarget()
		{
			textElement = null;
			Signals.Get<OnLocaleChangedSignal>().RemoveListener(OnLocaleChange);
		}

		protected void OnLocaleChange(ILocalization localization)
		{
			textElement.text = localization.GetLocalizedText(Identifier);
		}
	}

	public class LocaleChangedEvent : EventBase<LocaleChangedEvent>
	{
		public ILocalization Localization { get; protected set; }

		public LocaleChangedEvent() : base()
		{
			bubbles = true;
			tricklesDown = true;
		}

		public static LocaleChangedEvent GetPooled(ILocalization localization)
		{
			LocaleChangedEvent evt = GetPooled();
			evt.Localization = localization;
			//evt.bubbles = false;
			return evt;
		}
	}


	[Serializable]
	public struct Locale
	{
		[SerializeField]
		private string name;

		public Locale(string identifier)
		{
			this.name = identifier;
		}

		public string Name { get => name; }

		public override bool Equals(object obj)
		{
			return obj is Locale locale &&
				   name == locale.name;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(name);
		}
	}

	[Serializable]
	public class LocalizedTextToken
	{
		[SerializeField]
		private string name;
		[SerializeField]
		private List<LocaleTextData> localeTexts;

		public LocalizedTextToken(string name, List<LocaleTextData> localeTexts)
		{
			this.name = name;
			this.localeTexts = localeTexts;
		}

		public string Name => name;
		public IReadOnlyList<LocaleTextData> LocaleTexts => localeTexts;
	}

	[Serializable]
	public struct LocaleTextData
	{
		[SerializeField]
		private Locale locale;
		[SerializeField]
		private string text;

		public LocaleTextData(Locale locale, string text)
		{
			this.locale = locale;
			this.text = text;
		}

		public Locale Locale => locale;
		public string Text => text;
	}

	public interface ILocalization
	{
		public string GetLocalizedText(string identifier);
	}

	[RequireComponent(typeof(UIDocument))]
	public class Localization : MonoBehaviour, ILocalization
	{
		private UIDocument document;

		[SerializeField]
		private List<Locale> supportedLocales;

		[SerializeField]
		private List<LocalizedTextToken> localizedTokens;

		private int currentLocale = -1;

		private Dictionary<string, string[]> textDataById;

		private void Start()
		{
			document = GetComponent<UIDocument>();

			if (supportedLocales.Count > 0)
				currentLocale = 0;

			BuildTextDataMap();
			UpdateLocale();
		}

		private void OnEnable()
		{
			Signals.Get<OnChangeLocaleSignal>().AddListener(OnChangeLocale);
		}

		private void OnDisable()
		{
			Signals.Get<OnChangeLocaleSignal>().RemoveListener(OnChangeLocale);
		}

		private void BuildTextDataMap()
		{
			textDataById = new Dictionary<string, string[]>(localizedTokens.Count);
			foreach (var token in localizedTokens)
			{
				string[] texts = new string[supportedLocales.Count];

				for (int i = 0; i < token.LocaleTexts.Count; i++)
				{
					var localeText = token.LocaleTexts[i];
					int localeId = supportedLocales.IndexOf(localeText.Locale);
					if (localeId != -1)
						texts[localeId] = localeText.Text;
				}

				if (!textDataById.TryAdd(token.Name, texts))
					Debug.LogError($"Duplicate localization token found for \"{token.Name}\"");
			}
		}

		private void OnChangeLocale(Locale locale)
		{
			int localeID = supportedLocales.IndexOf(locale);
			if (localeID == -1)
				throw new NotSupportedException($"Locale \"{locale.Name}\" is not supported.");

			currentLocale = localeID;
			Debug.Log("Changing locale to " + locale.Name);
			UpdateLocale();

		}

		public void UpdateLocale()
		{
			//using (LocaleChangedEvent localeEvt = LocaleChangedEvent.GetPooled(this))
			//{
			//	localeEvt.target = null;

			//	document.rootVisualElement.SendEvent(localeEvt);
			//}

			Signals.Get<OnLocaleChangedSignal>().Dispatch(this);
		}

		public string GetLocalizedText(string identifier)
		{
			if (textDataById.TryGetValue(identifier, out string[] texts))
			{
				return texts[currentLocale] ?? "$MISSING LOCALE";
			}

			return "$IDENTIFER NOT FOUND";
		}
	}
}
