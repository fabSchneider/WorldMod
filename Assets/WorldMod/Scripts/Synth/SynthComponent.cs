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
		private static readonly string datasetLayerKey = "layer";

		[SerializeField]
		private DatasetsComponent datasets;

		[SerializeField]
		private SynthChannelData[] channelsData;

		private Dictionary<string, SynthChannelData> channelsByName;

		private bool[] channelUpdateFlags;

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
			channelUpdateFlags = new bool[channelsData.Length];
			for (int i = 0; i < channelsData.Length; i++)
			{
				SynthChannelData layerGroupData = channelsData[i];
				channelsByName.Add(layerGroupData.Name, layerGroupData);
				channelUpdateFlags[i] = true;
				synthesizer.AddChannel(layerGroupData.Name, layerGroupData.Id, layerGroupData.OutputTexture);
			}

			layersUpdateFlag = true;
		}

		private void Start()
		{
			Signals.Get<DatasetUpdatedSignal>().AddListener(OnDatasetUpdated);
			datasets.Layers.layerChanged += OnLayerChanged;

			layerMaterial = new Material(layerRenderer.sharedMaterial);
			layerMaterial.name = layerMaterial.name + " (inst)";
			layerRenderer.sharedMaterial = layerMaterial;
		}

		private void Update()
		{
			if (layersUpdateFlag)
			{
				synthesizer.ClearAllChannels();
				foreach (var dataset in datasets.Layers)
				{
					if (dataset.TryGetData(datasetLayerKey, out SynthLayer layer))
					{
						SynthChannelData channelData = channelsByName[layer.ChannelName];
						synthesizer.AddLayer(layer, channelData.Id);
					}
				}
				layersUpdateFlag = false;
			}

			for (int i = 0; i < channelUpdateFlags.Length; i++)
			{
				if (channelUpdateFlags[i])
				{
					SynthChannelData channelData = channelsData[i];
					synthesizer.UpdateChannel(channelData.Id);
					channelUpdateFlags[i] = false;
					layerMaterial.SetTexture(channelData.TexturePropertyId, channelData.OutputTexture);
				}
			}
		}

		public bool HasChannel(string channel)
		{
			return channelsByName.ContainsKey(channel);
		}

		private void OnLayerChanged(Dataset dataset, DatasetLayers.ChangeEventType changeType)
		{
			SetGroupDirtyFlag(dataset);
			layersUpdateFlag = true;
		}

		private void OnDatasetUpdated(Dataset dataset)
		{
			if (datasets.Layers.IsLayer(dataset))
			{
				SetGroupDirtyFlag(dataset);
			}
		}

		private void SetGroupDirtyFlag(Dataset dataset)
		{
			if (dataset.TryGetData(datasetLayerKey, out SynthLayer layer))
			{
				SynthChannelData channelData = channelsByName[layer.ChannelName];
				int channelIndex = Array.IndexOf(channelsData, channelData);
				if (channelIndex != -1)
					channelUpdateFlags[channelIndex] = true;
			}
		}

		private void OnDestroy()
		{
			synthesizer.Dispose();
			Signals.Get<DatasetUpdatedSignal>().RemoveListener(OnDatasetUpdated);
		}
	}
}
