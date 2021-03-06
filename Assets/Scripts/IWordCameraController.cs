using Fab.Geo;

namespace Fab.WorldMod
{
    public interface IWorldCameraController
    {
		public abstract Coordinate GetCoordinate();
		public abstract void SetCoordinate(Coordinate coord);
		public abstract void SetZoom(float zoom);
		public abstract bool ControlEnabled { get; set; }
	}
}
