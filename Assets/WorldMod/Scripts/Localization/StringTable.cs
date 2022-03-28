using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fab.WorldMod.Localization

{
	[CreateAssetMenu(
	fileName = "StringTable.asset",
	menuName = "WorldMod/Localization/String Table")]
	public class StringTable : ScriptableObject, ISerializationCallbackReceiver
    {
		public static StringTable CreateInstance(Locale locale)
		{
			StringTable table = CreateInstance<StringTable>();
			table.locale = locale;
			return table;
		}


		[SerializeField]
		public Locale locale;
		public Locale Locale => locale;

		[SerializeField]
		private List<StringItem> localStrings;

		private Dictionary<int, string> localStringsById = new Dictionary<int, string>();

		[Serializable]
		public class StringItem
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

		public string this[int id] => localStringsById[id];

		public bool TryGetLocalString(int id, out string localString)
		{
			return localStringsById.TryGetValue(id, out localString);
		}

		public void SetLocalString(int id, string localString)
		{
			if (!localStringsById.TryAdd(id, localString))
				localStringsById[id] = localString;
		}

		public void Clear()
		{
			localStrings.Clear();
			localStringsById.Clear(); 
		}

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
			if (localStringsById == null)
				localStringsById = new Dictionary<int, string>();

			//localStrings = new List<StringItem>(localStringsById.Count);
			//foreach (var item in localStringsById)
			//{
			//	localStrings.Add(new StringItem(item.Key, item.Value));
			//}
		
		}

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			if (localStringsById == null)
				localStringsById = new Dictionary<int, string>();
			else
				localStringsById.Clear();

			for (int i = 0; i < localStrings.Count; i++)
				SetLocalString(localStrings[i].Id, localStrings[i].LocalString);
		}
	}
}
