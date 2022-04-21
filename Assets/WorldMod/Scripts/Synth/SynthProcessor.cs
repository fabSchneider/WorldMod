using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fab.WorldMod.Synth
{
	/// <summary>
	/// This class is responsible for processing and synthesizing a collection of layers. 
	/// </summary>
    public class SynthProcessor
    {
		private RenderTextureDescriptor bufferDescriptor;
		public RenderTextureDescriptor BufferDescriptor => bufferDescriptor;	

		private RenderTexture swapBufferA;
		private RenderTexture swapBufferB;
		private RenderTexture storeBuffer;

		public SynthProcessor(string name, RenderTextureDescriptor bufferDescriptor)
		{
			this.bufferDescriptor = bufferDescriptor;	
			swapBufferA = new RenderTexture(bufferDescriptor);
			swapBufferA.name = name + " Swap Buffer A";
			swapBufferB = new RenderTexture(bufferDescriptor);
			swapBufferB.name = name + " Swap Buffer B";
			storeBuffer = new RenderTexture(bufferDescriptor);
			storeBuffer.name = name + " Store Buffer A";
		}

		public void Process(List<SynthLayer> layers, RenderTexture result)
		{
			//clear store buffer
			ClearRenderTexture(storeBuffer);
			ClearRenderTexture(result);

			if (layers.Count == 0)
				return;

			for (int i = 0; i < layers.Count - 1; i++)
			{
				SynthLayer layer = layers[i];
				ProcessLayer(layer, storeBuffer);
			}

			ProcessLayer(layers[layers.Count - 1], result);
		}

		private void ProcessLayer(SynthLayer layer, RenderTexture result)
		{
			if (!GetLayerBaseTexture(layer, swapBufferA, out Texture baseTex))
				return;

			Texture mutateTex;
			if (baseTex == swapBufferA)
				mutateTex = MutateTexture(layer, baseTex, swapBufferB, swapBufferA);
			else
				mutateTex = MutateTexture(layer, baseTex, swapBufferA, swapBufferB);

			RenderTexture blendResult = mutateTex == swapBufferA ? swapBufferB : swapBufferA;

			BlendTexture(layer, storeBuffer, mutateTex, blendResult);
			Graphics.Blit(blendResult, result);
		}

		private void ClearRenderTexture(RenderTexture renderTexture)
		{
			RenderTexture rt = RenderTexture.active;
			RenderTexture.active = renderTexture;
			GL.Clear(true, true, Color.clear);
			RenderTexture.active = rt;
		}

		private bool GetLayerBaseTexture(SynthLayer layer, RenderTexture buffer, out Texture result)
		{
			if (layer.InputTexture != null)
			{
				result = layer.InputTexture;
				return true;
			}
			else if (layer.GenerateNode != null)
			{
				layer.GenerateNode.Generate(buffer);
				result = buffer;
				return true;
			}
			result = null;
			return false;
		}

		private Texture MutateTexture(SynthLayer layer, Texture baseTex, RenderTexture bufferA, RenderTexture bufferB)
		{
			if (layer.MutateNodes.Count == 0)
				return baseTex;

			IReadOnlyList<MutateNode> mutateNodes = layer.MutateNodes;
			mutateNodes[0].Mutate(baseTex, bufferA);
			RenderTexture sourceBuffer = bufferA;
			RenderTexture destinationBuffer = bufferB;

			for (int i = 1; i < mutateNodes.Count; i++)
			{
				mutateNodes[i].Mutate(sourceBuffer, destinationBuffer);
				RenderTexture newDestination = sourceBuffer;
				sourceBuffer = destinationBuffer;
				destinationBuffer = newDestination;
			}

			return sourceBuffer;
		}

		private void BlendTexture(SynthLayer layer, Texture baseTex, Texture blendTex, RenderTexture resultBuffer)
		{
			if (layer.BlendNode != null)
				layer.BlendNode.Blend(baseTex, blendTex, resultBuffer);
			else
				Graphics.Blit(blendTex, resultBuffer);
		}

		public void Dispose()
		{
			swapBufferA.Release();
			swapBufferB.Release();
			storeBuffer.Release();
		}
	}
}
