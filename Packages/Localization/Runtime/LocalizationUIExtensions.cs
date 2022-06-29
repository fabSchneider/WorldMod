using UnityEngine.UIElements;

namespace Fab.Localization
{
    public static class LocalizationUIExtensions 
    {
		public static T WithLocalizable<T>(this T elem) where T : TextElement
		{
			elem.AddManipulator(new Localizable(LocalizationComponent.Localization));
			return elem;
		}
	}
}
