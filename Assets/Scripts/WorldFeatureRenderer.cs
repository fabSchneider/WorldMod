using System.Collections.Generic;
using Fab.Common;
using Fab.Geo;
using TMPro;
using UnityEngine;

namespace Fab.WorldMod
{
	[RequireComponent(typeof(WorldFeaturesComponent))]
	public class WorldFeatureRenderer : MonoBehaviour
	{
		private WorldFeaturesComponent featuresComponent;

		[SerializeField]
		private WorldCameraController cameraController;

		[SerializeField]
		private GameObject pointFeaturePrefab;

		private List<WorldFeature> featureQueryResult;

		private ObjectPool<GameObject> pointFeaturesPool;

		private List<GameObject> activePointFeatures;

		[SerializeField]
		private float maxDistance = 500f;

		private void Start()
		{
			featuresComponent = GetComponent<WorldFeaturesComponent>();
			pointFeaturesPool = new ObjectPool<GameObject>(4, true, CreatePointFeature, ResetFeatureObject);
			featureQueryResult = new List<WorldFeature>();
			activePointFeatures = new List<GameObject>();
		}

		private void Update()
		{
			Coordinate coord = cameraController.GetCoordinate();

			GetFeaturesInRange(coord, maxDistance, featureQueryResult);

			int count = Mathf.Min(featureQueryResult.Count, activePointFeatures.Count);

			for (int i = 0; i < count; i++)
			{
				SetPointFeature(featureQueryResult[i], activePointFeatures[i]);
			}

			if (featureQueryResult.Count < activePointFeatures.Count)
			{
				for (int i = activePointFeatures.Count - 1; i >= featureQueryResult.Count; i--)
				{
					pointFeaturesPool.ReturnToPool(activePointFeatures[i]);
					activePointFeatures.RemoveAt(i);
				}
			}
			else
			{
				for (int i = activePointFeatures.Count; i < featureQueryResult.Count; i++)
				{		
					GameObject obj = pointFeaturesPool.GetPooled();
					SetPointFeature(featureQueryResult[i], obj);
					activePointFeatures.Add(obj);
				}
			}
		}

		private GameObject CreatePointFeature()
		{
			GameObject featureObject = Instantiate(pointFeaturePrefab, transform);
			featureObject.name = "Feature Object";
			featureObject.SetActive(false);
			return featureObject;
		}

		private void ResetFeatureObject(GameObject obj)
		{
			obj.SetActive(false);
		}

		private void GetFeaturesInRange(Coordinate from, float range, List<WorldFeature> result)
		{
			result.Clear();
			foreach (var features in featuresComponent.FeatureCollections)
			{
				foreach (var feature in features)
				{
					float distance = GeoUtils.Distance(from, feature.Coordinate);
					if (distance < range)
					{
						result.Add(feature);
					}
				}
			}
		}

		private static readonly string nameKey = "name";

		private void SetPointFeature(WorldFeature feature, GameObject obj)
		{
			if(feature.TryGetData(nameKey, out string name))
			{
				var text = obj.GetComponentInChildren<TextMeshPro>();
				if (text != null)
					text.text = name;
			}

			Vector3 point = GeoUtils.CoordinateToPoint(feature.Coordinate);
			obj.transform.SetPositionAndRotation(point, Quaternion.LookRotation(-point, cameraController.transform.up));
			obj.SetActive(true);
		}
	}
}
