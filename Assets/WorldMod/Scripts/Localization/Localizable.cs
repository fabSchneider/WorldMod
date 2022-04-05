using System;
using UnityEngine.UIElements;

namespace Fab.WorldMod.Localization
{
	public class Localizable : Manipulator
	{
		private static readonly string rightToLeftClassname = "text-element-rtl";

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

			OnLocaleChanged();

			localization.LocaleChanged += OnLocaleChanged;
		}

		protected override void UnregisterCallbacksFromTarget()
		{
			textElement.EnableInClassList(rightToLeftClassname, false);
			textElement = null;
			id = 0;
			isIdSet = false;
			localization.LocaleChanged -= OnLocaleChanged;
		}

		protected void OnLocaleChanged()
		{
			LocaleFormat format = localization.ActiveFormat;
			textElement.EnableInClassList(rightToLeftClassname, format.IsRightToLeft);
			UpdateText();
		}

		public void SetKey(string key)
		{
			isIdSet = localization.TryGetStringID(key, out id);

			if (isIdSet)
				UpdateText();
			else
				textElement.text = key;
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
