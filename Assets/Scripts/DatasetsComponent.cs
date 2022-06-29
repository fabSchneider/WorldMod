using System.Collections.Generic;
using UnityEngine;

namespace Fab.WorldMod
{
	[AddComponentMenu("WorldMod/Datasets")]
	public class DatasetsComponent : MonoBehaviour
	{
		private IList<Dataset> stock;
		public IList<Dataset> Stock => stock;

		private Sequence<Dataset> sequence;
		public Sequence<Dataset> Sequence => sequence;

		private void Awake()
		{
			stock = new List<Dataset>();
			sequence = new Sequence<Dataset>();
		}
	}
}
