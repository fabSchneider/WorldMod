using UnityEngine;

namespace Fab.WorldMod.Synth
{
	[CreateAssetMenu(
		fileName = "SynthNodeLibrary",
		menuName = "WorldMod/Synth Node library",
		order = 0)]
	public class SynthNodeLibraryAsset : ScriptableObject
	{
		[SerializeField]
		private SynthNodeLibrary library;

		public SynthNodeFactory CreateNodeFactory()
		{
			return new SynthNodeFactory(library);
		}
	}
}
