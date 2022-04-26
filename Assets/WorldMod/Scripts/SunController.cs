using Fab.Geo;
using UnityEngine;

namespace Fab.WorldMod
{
    public class SunController : MonoBehaviour
    {
		[SerializeField]
		private Transform followTransform;

		private bool followCamera;
		public bool FollowCamera
		{
			get => followCamera;
			set
			{
				if (value == followCamera)
					return;

				if (value)
				{
					followCamera = true;
					AttachToCamera();
				}
				else
				{
					followCamera = false;
					DetachFromCamera();
				}
			}
		}

		private void Start()
		{
			followCamera = transform.parent == followTransform;
		}

		private void AttachToCamera()
		{
			transform.parent = followTransform.transform;
			transform.localRotation = Quaternion.identity;
		}

		private void DetachFromCamera()
		{
			transform.parent = null;
		}

		public void SetSunPosition(float x, float y)
		{
			FollowCamera = false;
			transform.rotation = Quaternion.Euler(x, y, 0);
		}

		public Coordinate Zenith => GeoUtils.PointToCoordinate(-transform.forward);

		public float SunX
		{
			get { return transform.eulerAngles.x; }
			set
			{
				FollowCamera = false;
				transform.eulerAngles = new Vector3(value, transform.eulerAngles.y, 0f);
			}
		}

		public float SunY
		{
			get { return transform.eulerAngles.y; }
			set
			{
				FollowCamera = false;
				transform.eulerAngles = new Vector3(transform.eulerAngles.x, value, 0f);
			}
		}
	}
}
