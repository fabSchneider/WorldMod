using Fab.Lua.Core;
using UnityEngine;

namespace Fab.Lua.ConsoleSample
{
	[LuaHelpInfo("Controls the cube on screen")]
	[LuaName("cube")]
	public class CubeModule : LuaObject, ILuaObjectInitialize
	{
		private Rigidbody cubeRB;
		private GameObject arrow;
		private Vector3 force;
		public void Initialize()
		{
			cubeRB = Object.FindObjectOfType<Rigidbody>();
			arrow = GameObject.Find("Arrow");
			if (cubeRB == null)
				throw new LuaObjectInitializationException("The cube was not found");
			if (arrow == null)
				throw new LuaObjectInitializationException("The arrow was not found");

			force = arrow.transform.forward;
		}

		[LuaHelpInfo("Sets the point at which to apply the force")]
		public void set_point(float x, float y, float z)
		{
			arrow.transform.position = new Vector3(x, y, z);
		}

		[LuaHelpInfo("Sets the force to be applied to the cube")]
		public void set_force(float x, float y, float z)
		{
			force = new Vector3(x, y, z);
			arrow.transform.rotation = Quaternion.LookRotation(force, Camera.main.transform.right);
		}

		[LuaHelpInfo("Applies the force to the cube")]
		public void apply_force()
		{
			cubeRB.AddForceAtPosition(force, arrow.transform.position, ForceMode.Impulse);
		}
	}
}
