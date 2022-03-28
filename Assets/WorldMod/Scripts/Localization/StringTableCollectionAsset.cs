using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;
using NaughtyAttributes;
using UnityEngine;

namespace Fab.WorldMod.Localization
{
	public class StringTableCollection
	{
		private Dictionary<Locale, StringTable> stringTablesByLocale;
		public IEnumerable<Locale> Locales => stringTablesByLocale.Keys;

		private Dictionary<string, StringIdentifier> identifiersByKey = new Dictionary<string, StringIdentifier>();
		public StringTable this[Locale locale] => stringTablesByLocale[locale];

		public bool HasLocale(Locale locale)
		{
			return stringTablesByLocale.ContainsKey(locale);
		}

		public bool TryGetStringID(string key, out int id)
		{
			if(identifiersByKey.TryGetValue(key, out StringIdentifier identifier))
			{
				id = identifier.id;
				return true;
			}
			id = default(int);
			return false;
		}

		public bool TyrGetStringKey(int id, out string key)
		{
			foreach(var identifier in identifiersByKey.Values)
			{
				if(identifier.id == id)
				{
					key = identifier.key;
					return true;
				}
			}
			key = default(string);
			return false;
		}

		public bool AddStringTable(StringTable stringTable)
		{
			if (stringTable == null)
				return false;

			return stringTablesByLocale.TryAdd(stringTable.Locale, stringTable);
		}

		public void RemoveStringTable(StringTableAsset stringTable)
		{
			stringTablesByLocale.Remove(stringTable.Locale);
		}

		public static StringTableCollection CreateFromAsset(StringTableCollectionAsset asset)
		{
			Dictionary<Locale, StringTable> stringTablesByLocale = new Dictionary<Locale, StringTable>(asset.TableCount);
			foreach (var tableAsset in asset.StringTables)
			{
				StringTable stringTable = StringTable.CreateFromAsset(tableAsset);
				stringTablesByLocale[stringTable.Locale] = stringTable;
			}

			Dictionary<string, StringIdentifier> idsByKey = new Dictionary<string, StringIdentifier>(asset.KeyCount);
			foreach (var key in asset.LocalStringKeys)
				idsByKey[key.Key] = (key.Id, key.Key);

			return new StringTableCollection()
			{
				stringTablesByLocale = stringTablesByLocale,
				identifiersByKey = idsByKey
			};
		}

		public void SetLocalStrings(string key, IEnumerable<(Locale locale, string localString)> localStrings)
		{
			if (!identifiersByKey.TryGetValue(key, out StringIdentifier identfier))
			{
				identfier = new StringIdentifier(UIdGenerator.NextID(), key);
				identifiersByKey.Add(key, identfier);
			}
			
			foreach (var item in localStrings)
			{
				if (stringTablesByLocale.TryGetValue(item.locale, out StringTable stringTable))
					stringTable.SetLocalString(identfier.id, item.localString);
			}
		}

		public bool SetLocalString(string key, Locale locale, string localString)
		{
			if (stringTablesByLocale.TryGetValue(locale, out StringTable stringTable))
			{
				if (identifiersByKey.TryGetValue(key, out StringIdentifier identfier))
				{
					stringTable.SetLocalString(identfier.id, localString);
					return true;
				}
				else
				{
					identfier = new StringIdentifier(UIdGenerator.NextID(), key);
					identifiersByKey.Add(key, identfier);
					stringTable.SetLocalString(identfier.id, localString);
					return true;
				}
			}
			else
			{
				return false;
			}
		}

		//public bool TryGetLocalString(string key, Locale locale, out string localString)
		//{
		//	if (stringTablesByLocale.TryGetValue(locale, out StringTableAsset stringTable))
		//	{
		//		if (idsByKey.TryGetValue(key, out LocalStringKey keyItem))
		//		{
		//			return stringTable.TryGetLocalString(keyItem.Id, out localString);
		//		}
		//	}
		//	localString = default(string);
		//	return false;
		//}

		//public bool TryGetLocalString(int id, Locale locale, out string localString)
		//{
		//	if (stringTablesByLocale.TryGetValue(locale, out StringTableAsset stringTable))
		//		return stringTable.TryGetLocalString(id, out localString);

		//	localString = default(string);
		//	return false;
		//}
	}

	internal struct StringIdentifier
	{
		public readonly int id;
		public readonly string key;

		public StringIdentifier(int id, string key)
		{
			this.id = id;
			this.key = key;
		}

		public override bool Equals(object obj)
		{
			return obj is StringIdentifier other &&
				   id == other.id &&
				   key == other.key;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(id, key);
		}

		public void Deconstruct(out int id, out string key)
		{
			id = this.id;
			key = this.key;
		}

		public static implicit operator (int id, string key)(StringIdentifier value)
		{
			return (value.id, value.key);
		}

		public static implicit operator StringIdentifier((int id, string key) value)
		{
			return new StringIdentifier(value.id, value.key);
		}
	}

	[CreateAssetMenu(
		fileName = "StringTableCollection.asset",
		menuName = "WorldMod/Localization/String Table Collection")]
	public class StringTableCollectionAsset : ScriptableObject
	{
		[SerializeField]
		private List<StringTableAsset> stringTables;
		internal IEnumerable<StringTableAsset> StringTables => stringTables;

		public int TableCount => stringTables.Count;

		[SerializeField]
		private List<LocalStringKey> localStringKeys = new List<LocalStringKey>();

		internal IEnumerable<LocalStringKey> LocalStringKeys => localStringKeys;

		public int KeyCount => localStringKeys.Count;

		[Serializable]
		internal class LocalStringKey
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




#if UNITY_EDITOR
		[Button("Import")]
		private void ImportInEditor()
		{
			string filePath = UnityEditor.EditorUtility.OpenFilePanel("Import localization data", null, "csv");
			if (!string.IsNullOrEmpty(filePath))
			{
				ImportFromCSV(filePath);
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
					StringTableAsset table = stringTables[i];
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
								stringTables[i].AddLocalString(id, localString);
					}
				}
			}
		}
	}
}
