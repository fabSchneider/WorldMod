using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fab.WorldMod
{
    public class ProjectionController : MonoBehaviour
    {
		[SerializeField]
		private Camera projectionCamera;

		[SerializeField]
		private float scale = 1f;

		[SerializeField]
		private Vector2 offset;

		public Vector2 Offset => offset;

		public float Scale => scale;

		private void OnValidate()
		{
			SetProjection(offset, scale);
		}

		public void SetProjection(Vector2 offset, float scale)
		{
			if (projectionCamera == null)
				return;

			scale = Mathf.Clamp01(scale);
			float off = (1f - scale) / 2f; 
			projectionCamera.rect = new Rect(offset.x + off, offset.y + off, scale, scale);
		}

	}
}
