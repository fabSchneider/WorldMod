using UnityEngine;

namespace Fab.WorldMod
{
	[AddComponentMenu("WorldMod/Datasets")]
	public class DatasetsComponent : MonoBehaviour
	{
		private Datasets datasets;
		public Datasets Datasets => datasets;

		private void Awake()
		{
			datasets = new Datasets();
		}
	}
}
