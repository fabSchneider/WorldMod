using UnityEngine.UIElements;

namespace Fab.WorldMod.Localization
{
    public static class LocalizationUIExtensions 
    {
		public static T WithLocalizable<T>(this T elem, string key) where T : TextElement
		{
			elem.AddManipulator(new Localizable(key));
			return elem;
		}
	}
}
