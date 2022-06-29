using System;
using System.Collections;
using System.Collections.Generic;
using Fab.Geo;

namespace Fab.WorldMod
{
	public class WorldFeature
	{
		private Coordinate coord;

		public enum FeatureType
		{
			Point,
			Line,
			Polyline,
			Polygon
		}

		private Dictionary<string, object> data;
		public Coordinate Coordinate => coord;

		public FeatureType Type => FeatureType.Point;

		public WorldFeature(Coordinate coord)
		{
			this.coord = coord;
			data = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
		}

		public void SetData(string key, object value)
		{
			if (!data.TryAdd(key, value))
			{
				data[key] = value;
			}
		}

		public bool TryGetData<T>(string key, out T value)
		{
			if (data.TryGetValue(key, out object obj))
			{
				if (obj is T objT)
				{
					value = objT;
					return true;
				}
			}
			value = default(T);
			return false;
		}
	}

    public class WorldFeatureCollection : IEnumerable<WorldFeature>, ICollection<WorldFeature>
    {
		private List<WorldFeature> features;

		private string name;
		public string Name => name;

		public int Count => features.Count;

		public bool IsReadOnly => false;

		public WorldFeature this[int index] => features[index];

		public WorldFeatureCollection(string name)
		{
			this.name = name;
			features = new List<WorldFeature>();
		}

		public IEnumerator<WorldFeature> GetEnumerator()
		{
			return features.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return features.GetEnumerator();
		}

		public void Add(WorldFeature item)
		{
			features.Add(item);
		}

		public void AddRange(IEnumerable<WorldFeature> items)
		{
			features.AddRange(items);
		}

		public void Clear()
		{
			features.Clear();
		}

		public bool Contains(WorldFeature item)
		{
			return features.Contains(item);
		}

		public void CopyTo(WorldFeature[] array, int arrayIndex)
		{
			features.CopyTo(array, arrayIndex);
		}

		public bool Remove(WorldFeature item)
		{
			return features.Remove(item);
		}
	}
}
