using UnityEngine;

namespace Fab.WorldMod
{
	[ExecuteInEditMode]
	[AddComponentMenu("Face Camera")]
	public class FaceCamera : MonoBehaviour
	{
		void Update()
		{
			transform.rotation = Quaternion.LookRotation(
				transform.position - Camera.main.transform.position,
				Camera.main.transform.up);
		}
	}
}
