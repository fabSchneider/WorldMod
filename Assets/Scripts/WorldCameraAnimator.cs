using System;
using System.Collections;
using Fab.Geo;
using UnityEngine;

namespace Fab.WorldMod
{
	public interface IWorldCameraAnimator
	{
		void Animate(Coordinate[] coords, float speed, bool loop);

		event Action OnAnimationFinished;
	}

	[AddComponentMenu("FabGeo/World Camera Animator")]
	public class WorldCameraAnimator : MonoBehaviour, IWorldCameraAnimator
	{
		private IWorldCameraController controller;

		private void Start()
		{
			controller = GetComponent<IWorldCameraController>();
			if (controller == null)
				Debug.LogError("World Camera animator needs a Camera controller to work");
		}

		public event Action OnAnimationFinished;

		private Coroutine animationRoutine;

		/// <summary>
		/// Moves the camera from one coordinate to the next in a list of coordinates
		/// </summary>
		/// <param name="coords"></param>
		/// <param name="speed"></param>
		/// <param name="loop"></param>
		public void Animate(Coordinate[] coords, float speed, bool loop)
		{
			if (controller == null)
				return;

			if (animationRoutine != null)
				StopCoroutine(animationRoutine);

			animationRoutine = StartCoroutine(AnimateCoroutine(coords, speed, loop));
		}

		IEnumerator AnimateCoroutine(Coordinate[] coords, float speed, bool loop)
		{
			if (coords != null && coords.Length > 0)
			{
				int i = 0;
				while (true)
				{
					Coordinate coord = coords[i];
					Vector3 target = GeoUtils.LonLatToPoint(coord);
					Vector3 current = GeoUtils.LonLatToPoint(controller.GetCoordinate());

					while (current != target)
					{
						current = Vector3.MoveTowards(current, target, Time.deltaTime * speed);
						controller.SetCoordinate(GeoUtils.PointToLonLat(current));
						yield return null;
					}
					i++;

					if (i == coords.Length)
					{
						if (loop)
							i = 0;
						else
							break;
					}
				}

				OnAnimationFinished?.Invoke();
			}
		}

	}
}
