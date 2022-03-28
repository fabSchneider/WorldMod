using System;
using UnityEngine;

namespace Fab.WorldMod.Localization
{
	[Serializable]
	public struct Locale
	{
		[SerializeField]
		private string name;

		[SerializeField]
		private string code;

		public Locale(string name, string code)
		{
			this.name = name;
			this.code = code;
		}

		public string Name { get => name; }
		public string Code { get => code; }

		public override bool Equals(object obj)
		{
			return obj is Locale locale &&
				   code == locale.code;
		}

		public bool Equals(Locale locale)
		{
			return code == locale.code;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(code);
		}

		public override string ToString()
		{
			if (code == null)
				return "None";

			return $"{name}({code})";
		}

		public static Locale None => new Locale();
		public static Locale enUS => new Locale("English", "en-US");
		public static Locale deDE => new Locale("German", "de-DE");
	}

}
