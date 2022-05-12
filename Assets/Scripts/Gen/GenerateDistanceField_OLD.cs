using Unity.Mathematics;
using UnityEngine;

namespace Fab.Geo.Gen
{
	public class GenerateDistanceField_OLD
	{
		private static readonly string ComputeShaderPath = "Compute/GenerateDistanceField";
		private static readonly int MaxIterations = 5000;

		private ComputeShader compute;
		private ComputeBuffer changedFlagBuffer;

		public GenerateDistanceField_OLD()
		{
			compute = Resources.Load<ComputeShader>(ComputeShaderPath);

			if (compute == null)
				Debug.LogError("Distance field compute shader could not be found");
		}

		public RenderTexture Generate(Texture2D mask)
		{
			if (changedFlagBuffer == null)
				changedFlagBuffer = new ComputeBuffer(1, sizeof(int));

			RenderTexture tmpMask = CreateMask(mask);

			int passKernel = compute.FindKernel("DistancePass");

			RenderTexture distance = new RenderTexture(new RenderTextureDescriptor(tmpMask.width, tmpMask.height, RenderTextureFormat.RFloat));
			distance.wrapMode = TextureWrapMode.Repeat;
			distance.enableRandomWrite = true;
			distance.name = "Distance";
			Graphics.Blit(tmpMask, distance);

			compute.SetTexture(passKernel, "SqrDistanceField", distance);
			int iY = ExecutePass(distance, new int2(0, 1));
			int iX = ExecutePass(distance, new int2(1, 0));
			Debug.Log(Mathf.Max(iX, iY));

			tmpMask.Release();
			changedFlagBuffer.Release();
			changedFlagBuffer = null;

			int normKernel = compute.FindKernel("NormalizeDistance");
			compute.SetTexture(normKernel, "SqrDistanceField", distance);
			compute.SetInts("resolution", distance.width, distance.height);
			compute.SetInt("maxIteration", Mathf.Max(iX, iY));
			int groupX = (int)math.ceil(distance.width / 8f);
			int groupY = (int)math.ceil(distance.height / 8f);
			compute.Dispatch(normKernel, groupX, groupY, 1);

			return distance;
		}


		private RenderTexture CreateMask(Texture2D source)
		{
			RenderTexture mask = new RenderTexture(new RenderTextureDescriptor(source.width, source.height, RenderTextureFormat.RFloat));
			mask.wrapMode = TextureWrapMode.Repeat;
			mask.enableRandomWrite = true;
			Graphics.Blit(source, mask);

			int prepKernel = compute.FindKernel("PrepareMask");
			compute.SetTexture(prepKernel, "Mask", mask);

			int groupX = (int)math.ceil(mask.width / 8f);
			int groupY = (int)math.ceil(mask.height / 8f);
			compute.Dispatch(prepKernel, groupX, groupY, 1);
			return mask;
		}

		private int ExecutePass(RenderTexture texture, int2 offset)
		{
			compute.SetInts("passOffest", offset.x, offset.y);
			compute.SetInts("resolution", texture.width, texture.height);
			int kernel = compute.FindKernel("DistancePass");

			for (int i = 0; i < MaxIterations; i++)
			{
				changedFlagBuffer.SetData(new int[] { 0 });
				compute.SetBuffer(kernel, "ChangedFlag", changedFlagBuffer);
				compute.SetInt("iterationCount", i);

				int groupX = (int)math.ceil(texture.width / 8f);
				int groupY = (int)math.ceil(texture.height / 8f);
				compute.Dispatch(kernel, groupX, groupY, 1);

				int[] numChanges = new int[1];
				changedFlagBuffer.GetData(numChanges);
				if (numChanges[0] == 0)
					return i;
			}

			Debug.LogError("Max iterations exceeded");
			return MaxIterations;
		}
	}
}
