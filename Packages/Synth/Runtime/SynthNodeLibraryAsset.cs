using UnityEngine;

namespace Fab.Synth
{
	[CreateAssetMenu(
		fileName = "SynthNodeLibrary",
		menuName = "Synth/Synth Node library",
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
