using System.Collections.Generic;

namespace Fab.Localization
{
	public class StringTable
	{
		protected Locale locale;
		public Locale Locale => locale;

		private LocaleFormat localeFormat;
		public LocaleFormat LocaleFormat => localeFormat;


		protected Dictionary<int, string> localStringsById;
		public string this[int id] => localStringsById[id];


		private StringTable() { }

		public StringTable(Locale locale)
		{
			this.locale = locale;
			localStringsById = new Dictionary<int, string>();
		}

		internal static StringTable CreateFromAsset(StringTableAsset asset)
		{
			Dictionary<int, string> localStringsById = new Dictionary<int, string>(asset.Count);
			foreach (var item in asset.LocalStrings)
				localStringsById[item.Id] = item.LocalString;

			return new StringTable()
			{
				locale = asset.Locale,
				localStringsById = localStringsById,
				localeFormat = asset.LocaleFormat
			};
		}

		public bool TryGetLocalString(int id, out string localString)
		{
			return localStringsById.TryGetValue(id, out localString);
		}

		public void SetLocalString(int id, string localString)
		{
			if (!localStringsById.TryAdd(id, localString))
				localStringsById[id] = localString;
		}
	}
}
