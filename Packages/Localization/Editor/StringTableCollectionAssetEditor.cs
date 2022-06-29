using UnityEngine;
using UnityEditor;

namespace Fab.Localization.Editor
{

	[CustomEditor(typeof(StringTableCollectionAsset))]
	public class StringTableCollectionAssetEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			if (GUILayout.Button("Import"))
				Import();
		}

		private void Import()
		{
			string filePath = EditorUtility.OpenFilePanel("Import localization data", null, "csv");
			if (!string.IsNullOrEmpty(filePath))
			{
				StringTableCollectionAsset asset = (StringTableCollectionAsset)target;
				LocalizationImportUtility.ImportFromCSV(filePath, asset.LocalStringKeys, asset.StringTableAssets);
				EditorUtility.SetDirty(asset);
				foreach (var table in asset.StringTableAssets)
					EditorUtility.SetDirty(table);
			}
		}
	}
}
