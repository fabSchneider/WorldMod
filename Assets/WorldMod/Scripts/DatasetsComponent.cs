using UnityEngine;

namespace Fab.WorldMod
{
	[AddComponentMenu("WorldMod/Datasets")]
	public class DatasetsComponent : MonoBehaviour
	{
		private DatasetStock stock;
		public DatasetStock Stock => stock;

		private DatasetSequence sequence;
		public DatasetSequence Sequence => sequence;

		private void Awake()
		{
			stock = new DatasetStock();
			sequence = new DatasetSequence(stock);
		}
	}
}
