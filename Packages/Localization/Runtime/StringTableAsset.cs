using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fab.Localization
{


	[CreateAssetMenu(
	fileName = "StringTable.asset",
	menuName = "Localization/String Table")]
	public class StringTableAsset : ScriptableObject
    {
		[SerializeField]
		protected Locale locale;

		public Locale Locale => locale;

		[SerializeField]
		private List<StringItem> localStrings;

		[SerializeField]
		private LocaleFormat localeFormat;
		public LocaleFormat LocaleFormat => localeFormat;

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
