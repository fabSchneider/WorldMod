using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fab.WorldMod.Localization
{
	public class StringTable
	{
		protected Locale locale;
		public Locale Locale => locale;

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
				localStringsById = localStringsById
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


	[CreateAssetMenu(
	fileName = "StringTable.asset",
	menuName = "WorldMod/Localization/String Table")]
	public class StringTableAsset : ScriptableObject
    {
		[SerializeField]
		protected Locale locale;

		public Locale Locale => locale;

		[SerializeField]
		private List<StringItem> localStrings;

		internal IEnumerable<StringItem> LocalStrings => localStrings;

		public int Count => localStrings.Count;

		[Serializable]
		internal class StringItem
		{
			[SerializeField]
			private int id;
			[SerializeField]
			private string localString;
			public int Id => id;
			public string LocalString => localString;

			public StringItem(int id, string localString)
			{
				this.id = id;
				this.localString = localString;
			}

			public override bool Equals(object obj)
			{
				return obj is StringItem row &&
					   id == row.id;
			}

			public override int GetHashCode()
			{
				return HashCode.Combine(id);
			}
		}

		internal void AddLocalString(int id , string localString)
		{
			localStrings.Add(new StringItem(id, localString));
		}

		internal void Clear()
		{
			localStrings.Clear();
		}
	}
}
