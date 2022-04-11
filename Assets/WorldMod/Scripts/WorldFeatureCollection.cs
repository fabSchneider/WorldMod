using System;
using System.Collections;
using System.Collections.Generic;
using Fab.Geo;

namespace Fab.WorldMod
{
	public class WorldFeature
	{
		private Coordinate coord;

		private Dictionary<string, object> data;
		public Coordinate Coordinate => coord;

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

    public class WorldFeatureCollection : IEnumerable<WorldFeature>
    {
		private List<WorldFeature> features;
		public List<WorldFeature> Features => features;

		private string name;
		public string Name => name;

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
	}
}
