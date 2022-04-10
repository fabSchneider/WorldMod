using System;
using System.Collections.Generic;

namespace Fab.WorldMod.Localization
{
	public class StringTableCollection
	{
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

			public static implicit operator (int id, string key)(StringIdentifier value)
			{
				return (value.id, value.key);
			}

			public static implicit operator StringIdentifier((int id, string key) value)
			{
				return new StringIdentifier(value.id, value.key);
			}
		}


		private Dictionary<Locale, StringTable> stringTablesByLocale;
		public IEnumerable<Locale> Locales => stringTablesByLocale.Keys;

		private Dictionary<string, StringIdentifier> identifiersByKey = new Dictionary<string, StringIdentifier>(StringComparer.InvariantCultureIgnoreCase);
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
			Dictionary<Locale, StringTable> stringTablesByLocale = new Dictionary<Locale, StringTable>(asset.StringTableAssets.Count);
			foreach (var tableAsset in asset.StringTableAssets)
			{
				StringTable stringTable = StringTable.CreateFromAsset(tableAsset);
				stringTablesByLocale[stringTable.Locale] = stringTable;
			}

			Dictionary<string, StringIdentifier> idsByKey = new Dictionary<string, StringIdentifier>(
				asset.LocalStringKeys.Count,
				StringComparer.InvariantCultureIgnoreCase);
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
	}
}
