using Fab.Common;
using Fab.WorldMod.Localization;
using UnityEngine.UIElements;

namespace Fab.WorldMod.UI
{
    public class MainbarController 
    {
		VisualElement mainBar;

		public MainbarController(VisualElement root)
		{
			mainBar = root.Q(name: "main-bar");
			mainBar.Q<RadioButtonGroup>(name: "language-toggles").RegisterValueChangedCallback(OnLanguageToggleChange);
		}

		protected void OnLanguageToggleChange(ChangeEvent<int> evt)
		{
			var rbGroup = evt.target as RadioButtonGroup;
			if (rbGroup == null)
				return;

			int i = 0;
			var signal = Signals.Get<OnChangeLocaleSignal>();
			foreach (var choice in rbGroup.choices)
			{
				if (i == evt.newValue)
				{
					switch (choice.ToLower())
					{
						case "en":
							signal.Dispatch(Locale.enUS);
							return;
						case "de":
							signal.Dispatch(Locale.deDE);
							return;
					}
				}
				else
				{
					i++;
				}
			}
		}
    }
}
