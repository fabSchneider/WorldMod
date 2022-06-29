using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fab.Synth
{
	/// <summary>
	/// This component is responsible for creating and updating the synthesizer. 
	/// </summary>
	[AddComponentMenu("Synth/Synthesizer")]
	public class SynthComponent : MonoBehaviour
	{
		[SerializeField]
		private SynthChannelData[] channels;

		private Dictionary<string, SynthChannelData> channelsByName;

		private bool[] channelDirtyFlags;

		private bool rebuildChannelsFlag;

		[SerializeField]
		private SynthNodeLibraryAsset nodeLibrary;

		public SynthNodeFactory NodeFactory { get; private set; }

		[SerializeField]
		private Renderer layerRenderer;

		private Material layerMaterial;

		private Synthesizer synthesizer;
		public Synthesizer Synthesizer => synthesizer;

		private List<SynthLayer> synthLayers;

		void Awake()
		{
			synthesizer = new Synthesizer();

			NodeFactory = nodeLibrary.CreateNodeFactory();

			synthLayers = new List<SynthLayer>();

			channelsByName = new Dictionary<string, SynthChannelData>(channels.Length, StringComparer.InvariantCultureIgnoreCase);
			channelDirtyFlags = new bool[channels.Length];
			for (int i = 0; i < channels.Length; i++)
			{
				SynthChannelData layerGroupData = channels[i];
				channelsByName.Add(layerGroupData.Name, layerGroupData);
				channelDirtyFlags[i] = true;
				synthesizer.AddChannel(layerGroupData.Name, layerGroupData.Id, layerGroupData.OutputTexture);
			}

			rebuildChannelsFlag = true;
		}

		private void Start()
		{
			layerMaterial = new Material(layerRenderer.sharedMaterial);
			layerMaterial.name = layerMaterial.name + " (inst)";
			layerRenderer.sharedMaterial = layerMaterial;
		}

		private void Update()
		{
			if (rebuildChannelsFlag)
			{
				synthesizer.ClearAllChannels();
				foreach (var layer in synthLayers)
				{
					SynthChannelData channelData = channelsByName[layer.ChannelName];
					synthesizer.AddLayer(layer, channelData.Id);
				}
				rebuildChannelsFlag = false;
			}

			for (int i = 0; i < channelDirtyFlags.Length; i++)
			{
				if (channelDirtyFlags[i])
				{
					SynthChannelData channelData = channels[i];
					synthesizer.UpdateChannel(channelData.Id);
					channelDirtyFlags[i] = false;
					layerMaterial.SetTexture(channelData.TexturePropertyId, channelData.OutputTexture);
				}
			}
		}
		private void OnDestroy()
		{
			synthesizer.Dispose();
		}

		/// <summary>
		/// Adds a layer to the synthesizer
		/// </summary>
		/// <param name="layer"></param>
		public void AddLayer(SynthLayer layer)
		{
			synthLayers.Add(layer);
			SetLayerDirty(layer);
		}

		/// <summary>
		/// Removes a layer from the synthesizer
		/// </summary>
		/// <param name="layer"></param>
		public void RemoveLayer(SynthLayer layer)
		{
			if(synthLayers.Remove(layer))
				SetChannelDirty(layer.ChannelName);
		}

		/// <summary>
		/// Returns true if the channel exists in the synthesizer
		/// </summary>
		/// <param name="channel"></param>
		/// <returns></returns>
		public bool HasChannel(string channel)
		{
			return channelsByName.ContainsKey(channel);
		}

		/// <summary>
		/// Flags the channel of the supplied layer to be updated for the next frame
		/// </summary>
		/// <param name="layer"></param>

		public void SetLayerDirty(SynthLayer layer)
		{
			if (synthLayers.Contains(layer))
				SetChannelDirty(layer.ChannelName);
		}

		/// <summary>
		/// Flags the channel to be updated for the next frame
		/// </summary>
		/// <param name="channel"></param>

		public void SetChannelDirty(string channel)
		{
			SynthChannelData channelData = channelsByName[channel];
			int channelIndex = Array.IndexOf(channels, channelData);
			if (channelIndex != -1)
				channelDirtyFlags[channelIndex] = true;

			// NOTE: this will always rebuild the channels
			rebuildChannelsFlag = true;
		}
	}
}
