
using UnityEngine;

namespace Fab.WorldMod.Gen
{
	public class ReverseGradientGenerator
	{
		private static readonly string ComputeShaderPath = "Compute/ReverseGradient";

		private ComputeShader compute;

		public ReverseGradientGenerator()
		{
			compute = Resources.Load<ComputeShader>(ComputeShaderPath);

			if (compute == null)
				Debug.LogError("Reverse Gradient compute shader could not be found");
		}

		public RenderTexture Generate(Texture2D gradient, Texture2D source)
		{
			RenderTexture dst = new RenderTexture(new RenderTextureDescriptor(source.width, source.height, RenderTextureFormat.RFloat));
			dst.enableRandomWrite = true;


			int kernel = compute.FindKernel("CSMain");

			compute.SetInt("SampleSize", gradient.width);
			compute.SetTexture(kernel, "Gradient", gradient);
			compute.SetTexture(kernel, "Source", source);
			compute.SetTexture(kernel, "Result", dst);


			int groupX = (int)Mathf.Ceil(source.width / 8f);
			int groupY = (int)Mathf.Ceil(source.height / 8f);
			compute.Dispatch(kernel, groupX, groupY, 1);

			return dst;
		}
	}
}
