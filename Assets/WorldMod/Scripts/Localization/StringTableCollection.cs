using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;
using NaughtyAttributes;
using UnityEngine;

namespace Fab.WorldMod.Localization
{
	[CreateAssetMenu(
		fileName = "StringTableCollection.asset",
		menuName = "WorldMod/Localization/String Table Collection")]
	public class StringTableCollection : ScriptableObject, ISerializationCallbackReceiver
	{
		[SerializeField]
		private List<StringTable> stringTables;

		private Dictionary<Locale, StringTable> stringTablesByLocale;

		public IEnumerable<Locale> Locales => stringTablesByLocale.Keys;

		[SerializeField]
		private List<LocalStringKey> localStringKeys = new List<LocalStringKey>();

		private Dictionary<string, LocalStringKey> keysByName = new Dictionary<string, LocalStringKey>();

		public LocalStringKey GetLocalStringKey(string key)
		{
			keysByName.TryGetValue(key, out LocalStringKey localStringKey);
			return localStringKey;
		}

		[Serializable]
		public class LocalStringKey
		{
			[SerializeField]
			private string key;
			[SerializeField]
			private int id;
			[SerializeField]
			private string comment;

			public string Key => key;
			public int Id => id;
			public string Comment => comment;

			public LocalStringKey(string key, int id, string comment)
			{
				this.key = key;
				this.id = id;
				this.comment = comment;
			}

			public override bool Equals(object obj)
			{
				return obj is LocalStringKey item &&
					   id == item.id;
			}

			public override int GetHashCode()
			{
				return HashCode.Combine(id);
			}
		}

		public bool HasLocale(Locale locale)
		{
			return stringTablesByLocale.ContainsKey(locale);
		}

		public StringTable this[Locale locale] => stringTablesByLocale[locale];


		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
			if (stringTablesByLocale == null)
				stringTablesByLocale = new Dictionary<Locale, StringTable>();

			//stringTables = new List<StringTable>(stringTablesByLocale.Values);

			if (keysByName == null)
				keysByName = new Dictionary<string, LocalStringKey>();

			//localStringKeys = new List<LocalStringKey>(keysByName.Values);
		}

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			if (stringTablesByLocale == null)
				stringTablesByLocale = new Dictionary<Locale, StringTable>();
			else
				stringTablesByLocale.Clear();

			for (int i = 0; i < stringTables.Count; i++)
			{
				AddStringTable(stringTables[i]);
			}

			if (keysByName == null)
				keysByName = new Dictionary<string, LocalStringKey>();
			else
				keysByName.Clear();

			for (int i = 0; i < localStringKeys.Count; i++)
				keysByName.Add(localStringKeys[i].Key, localStringKeys[i]);
		}

		public bool AddStringTable(StringTable stringTable)
		{
			if (stringTable == null)
				return false;

			return stringTablesByLocale.TryAdd(stringTable.Locale, stringTable);
		}

		public void RemoveStringTable(StringTable stringTable)
		{
			stringTablesByLocale.Remove(stringTable.Locale);
		}

		public void SetLocalStrings(string key, IEnumerable<(Locale locale, string localString)> localStrings)
		{
			if (!keysByName.TryGetValue(key, out LocalStringKey localStringKey))
			{
				localStringKey = new LocalStringKey(key, UIdGenerator.NextID(), null);
				keysByName.Add(key, localStringKey);
			}

			foreach (var item in localStrings)
			{
				if (stringTablesByLocale.TryGetValue(item.locale, out StringTable stringTable))
					stringTable.SetLocalString(localStringKey.Id, item.localString);
			}
		}

		public bool SetLocalString(string key, Locale locale, string localString)
		{
			if (stringTablesByLocale.TryGetValue(locale, out StringTable stringTable))
			{
				if (keysByName.TryGetValue(key, out LocalStringKey localStringKey))
				{
					stringTable.SetLocalString(localStringKey.Id, localString);
					return true;
				}
				else
				{
					LocalStringKey stringKey = new LocalStringKey(key, UIdGenerator.NextID(), null);
					keysByName.Add(key, stringKey);
					stringTable.SetLocalString(stringKey.Id, localString);
					return true;
				}
			}
			else
			{
				return false;
			}
		}

		public bool TryGetLocalString(string key, Locale locale, out string localString)
		{
			if (stringTablesByLocale.TryGetValue(locale, out StringTable stringTable))
			{
				if (keysByName.TryGetValue(key, out LocalStringKey keyItem))
				{
					return stringTable.TryGetLocalString(keyItem.Id, out localString);
				}
			}
			localString = default(string);
			return false;
		}

		public bool TryGetLocalString(int id, Locale locale, out string localString)
		{
			if (stringTablesByLocale.TryGetValue(locale, out StringTable stringTable))
				return stringTable.TryGetLocalString(id, out localString);

			localString = default(string);
			return false;
		}


#if UNITY_EDITOR
		[Button("Import")]
		private void ImportInEditor()
		{
			string filePath = UnityEditor.EditorUtility.OpenFilePanel("Import localization data", null, "csv");
			if (!string.IsNullOrEmpty(filePath))
			{
				ImportFromCSV(filePath);
				((ISerializationCallbackReceiver)this).OnAfterDeserialize();
			}
		}
#endif

		private static readonly string keyFieldName = "Key";
		private static readonly string idFieldName = "Id";
		private static readonly string commentFieldName = "Comment";

		public void ImportFromCSV(string filePath)
		{
			if (!File.Exists(filePath) && Path.GetExtension(filePath) == ".csv")
			{
				Debug.LogError("Could not load file because it does not exist or is not of the right file format.");
				return;
			}

			using (var reader = new StreamReader(filePath, encoding: System.Text.Encoding.Default))
			using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
			{
				localStringKeys.Clear();

				string[] localeNames = new string[stringTables.Count];
				for (int i = 0; i < stringTables.Count; i++)
				{
					StringTable table = stringTables[i];
					localeNames[i] = table.Locale.ToString();
					table.Clear();
				}

				csv.Read();
				csv.ReadHeader();
				while (csv.Read())
				{
					string key = csv.GetField(keyFieldName);
					int id = csv.GetField<int>(idFieldName);
					string comment = csv.GetField(commentFieldName);

					localStringKeys.Add(new LocalStringKey(key, id, comment));

					for (int i = 0; i < stringTables.Count; i++)
					{
						if (csv.TryGetField(localeNames[i], out string localString))
							if (!string.IsNullOrEmpty(localString))
								stringTables[i].SetLocalString(id, localString);
					}
				}
			}
		}
	}
}
