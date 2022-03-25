using UnityEngine;

namespace Fab.WorldMod
{
	[AddComponentMenu("WorldMod/Datasets")]
	public class DatasetsComponent : MonoBehaviour
	{
		private DatasetStock stock;
		public DatasetStock Stock => stock;

		private DatasetLayers layers;
		public DatasetLayers Layers => layers;

		private void Awake()
		{
			stock = new DatasetStock();
			layers = new DatasetLayers(stock);
		}
	}
}
