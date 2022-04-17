using System.Collections.Generic;
using UnityEngine;

namespace Fab.WorldMod.Synth
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

		private List<MutateNode> mutateNodes;
		public IReadOnlyList<MutateNode> MutateNodes => mutateNodes;
		public BlendNode BlendNode { get; set; }

		public SynthLayer(string channelName)
		{
			this.channelName = channelName;
			mutateNodes = new List<MutateNode>();
		}

		//public SynthLayer(int groupId)
		//{
		//	this.groupId = groupId;
		//}

		public void AddMutateNode(MutateNode node)
		{
			mutateNodes.Add(node);
		}
	}
}
