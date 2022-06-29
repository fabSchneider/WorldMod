using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fab.Common
{
	public static class SceneUtils
	{
		/// <summary>
		/// Finds all components of the specified type in the specified scene.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="scene"></param>
		/// <returns></returns>
		public static List<T> FindAll<T>(Scene scene)
		{
			List<T> interfaces = new List<T>();
			GameObject[] rootGameObjects = scene.GetRootGameObjects();
			foreach (var rootGameObject in rootGameObjects)
			{
				T[] childrenInterfaces = rootGameObject.GetComponentsInChildren<T>();
				foreach (var childInterface in childrenInterfaces)
				{
					interfaces.Add(childInterface);
				}
			}
			return interfaces;
		}

		/// <summary>
		/// Finds all components of the specified type in the active scene.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static List<T> FindAll<T>()
		{
			return FindAll<T>(SceneManager.GetActiveScene());
		}

		/// <summary>
		/// Finds the first component of the specified type in the specified scene.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="scene"></param>
		/// <returns></returns>
		public static T Find<T>(Scene scene)
		{
			GameObject[] rootGameObjects = scene.GetRootGameObjects();
			foreach (var rootGameObject in rootGameObjects)
			{
				T foundInterface = rootGameObject.GetComponentInChildren<T>();
				if(foundInterface != null)
					return foundInterface;
			}
			return default(T);
		}

		/// <summary>
		/// Finds the first component of the specified type in the active scene.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="scene"></param>
		/// <returns></returns>
		public static T Find<T>()
		{
			return Find<T>(SceneManager.GetActiveScene());
		}
	}
}
