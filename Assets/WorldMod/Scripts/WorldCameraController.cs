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
		private float drag = 0.9f;


		public Transform targetTransform;

		private Quaternion spinRotation;
		private float spinEnergy;

		public bool ControlEnabled
		{
			get => enabled;
			set => enabled = value;
		}

		private void Update()
		{
			targetTransform.rotation =  Quaternion.Slerp(targetTransform.rotation, targetTransform.rotation * spinRotation, spinEnergy);
			spinEnergy *=  1f - drag * Time.unscaledDeltaTime;
		}

		public void SpinCamera(Vector3 axis)
		{
			spinRotation = Quaternion.Euler(axis.y * panSpeed, axis.x * panSpeed, axis.z * orbitSpeed);
			spinEnergy = 1f;
		}

		public Coordinate GetCoordinate()
		{
			return GeoUtils.PointToCoordinate(-targetTransform.forward);
		}

		public void SetCoordinate(Coordinate coord)
		{
			Vector3 to = GeoUtils.LonLatToPoint(coord.longitude, coord.latitude);
			targetTransform.rotation = Quaternion.LookRotation(-to, targetTransform.up);
		}

		public void SetZoom(float zoom)
		{
			Debug.LogError("Setting zoom not supported");
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
