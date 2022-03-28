using System;
using Fab.Common;
using UnityEngine.UIElements;

namespace Fab.WorldMod.Localization
{
	public class Localizable : Manipulator
	{
		public static readonly string localeMissingString = "$LOCALE_MISSING";

		private TextElement textElement;
		public string Key { get; private set; }

		public Localizable(string key)
		{
			Key = key;
		}

		protected override void RegisterCallbacksOnTarget()
		{
			textElement = target as TextElement;
			if (textElement == null)
				throw new Exception("Localizable Manipulator can only be added to TextElements");

			Signals.Get<OnLocaleChangedSignal>().AddListener(OnLocaleChange);
		}

		protected override void UnregisterCallbacksFromTarget()
		{
			textElement = null;
			Signals.Get<OnLocaleChangedSignal>().RemoveListener(OnLocaleChange);
		}

		protected void OnLocaleChange(ILocalization localization)
		{
			if (localization.TryGetLocalizedString(Key, out string localString))
				textElement.text = localString;
			else
				textElement.text = localeMissingString;
		}
	}

}
