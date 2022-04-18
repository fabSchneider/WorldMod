using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Fab.WorldMod.Synth
{
	/// <summary>
	/// The synthesizer is generating, transforming and blending textures together on different channels.
	/// </summary>
	public class Synthesizer : IDisposable
	{
		private Dictionary<int, SynthChannel> channelsById;

		private List<SynthProcessor> synthProcessors;
		private Dictionary<int, SynthProcessor> processorsByChannelId;


		public Synthesizer()
		{
			channelsById = new Dictionary<int, SynthChannel>();

			synthProcessors = new List<SynthProcessor>();
			processorsByChannelId = new Dictionary<int, SynthProcessor>();
		}

		public void AddChannel(string name, int channelId, RenderTexture outputTex)
		{
			if (!channelsById.ContainsKey(channelId))
			{
				channelsById.Add(channelId, new SynthChannel(outputTex));

				SynthProcessor processor = FindMatchingProcessor(outputTex.descriptor);
				if (processor == null)
				{
					processor = new SynthProcessor(name, outputTex.descriptor);
					synthProcessors.Add(processor);
				}

				processorsByChannelId.Add(channelId, processor);
			}
		}

		private SynthProcessor FindMatchingProcessor(RenderTextureDescriptor descriptor)
		{
			foreach (SynthProcessor processor in synthProcessors)
			{
				if (processor.BufferDescriptor.Equals(descriptor))
					return processor;
			}
			return null;
		}

		public void RemoveChannel(int channelId)
		{
			if (channelsById.Remove(channelId))
			{
				SynthProcessor processor = processorsByChannelId[channelId];
				processorsByChannelId.Remove(channelId);
				if (!processorsByChannelId.Values.Contains(processor))
				{
					synthProcessors.Remove(processor);
					processor.Dispose();
				}
			}
		}

		public void ClearAllChannels()
		{
			foreach (SynthChannel channel in channelsById.Values)
				channel.Layers.Clear();
		}

		public void AddLayer(SynthLayer layer, int channelId)
		{
			channelsById[channelId].Layers.Add(layer);
		}

		public void AddLayers(IEnumerable<SynthLayer> layers, int channelId)
		{
			SynthChannel channel = channelsById[channelId];
			channel.Layers.AddRange(layers);
		}

		public void UpdateChannel(int channelId)
		{
			if (channelsById.TryGetValue(channelId, out SynthChannel channel))
			{
				processorsByChannelId[channelId].Process(channel.Layers, channel.RenderTexture);
			}
		}

		public void Dispose()
		{
			foreach (SynthProcessor processor in synthProcessors)
			{
				processor.Dispose();
			}

			synthProcessors = null;
			processorsByChannelId = null;
			channelsById = null;
		}
	}
}
