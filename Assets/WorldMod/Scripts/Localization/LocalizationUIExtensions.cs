using UnityEngine.UIElements;

namespace Fab.WorldMod.Localization
{
    public static class LocalizationUIExtensions 
    {
		public static T WithLocalizable<T>(this T elem) where T : TextElement
		{
			elem.AddManipulator(new Localizable(LocalizationComponent.Localization));
			return elem;
		}

		//public static T WithLocalizable<T>(this T elem, string key) where T : TextElement
		//{
		//	if(LocalizationComponent.Localization.LocalizationTables.TryGetStringID(key, out int id))
		//		elem.AddManipulator(new Localizable(id, LocalizationComponent.Localization));
		//	else
		//	{
		//		elem.text = key;
		//		elem.AddManipulator(new Localizable(LocalizationComponent.Localization));
		//	}
		//	return elem;
		//}
	}
}
