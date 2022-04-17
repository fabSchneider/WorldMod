using UnityEngine;

namespace Fab.WorldMod.Synth
{
	/// <summary>
	/// This class is the base for a single process in the synthesize chain.
	/// </summary>
	public abstract class SynthNode
	{
		public string Name => shader.name;

		protected Shader shader;
		protected Material blitMaterial;

		public Material Material => blitMaterial;

		public SynthNode(Shader shader)
		{
			this.shader = shader;
			blitMaterial = new Material(shader);
		}
	}

	/// <summary>
	/// This class represents a generative process that outputs into the synthesize chain.
	/// </summary>
	public class GenNode : SynthNode
	{
		public GenNode(Shader shader) : base(shader) { }

		public void Generate(RenderTexture result)
		{
			Graphics.Blit(null, result, blitMaterial, 0);
		}
	}

	/// <summary>
	/// This class represents a mutative process on an input in the synthesize chain.
	/// </summary>
	public class MutateNode : SynthNode
	{
		public MutateNode(Shader shader) : base(shader) { }

		public void Mutate(Texture inTex, RenderTexture outTex)
		{
			Graphics.Blit(inTex, outTex, blitMaterial, 0);
		}

	}

	/// <summary>
	/// This class represents a blend process of two inputs in the synthesize chain.
	/// </summary>
	public class BlendNode : SynthNode
	{
		private static readonly string BaseTexPropID = "_BaseTex";

		public BlendNode(Shader shader) : base(shader) { }

		public void Blend(Texture baseTex, Texture blendTex, RenderTexture resultBuffer)
		{
			blitMaterial.SetTexture(BaseTexPropID, baseTex);
			Graphics.Blit(blendTex, resultBuffer, blitMaterial, 0);
		}
	}
}
