using UnityEngine;

namespace Fab.WorldMod
{
    public class SunController : MonoBehaviour
    {
		public void SetSunPosition(float x, float y)
		{
			transform.rotation = Quaternion.Euler(x, y, 0);
		}

		public float SunX
		{
			get { return transform.eulerAngles.x; }
			set
			{
				transform.eulerAngles = new Vector3(value, transform.eulerAngles.y, 0f);
			}
		}

		public float SunY
		{
			get { return transform.eulerAngles.x; }
			set
			{
				transform.eulerAngles = new Vector3(transform.eulerAngles.x, value, 0f);
			}
		}
	}
}
