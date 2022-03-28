using Fab.Common;

namespace Fab.WorldMod.Localization
{
	public interface ILocalization
	{
		public bool TryGetLocalizedString(string key, out string localString);
		public bool TryGetLocalizedString(int id, out string localString);
	}

	public class OnChangeLocaleSignal : ASignal<Locale> { }
	public class OnLocaleChangedSignal : ASignal<ILocalization> { }
}
