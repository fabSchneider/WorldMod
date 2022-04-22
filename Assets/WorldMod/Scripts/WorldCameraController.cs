using Fab.Geo;
using UnityEngine;

namespace Fab.WorldMod
{

	[AddComponentMenu("WorldMod/World Camera Controller")]
	public class WorldCameraController : MonoBehaviour, IWorldCameraController
	{
		[SerializeField]
		private float panSpeed = 1f;
		[SerializeField]
		private float orbitSpeed = 1f;

		[SerializeField]
		private float zoomSpeed = 1f;
		[SerializeField]
		private Vector2 zoomBounds;

		[SerializeField]
		private float drag = 0.9f;

		[SerializeField]
		private Transform targetTransform;

		[SerializeField]
		private Camera _camera;

		private Quaternion spinRotation;
		private float spinEnergy;

		private float zoom;
		private float zoomEnergy;

		public bool ControlEnabled
		{
			get => enabled;
			set => enabled = value;
		}

		private void Update()
		{
			targetTransform.rotation = Quaternion.Slerp(targetTransform.rotation, targetTransform.rotation * spinRotation, spinEnergy);
			spinEnergy *= 1f - drag * Time.unscaledDeltaTime;

			float z;
			if (_camera.orthographic)
			{
				z = Mathf.Lerp(_camera.orthographicSize, _camera.orthographicSize + zoom, zoomEnergy);
				z = Mathf.Clamp(z, zoomBounds.x, zoomBounds.y);
				_camera.orthographicSize = z;
			}
			else
			{
				z = Mathf.Lerp(transform.localPosition.z, transform.localPosition.z + zoom, zoomEnergy);
				z = Mathf.Clamp(z, zoomBounds.x, zoomBounds.y);
				transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, z);
			}
			zoomEnergy *= 1f - drag * Time.unscaledDeltaTime;
		}

		public void SpinCamera(Vector4 axis)
		{
			spinRotation = Quaternion.Euler(axis.y * panSpeed, axis.x * panSpeed, axis.z * orbitSpeed);
			spinEnergy = 1f;

			zoom = axis.w * zoomSpeed;
			zoomEnergy = 1f;
		}

		public Coordinate GetCoordinate()
		{
			return GeoUtils.PointToCoordinate(-targetTransform.forward);
		}

		public float GetHeading()
		{
			return targetTransform.eulerAngles.z > 180f ? targetTransform.eulerAngles.z - 360f : targetTransform.eulerAngles.z;
		}

		public void SetCoordinate(Coordinate coord)
		{
			Vector3 to = GeoUtils.LonLatToPoint(coord.longitude, coord.latitude);
			targetTransform.rotation = Quaternion.LookRotation(-to, targetTransform.up);
		}

		public void SetZoom(float zoom)
		{
			zoom = Mathf.Clamp(zoom, zoomBounds.x, zoomBounds.y);

			if (_camera.orthographic)
				_camera.orthographicSize = zoom;
			else
				transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, zoom);
		}

		public float GetZoom()
		{
			if (_camera.orthographic)
				return Mathf.InverseLerp(zoomBounds.x, zoomBounds.y, _camera.orthographicSize);
			else
				return Mathf.InverseLerp(zoomBounds.x, zoomBounds.y, transform.localPosition.z);
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.green;
			Gizmos.DrawLine(Vector3.zero, spinRotation * Vector3.forward * 2);

			Gizmos.color = Color.red;
			Gizmos.DrawLine(Vector3.zero, targetTransform.rotation * Vector3.forward * 2);
		}
	}
}
