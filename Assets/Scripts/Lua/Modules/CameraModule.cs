using Fab.Common;
using Fab.Geo;
using Fab.Lua.Core;
using MoonSharp.Interpreter;

namespace Fab.WorldMod.Lua.Interop
{
	[LuaName("camera")]
	[LuaHelpInfo("Module for controlling the world camera")]
	public class CameraModule : LuaObject, ILuaObjectInitialize
	{
		private IWorldCameraController cameraController;
		private IWorldCameraAnimator animator;

		private Closure onAnimationFinished;
		public void Initialize()
		{
			cameraController = SceneUtils.Find<IWorldCameraController>();

			if (cameraController == null)
				throw new LuaObjectInitializationException("Could not find camera controller");

			animator = SceneUtils.Find<IWorldCameraAnimator>();
		}

		[LuaHelpInfo("Gets/Sets the camera's position in coordinates")]
		public Coordinate coord
		{
			get => cameraController.GetCoordinate();
			set => cameraController.SetCoordinate(value);
		}

		[LuaHelpInfo("Sets the camera's zoom level [0-1]")]
		public void set_zoom(float zoom)
		{
			cameraController.SetZoom(zoom);
		}

		[LuaHelpInfo("Enables the camera's input control")]
		public void enable_control() => cameraController.ControlEnabled = true;

		[LuaHelpInfo("Disables the camera's input control")]
		public void disable_control() => cameraController.ControlEnabled = false;

		[LuaHelpInfo("Moves the camera from one coordinate to the next in a list of coordinates")]
		public void animate(Coordinate[] coords, float speed, bool loop = false)
		{
			animator?.Animate(coords, speed, loop);
		}

		[LuaHelpInfo("Called when a camera animation finished")]
		public void on_animation_finished(Closure evt)
		{
			if (animator == null)
				return;

			onAnimationFinished = evt;
			animator.OnAnimationFinished -= OnAnimationFinished;
			if (evt != null)
				animator.OnAnimationFinished += OnAnimationFinished;
		}

		private void OnAnimationFinished()
		{
			if (onAnimationFinished != null)
				onAnimationFinished.Call();
		}
	}
}
