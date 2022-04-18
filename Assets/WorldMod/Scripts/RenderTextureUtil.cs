using UnityEngine;

namespace Fab.WorldMod
{
    public static class RenderTextureUtil 
    {
		public static void ClearRenderTexture(RenderTexture renderTexture)
		{
			RenderTexture rt = RenderTexture.active;
			RenderTexture.active = renderTexture;
			GL.Clear(true, true, Color.clear);
			RenderTexture.active = rt;
		}
	}
}
