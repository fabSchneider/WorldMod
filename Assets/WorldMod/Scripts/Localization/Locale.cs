using System;
using UnityEngine;

namespace Fab.WorldMod.Localization
{
	[Serializable]
	public struct Locale
	{
		public static readonly Locale None = new Locale();
		public static readonly Locale enUS = new Locale("English", "en", "US");
		public static readonly  Locale deDE = new Locale("German", "de", "DE");

		[SerializeField]
		private string name;

		[SerializeField]
		private string language;

		[SerializeField]
		private string territory;

		public Locale(string name, string language, string territory)
		{
			this.name = name;
			this.language = language;
			this.territory = territory;
		}

		public string Name { get => name; }
		public string Language { get => language; set => language = value; }
		public string Territory { get => territory; set => territory = value; }

		public string Code => Language + '-' + Territory;

		public override bool Equals(object obj)
		{
			return obj is Locale locale && Equals(locale);
		}

		public bool Equals(Locale locale)
		{
			return language == locale.language && territory == locale.territory;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(language, territory);
		}

		public override string ToString()
		{
			if (language == null)
				return "None";

			return $"{name}({language}-{territory})";
		}
	}

}
