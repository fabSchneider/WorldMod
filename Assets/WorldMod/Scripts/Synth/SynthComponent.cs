using System;
using System.Collections.Generic;
using Fab.Common;
using UnityEngine;

namespace Fab.WorldMod.Synth
{
	[Serializable]
	public class SynthChannelData : ISerializationCallbackReceiver
	{
		[SerializeField]
		private string name;
		[SerializeField]
		private string targetPropertyName;
		[SerializeField]
		private CustomRenderTexture outputTexture;

		private int targetPropertyId;

		public string Name => name;
		public int TexturePropertyId  => targetPropertyId;
		public int Id => targetPropertyId;
		public CustomRenderTexture OutputTexture  => outputTexture; 

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			targetPropertyId = Shader.PropertyToID(targetPropertyName);
		}

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
			
		}
	}

	/// <summary>
	/// This component is responsible for creating and updating the synthesizer with data from the scene. 
	/// </summary>
	[AddComponentMenu("WorldMod/Synthesizer")]
	public class SynthComponent : MonoBehaviour
    {
		private static readonly string datasetSynthKey = "synth";

		[SerializeField]
		private DatasetsComponent datasets;

		[SerializeField]
		private SynthChannelData[] channelsData;

		private Dictionary<string, SynthChannelData> channelsByName;

		private bool[] channelDirtyFlags;

		private bool layersUpdateFlag;

		[SerializeField]
		private SynthNodeLibraryAsset nodeLibrary;

		public SynthNodeFactory NodeFactory { get; private set; }

		[SerializeField]
		private Renderer layerRenderer;

		private Material layerMaterial;

		private Synthesizer synthesizer;
		public Synthesizer Synthesizer => synthesizer;

		void Awake()
		{
			synthesizer = new Synthesizer();

			NodeFactory = nodeLibrary.CreateNodeFactory();

			channelsByName = new Dictionary<string, SynthChannelData>(channelsData.Length, StringComparer.InvariantCultureIgnoreCase);
			channelDirtyFlags = new bool[channelsData.Length];
			for (int i = 0; i < channelsData.Length; i++)
			{
				SynthChannelData layerGroupData = channelsData[i];
				channelsByName.Add(layerGroupData.Name, layerGroupData);
				channelDirtyFlags[i] = true;
				synthesizer.AddChannel(layerGroupData.Name, layerGroupData.Id, layerGroupData.OutputTexture);
			}

			layersUpdateFlag = true;
		}

		private void Start()
		{
			Signals.Get<DatasetUpdatedSignal>().AddListener(OnDatasetUpdated);
			datasets.Sequence.sequenceChanged += OnSequenceChanged;

			layerMaterial = new Material(layerRenderer.sharedMaterial);
			layerMaterial.name = layerMaterial.name + " (inst)";
			layerRenderer.sharedMaterial = layerMaterial;
		}

		private void Update()
		{
			if (layersUpdateFlag)
			{
				synthesizer.ClearAllChannels();
				foreach (var dataset in datasets.Sequence)
				{
					if (dataset.TryGetData(datasetSynthKey, out SynthLayer layer))
					{
						SynthChannelData channelData = channelsByName[layer.ChannelName];
						synthesizer.AddLayer(layer, channelData.Id);
					}
				}
				layersUpdateFlag = false;
			}

			for (int i = 0; i < channelDirtyFlags.Length; i++)
			{
				if (channelDirtyFlags[i])
				{
					SynthChannelData channelData = channelsData[i];
					synthesizer.UpdateChannel(channelData.Id);
					channelDirtyFlags[i] = false;
					layerMaterial.SetTexture(channelData.TexturePropertyId, channelData.OutputTexture);
				}
			}
		}

		public bool HasChannel(string channel)
		{
			return channelsByName.ContainsKey(channel);
		}

		private void OnSequenceChanged(SequenceChangedEvent<Dataset> evt)
		{
			SetChannelDirtyFlag(evt.data);
			layersUpdateFlag = true;
		}

		private void OnDatasetUpdated(Dataset dataset)
		{
			if (datasets.Sequence.Contains(dataset))
			{
				SetChannelDirtyFlag(dataset);
			}
		}

		private void SetChannelDirtyFlag(Dataset dataset)
		{
			if (dataset.TryGetData(datasetSynthKey, out SynthLayer layer))
			{
				SetLayerDirty(layer);
			}
		}

		public void SetLayerDirty(SynthLayer layer)
		{
			// NOTE: This does not check if the layer actually belongs to the channel. It will update the channel regardless.
			SynthChannelData channelData = channelsByName[layer.ChannelName];
			int channelIndex = Array.IndexOf(channelsData, channelData);
			if (channelIndex != -1)
				channelDirtyFlags[channelIndex] = true;
		}

		private void OnDestroy()
		{
			synthesizer.Dispose();
			Signals.Get<DatasetUpdatedSignal>().RemoveListener(OnDatasetUpdated);
		}
	}
}
