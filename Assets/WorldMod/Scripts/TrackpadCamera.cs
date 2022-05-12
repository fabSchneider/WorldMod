using UnityEngine;

namespace Fab.WorldMod
{
	[RequireComponent(typeof(Camera))]
    public class TrackpadCamera : MonoBehaviour
    {
		private Camera trackpadCamera;

		[SerializeField]
		private Camera worldCamera;

		private void Start()
		{
			trackpadCamera = GetComponent<Camera>();
		}

		public void Update()
		{
			if (worldCamera.orthographic)
			{
				trackpadCamera.orthographicSize = worldCamera.orthographicSize;
			}
		}
	}
}
