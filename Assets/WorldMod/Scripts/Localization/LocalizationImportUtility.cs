using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;

namespace Fab.WorldMod.Localization
{
	public static class LocalizationImportUtility
	{
		private static readonly string keyFieldName = "Key";
		private static readonly string idFieldName = "Id";
		private static readonly string commentFieldName = "Comment";

		public static void ImportFromCSV(string filePath, List<LocalStringKey> keys, List<StringTableAsset> stringTables)
		{
			if (!File.Exists(filePath))
				throw new FileNotFoundException("Could not import from file because the file was not found.");

			if (Path.GetExtension(filePath) != ".csv")
				throw new FileLoadException("File is of the wrong format. Expected format is .csv");



			using (var reader = new StreamReader(filePath, encoding: System.Text.Encoding.Default))
			using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
			{

				if (keys == null)
					keys = new List<LocalStringKey>();
				else
					keys.Clear();


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

					keys.Add(new LocalStringKey(key, id, comment));

					for (int i = 0; i < stringTables.Count; i++)
					{
						if (csv.TryGetField(localeNames[i], out string localString))
							if (!string.IsNullOrEmpty(localString))
								stringTables[i].AddLocalString(id, localString);
					}
				}
			}
		}

		public static void ImportFromCSV(string filePath, StringTableCollection stringTables)
		{
			if (!File.Exists(filePath))
				throw new FileNotFoundException("Could not import from file because the file was not found.");

			if (Path.GetExtension(filePath) != ".csv")
				throw new FileLoadException("File is of the wrong format. Expected format is .csv");



			using (var reader = new StreamReader(filePath, encoding: System.Text.Encoding.Default))
			using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
			{
				Locale[] locales = stringTables.Locales.ToArray();
				string[] localeNames = locales.Select(l => l.ToString()).ToArray();

				csv.Read();
				csv.ReadHeader();
				while (csv.Read())
				{
					string key = csv.GetField(keyFieldName);

					for (int i = 0; i < locales.Length; i++)
					{
						if (csv.TryGetField(localeNames[i], out string localString))
							stringTables.SetLocalString(key, locales[i], localString);
					}
				}
			}
		}
	}
}
