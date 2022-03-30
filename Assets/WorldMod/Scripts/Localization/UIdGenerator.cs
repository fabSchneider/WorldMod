namespace Fab.WorldMod.Localization
{

	public static class UIdGenerator 
	{
		private static int sequencePos = 0;
		public static int NextID()
		{
			return --sequencePos;
		}
	}
}
