using System;
using System.Collections.Generic;
using Fab.Geo;
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
			datasets.Sequence.sequenceChanged += Sequence_sequenceChanged;
		}

		private void Sequence_sequenceChanged(Dataset dataset, DatasetSequence.ChangeEventType changeType, int lastIndex)
		{
			if(changeType == DatasetSequence.ChangeEventType.Added)
			{
				if (dataset.TryGetData(datasetFeaturesKey, out WorldFeatureCollection features))
					featureCollections.Add(features);
			}else if(changeType == DatasetSequence.ChangeEventType.Removed)
			{
				if (dataset.TryGetData(datasetFeaturesKey, out WorldFeatureCollection features))
					featureCollections.Remove(features);
			}
		}
	}
}
