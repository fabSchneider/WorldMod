using UnityEngine;
using UnityEngine.Rendering;

namespace Fab.Synth
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

		public void SetFloat(SynthNodeDescriptor.PropertyDescriptor propDescriptor, float value)
		{
			blitMaterial.SetFloat(propDescriptor.PropName, value);
		}

		public void SetFloat(string propName, float value)
		{
			blitMaterial.SetFloat(propName, value);
		}

		public void SetColor(SynthNodeDescriptor.PropertyDescriptor propDescriptor, Color value)
		{
			blitMaterial.SetColor(propDescriptor.PropName, value);
		}

		public void SetColor(string propName, Color value)
		{
			blitMaterial.SetColor(propName, value);
		}

		public void SetVector(SynthNodeDescriptor.PropertyDescriptor propDescriptor, Vector3 value)
		{
			blitMaterial.SetVector(propDescriptor.PropName, value);
		}

		public void SetVector(string propName, Vector3 value)
		{
			blitMaterial.SetVector(propName, value);
		}

		public void SetEnum(SynthNodeDescriptor.PropertyDescriptor propDescriptor, string value)
		{
			SetKeywordOnMaterial(blitMaterial, propDescriptor.PropName, value);
		}

		public void SetEnum(string propName, string value)
		{
			SetKeywordOnMaterial(blitMaterial, propName, value);
		}

		public void SetTexture(SynthNodeDescriptor.PropertyDescriptor propDescriptor, Texture2D value)
		{
			blitMaterial.SetTexture(propDescriptor.PropName, value);
		}

		public void SetTexture(string propName, Texture2D value)
		{
			blitMaterial.SetTexture(propName, value);
		}

		private void SetKeywordOnMaterial(Material material, string propName, string keyword)
		{
			foreach (LocalKeyword enabled in material.enabledKeywords)
			{
				if (enabled.name.StartsWith(propName))
					material.DisableKeyword(in enabled);
			}

			material.EnableKeyword(propName + "_" + keyword);
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
	/// This class represents a modulation process of on an input in the synthesize chain.
	/// </summary>
	public class ModulateNode : SynthNode
	{
		public ModulateNode(Shader shader) : base(shader) { }

		public void Modulate(Texture inTex, RenderTexture outTex)
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
