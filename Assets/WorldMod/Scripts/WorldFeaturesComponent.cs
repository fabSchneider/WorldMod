using System.Collections.Generic;
using UnityEngine;

namespace Fab.WorldMod
{
	[AddComponentMenu("WorldMod/World Features")]
    public class WorldFeaturesComponent : MonoBehaviour
    {
		private static readonly string datasetFeaturesKey = "features";

		private List<WorldFeatureCollection> featureCollections;
		public IReadOnlyList<WorldFeatureCollection> FeatureCollections => featureCollections;

		[SerializeField]
		private DatasetsComponent datasets;


		private void Awake()
		{
			featureCollections = new List<WorldFeatureCollection>();
			datasets.Sequence.sequenceChanged += OnSequenceChanged;
		}

		private void OnSequenceChanged(SequenceChangedEvent<Dataset> evt)
		{
			if(evt.changeType == SequenceChangeEventType.Added)
			{
				if (evt.data.TryGetData(datasetFeaturesKey, out WorldFeatureCollection features))
					featureCollections.Add(features);
			}else if(evt.changeType == SequenceChangeEventType.Removed)
			{
				if (evt.data.TryGetData(datasetFeaturesKey, out WorldFeatureCollection features))
					featureCollections.Remove(features);
			}
		}
	}
}
