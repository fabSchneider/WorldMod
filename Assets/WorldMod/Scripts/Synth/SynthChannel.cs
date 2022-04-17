using System.Collections.Generic;
using UnityEngine;

namespace Fab.WorldMod.Synth
{
	/// <summary>
	/// This class holds a list of layers represented as a channel inside the synthesizer.
	/// </summary>
    public class SynthChannel 
    {
		private List<SynthLayer> layers;
		public List<SynthLayer> Layers => layers;

		private RenderTexture renderTexture;
		public RenderTexture RenderTexture => renderTexture;

		public SynthChannel(RenderTexture renderTexture)
		{
			layers = new List<SynthLayer>();
			this.renderTexture = renderTexture;
		}
	}
}
