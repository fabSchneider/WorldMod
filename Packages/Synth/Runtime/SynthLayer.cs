using System.Collections.Generic;
using UnityEngine;

namespace Fab.Synth
{
	/// <summary>
	/// This class represents one layer inside the synthesizer.
	/// Each layer can be build up with different <see cref="SynthNode"/>s that drive the visual output of the layer.  
	/// </summary>
    public class SynthLayer
    {
		private string channelName;
		public string ChannelName => channelName;
		public Texture InputTexture { get; set; }
		public GenNode GenerateNode { get; set; }

		private List<ModulateNode> modulateNodes;
		public IReadOnlyList<ModulateNode> ModulateNodes => modulateNodes;
		public BlendNode BlendNode { get; set; }

		public SynthLayer(string channelName)
		{
			this.channelName = channelName;
			modulateNodes = new List<ModulateNode>();
		}

		public void AddMutateNode(ModulateNode node)
		{
			modulateNodes.Add(node);
		}
	}
}
