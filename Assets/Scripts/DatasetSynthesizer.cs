using UnityEngine;
using Fab.Synth;
using Fab.Common;
using System.Collections.Generic;

namespace Fab.WorldMod
{
	/// <summary>
	/// Updates the synthesizer with the data from the datasets
	/// </summary>
	[AddComponentMenu("WorldMod/Dataset Synthesizer")]
	public class DatasetSynthesizer : MonoBehaviour
	{
		private static readonly string datasetSynthKey = "synth";

		[SerializeField]
		private DatasetsComponent datasets;

		[SerializeField]
		private SynthComponent synth;

		void Start()
		{
			Signals.Get<DatasetUpdatedSignal>().AddListener(OnDatasetUpdated);

			datasets.Sequence.sequenceChanged -= OnSequenceChanged;
			datasets.Sequence.sequenceChanged += OnSequenceChanged;
		}

		public void OnEnable()
		{
			Signals.Get<DatasetUpdatedSignal>().AddListener(OnDatasetUpdated);
			if (datasets.Sequence != null)
				datasets.Sequence.sequenceChanged += OnSequenceChanged;
		}

		private void OnDisable()
		{
			Signals.Get<DatasetUpdatedSignal>().RemoveListener(OnDatasetUpdated);
			if (datasets)
			{
				datasets.Sequence.sequenceChanged += OnSequenceChanged;
			}
		}

		private void OnSequenceChanged(SequenceChangedEvent<Dataset> evt)
		{
			if (evt.data.TryGetData(datasetSynthKey, out IEnumerable<object> layers))
			{
				foreach (var item in layers)
				{
					if (item is SynthLayer layer)
					{
						switch (evt.changeType)
						{
							case SequenceChangeEventType.Added:
								synth.AddLayer(layer);
								break;
							case SequenceChangeEventType.Moved:
								synth.SetChannelDirty(layer.ChannelName);
								break;
							case SequenceChangeEventType.Removed:
								synth.RemoveLayer(layer);
								break;
							default:
								break;
						}
					}
				}
			}
			else if (evt.data.TryGetData(datasetSynthKey, out SynthLayer layer))
			{
				switch (evt.changeType)
				{
					case SequenceChangeEventType.Added:
						synth.AddLayer(layer);
						break;
					case SequenceChangeEventType.Moved:
						synth.SetChannelDirty(layer.ChannelName);
						break;
					case SequenceChangeEventType.Removed:
						synth.RemoveLayer(layer);
						break;
					default:
						break;
				}
			}
		}

		private void OnDatasetUpdated(Dataset dataset)
		{
			if (datasets.Sequence.Contains(dataset))
				SetChannelDirtyFlag(dataset);
		}

		private void SetChannelDirtyFlag(Dataset dataset)
		{
			if (dataset.TryGetData(datasetSynthKey, out IEnumerable<object> layers))
			{
				foreach (var item in layers)
				{
					if (item is SynthLayer layer)
						synth.SetChannelDirty(layer.ChannelName);
				}
			}
			else if (dataset.TryGetData(datasetSynthKey, out SynthLayer layer))
			{
				synth.SetChannelDirty(layer.ChannelName);
			}
		}
	}
}
