using System.Collections.Generic;
using Fab.Common;
using Fab.Geo;
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
		private GameObject featurePrefab;

		private List<WorldFeature> featureQueryResult;

		private ObjectPool<GameObject> featureObjectsPool;

		private List<GameObject> activeFeatureObjects;

		[SerializeField]
		private float maxDistance = 500f;

		private void Start()
		{
			featuresComponent = GetComponent<WorldFeaturesComponent>();
			featureObjectsPool = new ObjectPool<GameObject>(4, true, CreateFeatureObject, ResetFeatureObject);
			featureQueryResult = new List<WorldFeature>();
			activeFeatureObjects = new List<GameObject>();
		}

		private void Update()
		{
			Coordinate coord = cameraController.GetCoordinate();

			GetFeaturesInRange(coord, maxDistance, featureQueryResult);

			int count = Mathf.Min(featureQueryResult.Count, activeFeatureObjects.Count);

			for (int i = 0; i < count; i++)
			{
				SetFeatureObject(featureQueryResult[i], activeFeatureObjects[i]);
			}

			if (featureQueryResult.Count < activeFeatureObjects.Count)
			{
				for (int i = activeFeatureObjects.Count - 1; i >= featureQueryResult.Count; i--)
				{
					featureObjectsPool.ReturnToPool(activeFeatureObjects[i]);
					activeFeatureObjects.RemoveAt(i);
				}
			}
			else
			{
				for (int i = activeFeatureObjects.Count; i < featureQueryResult.Count; i++)
				{		
					GameObject obj = featureObjectsPool.GetPooled();
					SetFeatureObject(featureQueryResult[i], obj);
					activeFeatureObjects.Add(obj);
				}
			}
		}

		private GameObject CreateFeatureObject()
		{
			GameObject featureObject = Instantiate(featurePrefab, transform);
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
			foreach (var feature in featuresComponent.FeatureCollection)
			{
				float distance = GeoUtils.Distance(from, feature.Coordinate);
				if (distance < range)
				{
					result.Add(feature);
				}
			}
		}

		private static readonly string nameKey = "name";

		private void SetFeatureObject(WorldFeature feature, GameObject obj)
		{
			if(feature.TryGetData(nameKey, out string name))
			{
				var text = obj.GetComponentInChildren<TextMesh>();
				if (text != null)
					text.text = name;
			}

			Vector3 point = GeoUtils.CoordinateToPoint(feature.Coordinate);
			obj.transform.SetPositionAndRotation(point, Quaternion.LookRotation(-point, cameraController.transform.up));
			obj.SetActive(true);
		}
	}
}
