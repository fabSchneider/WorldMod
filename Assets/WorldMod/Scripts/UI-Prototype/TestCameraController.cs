using UnityEngine;

public class TestCameraController : MonoBehaviour
{
	public float speed = 1f;
	public float lag = 1f;

	public Transform targetTransform;

	private Quaternion targetRot;
	public void MoveCamera(Vector2 axis)
	{
		targetRot = Quaternion.AngleAxis(axis.x * speed, targetTransform.up) *
			Quaternion.AngleAxis(axis.y * speed, targetTransform.right) * targetTransform.rotation;
	}

	private void Update()
	{
		targetTransform.rotation = targetRot;// Quaternion.Slerp(targetTransform.rotation, targetRot, lag * Time.deltaTime);
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawLine(Vector3.zero, targetRot * Vector3.forward * 2);

		Gizmos.color = Color.red;
		Gizmos.DrawLine(Vector3.zero, targetTransform.rotation * Vector3.forward * 2);
	}
}
