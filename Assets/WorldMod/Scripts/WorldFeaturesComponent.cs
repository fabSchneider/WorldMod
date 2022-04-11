using System;
using System.Collections.Generic;
using Fab.Geo;
using UnityEngine;

namespace Fab.WorldMod
{
	[AddComponentMenu("WorldMod/World Features")]
    public class WorldFeaturesComponent : MonoBehaviour
    {

		[Serializable]
		public struct WorldFeatureData
		{
			public string name;
			public Vector2 coordinate;
		}

		private WorldFeatureCollection featureCollection;

		public WorldFeatureCollection FeatureCollection => featureCollection;

		[SerializeField]
		private List<WorldFeatureData> featureData;

		private void Awake()
		{
			featureCollection = new WorldFeatureCollection(name);

			foreach (var featureData in featureData)
			{
				WorldFeature feature = new WorldFeature(new Coordinate(featureData.coordinate.x, featureData.coordinate.y));
				feature.SetData("name", featureData.name);
				featureCollection.Features.Add(feature);
			}
		}
	}
}
