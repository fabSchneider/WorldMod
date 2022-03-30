using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fab.WorldMod.Localization
{
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
	}


	[CreateAssetMenu(
		fileName = "StringTableCollection.asset",
		menuName = "WorldMod/Localization/String Table Collection")]
	public class StringTableCollectionAsset : ScriptableObject
	{
		[SerializeField]
		private List<StringTableAsset> stringTables;
		public List<StringTableAsset> StringTableAssets => stringTables;

		[SerializeField]
		private List<LocalStringKey> localStringKeys;
		public List<LocalStringKey> LocalStringKeys => localStringKeys;
	}
}
