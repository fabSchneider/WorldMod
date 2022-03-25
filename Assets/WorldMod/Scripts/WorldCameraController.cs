using Fab.Geo;
using UnityEngine;

[AddComponentMenu("WorldMod/World Camera Controller")]
public class WorldCameraController : MonoBehaviour, IWorldCameraController
{
	public float speed = 1f;

	public Transform targetTransform;

	private Quaternion targetRot;

	public bool ControlEnabled { get; set; } = true;

	public void MoveCamera(Vector2 axis)
	{
		if (ControlEnabled)
			targetTransform.rotation = Quaternion.AngleAxis(axis.x * speed, targetTransform.up) *
										Quaternion.AngleAxis(axis.y * speed, targetTransform.right) * targetTransform.rotation;
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawLine(Vector3.zero, targetRot * Vector3.forward * 2);

		Gizmos.color = Color.red;
		Gizmos.DrawLine(Vector3.zero, targetTransform.rotation * Vector3.forward * 2);
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
}
