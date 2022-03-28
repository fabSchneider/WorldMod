using System;
using UnityEngine.UIElements;

namespace Fab.WorldMod.Localization
{
	public class Localizable : Manipulator
	{
		protected TextElement textElement;
		protected bool isIdSet = false;
		protected int id;

		private ILocalization localization;

		public Localizable(ILocalization localization)
		{
			this.localization = localization;
		}

		protected override void RegisterCallbacksOnTarget()
		{
			textElement = target as TextElement;
			if (textElement == null)
				throw new Exception("Localizable Manipulator can only be added to TextElements");

			UpdateText();

			localization.LocaleChanged += UpdateText;
		}

		protected override void UnregisterCallbacksFromTarget()
		{
			textElement = null;
			id = 0;
			isIdSet = false;
			localization.LocaleChanged -= UpdateText;
		}

		protected void UpdateText()
		{
			if (isIdSet)
			{
				if (localization.TryGetLocalizedString(id, out string localString))
					textElement.text = localString;
				else if (localization.TryGetStringKey(id, out localString))
					textElement.text = localString;
			}
			else if (localization.TryGetStringID(textElement.text, out id))
			{
				isIdSet = true;
				if (localization.TryGetLocalizedString(id, out string localString))
					textElement.text = localString;
			}
		}
	}

}
