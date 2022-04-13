using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fab.Common;
using UnityEngine;
using UnityEngine.Rendering;

namespace Fab.WorldMod
{
	public class LayersManager : MonoBehaviour
	{
		private static readonly string layerProfilerSample = "World Layering";

		[SerializeField]
		private DatasetsComponent datasetsComp;

		[SerializeField]
		private Renderer worldRenderer;

		private Material worldBaseMaterial;

		[SerializeField]
		private Material worldLayerMaterial;
		private Material worldLayerMaterialInst;

		public RenderTexture worldOverlayTexture;

		private RenderTexture worldLayerBaseTex;
		private RenderTexture worldLayerTexA;
		private RenderTexture worldLayerTexB;

		[Serializable]
		public class LayerProcessor
		{
			[Serializable]
			public class Property : ISerializationCallbackReceiver
			{
				public enum PropertyType
				{
					Float,
					Color,
					Vector,
					Enum
				}

				[SerializeField]
				private string name;
				[SerializeField]
				PropertyType type;
				[SerializeField]
				private string shaderName;
				[SerializeField]
				private string enumChoices;

				private List<string> keywords;
				public IReadOnlyList<string> Keywords => keywords;

				public string Name { get => name; }
				public PropertyType Type { get => type; }
				public string ShaderName { get => shaderName; }

				public Property(string name, string shaderName)
				{
					this.name = name;
					this.shaderName = shaderName;
				}


				public string GetKeyword(string choice)
				{
					for (int i = 0; i < keywords.Count; i++)
					{
						string keyword = keywords[i];
						int index = keyword.LastIndexOf('_') + 1;
						if (keyword.Substring(index, keyword.Length - index) == choice)
							return keyword;
					}

					return null;
				}

				public void OnBeforeSerialize() { }

				public void OnAfterDeserialize()
				{
					string[] choices = enumChoices.Split(',');
					keywords = new List<string>(choices.Length);
					for (int i = 0; i < choices.Length; i++)
					{
						string choice = choices[i].Trim();
						if (!string.IsNullOrEmpty(choice))
							keywords.Add(ShaderName + '_' + choice);
					}
				}
			}

			[SerializeField]
			private string name;
			public string Name => name;

			private Material processMaterialDefaults;

			[SerializeField]
			private Shader shader;

			public Shader Shader => shader;

			private Material processMaterialInst;
			[SerializeField]
			private Property[] properties;

			public Material ProcessMaterial
			{
				get
				{
					if (processMaterialInst == null)
						processMaterialInst = new Material(shader);
					return processMaterialInst;
				}
			}

			public IEnumerable<Property> Properties => properties;

			public LayerProcessor(Shader shader, Property[] processParams)
			{
				processMaterialDefaults = new Material(shader);
				processMaterialInst = new Material(shader);
				this.shader = shader;
				properties = processParams;
			}

			public void ResetDefaults()
			{
				if (processMaterialDefaults == null)
					processMaterialDefaults = new Material(shader);

				if (processMaterialInst == null)
					processMaterialInst = new Material(shader);
				else
					processMaterialInst.CopyPropertiesFromMaterial(processMaterialDefaults);

			}
		}

		[SerializeField]
		private List<LayerProcessor> layerBlendProcessors;

		[SerializeField]
		private List<LayerProcessor> layerProcessors;

		[SerializeField]
		private List<LayerProcessor> layerGenerators;

		private int baseMapId;
		private int bumpMapId;
		private int overlayMapId;

		private bool layerUpdateFlag = false;

		void Start()
		{
			datasetsComp.Layers.layersChanged += OnLayersChanged;
			worldBaseMaterial = worldRenderer.material;

			baseMapId = Shader.PropertyToID("_BaseMap");
			bumpMapId = Shader.PropertyToID("_BumpMap");
			overlayMapId = Shader.PropertyToID("_OverlayMap");

			worldLayerMaterialInst = new Material(worldLayerMaterial);

			worldLayerBaseTex = new RenderTexture(worldOverlayTexture.descriptor);
			worldLayerBaseTex.name = "WorldLayerBaseTex";
			worldLayerTexA = new RenderTexture(worldOverlayTexture.descriptor);
			worldLayerTexA.name = "WorldLayerProcessorTex A";
			worldLayerTexB = new RenderTexture(worldOverlayTexture.descriptor);
			worldLayerTexB.name = "WorldLayerProcessorTex B";

			Signals.Get<DatasetUpdatedSignal>().AddListener(OnDatasetUpdated);

			layerUpdateFlag = true;
		}

		private void OnDestroy()
		{
			if (datasetsComp)
				datasetsComp.Layers.layersChanged -= OnLayersChanged;

			Signals.Get<DatasetUpdatedSignal>().RemoveListener(OnDatasetUpdated);

			worldLayerBaseTex.Release();

		}

		private void Update()
		{
			if (layerUpdateFlag)
			{
				UpdateLayers();
				layerUpdateFlag = false;
			}

		}

		private void OnDatasetUpdated(Dataset dataset)
		{
			if (datasetsComp.Layers.IsLayer(dataset))
			{
				layerUpdateFlag = true;
			}
		}

		private void OnLayersChanged()
		{
			layerUpdateFlag = true;
		}

		public void ClearOutRenderTexture(RenderTexture renderTexture)
		{
			RenderTexture rt = RenderTexture.active;
			RenderTexture.active = renderTexture;
			GL.Clear(true, true, Color.clear);
			RenderTexture.active = rt;
		}

		public void UpdateLayers()
		{
			if (datasetsComp.Layers.Count == 0)
			{
				worldRenderer.sharedMaterial = worldBaseMaterial;
				return;
			}


			worldLayerMaterialInst.CopyPropertiesFromMaterial(worldLayerMaterial);

			ClearOutRenderTexture(worldOverlayTexture);
			ClearOutRenderTexture(worldLayerBaseTex);


			foreach (var processor in layerProcessors)
				processor.ResetDefaults();
			foreach (var processor in layerBlendProcessors)
				processor.ResetDefaults();

			for (int i = 0; i < datasetsComp.Layers.Count; i++)
			{
				var layer = datasetsComp.Layers[i];

				if (i > 0)
				{
					worldLayerMaterialInst.SetTexture(overlayMapId, worldOverlayTexture);
				}

				if (layer.TryGetData("mode", out string mode))
				{
					switch (mode)
					{
						case "base":
							SetBaseLayer(layer);
							break;
						case "normal":
							SetNormalLayer(layer);
							break;
						case "blend":
							SetBlendLayer(layer);
							break;
						default:
							break;
					}
				}
			}

			worldRenderer.sharedMaterial = worldLayerMaterialInst;
		}

		private void SetBaseLayer(Dataset layer)
		{
			if (!layer.TryGetData("texture", out Texture tex))
				return;

			worldLayerMaterialInst.SetTexture(baseMapId, tex);
		}

		private void SetNormalLayer(Dataset layer)
		{
			if (!layer.TryGetData("texture", out Texture tex))
				return;

			worldLayerMaterialInst.SetTexture(bumpMapId, tex);
		}

		private void SetBlendLayer(Dataset layer)
		{
			if (layerBlendProcessors.Count == 0)
				return;

			Texture tex = GetLayerTexture(layer);

			LayerProcessor blendProcesser = layerBlendProcessors[0];

			// check blendmode
			if (layer.TryGetData("blendmode", out string blendMode))
			{
				var processor = layerBlendProcessors.FirstOrDefault(p => p.Name.Equals(blendMode, StringComparison.InvariantCultureIgnoreCase));
				if (processor != null)
					blendProcesser = processor;
			}

			blendProcesser.ProcessMaterial.SetTexture("_BaseTex", worldLayerBaseTex);

			if (layer.TryGetData("opacity", out float opacity))
				blendProcesser.ProcessMaterial.SetFloat("_Opacity", opacity);


			Graphics.Blit(tex, worldOverlayTexture, blendProcesser.ProcessMaterial, 0);
			Graphics.Blit(worldOverlayTexture, worldLayerBaseTex);
		}

		private bool SetProcessorPropertiesFromLayer(Dataset layer, LayerProcessor processor)
		{
			bool process = false;
			Material processMaterial = processor.ProcessMaterial;
			foreach (var prop in processor.Properties)
			{
				switch (prop.Type)
				{
					case LayerProcessor.Property.PropertyType.Float:
						if (layer.TryGetData(prop.Name, out float val))
						{
							process = true;
							processMaterial.SetFloat(prop.ShaderName, val);
						}
						break;
					case LayerProcessor.Property.PropertyType.Color:
						if (layer.TryGetData(prop.Name, out Color cVal))
						{
							process = true;
							processMaterial.SetColor(prop.ShaderName, cVal);
						}
						break;
					case LayerProcessor.Property.PropertyType.Vector:
						if (layer.TryGetData(prop.Name, out Vector3 vVal))
						{
							process = true;
							processMaterial.SetVector(prop.ShaderName, vVal);
						}
						break;
					case LayerProcessor.Property.PropertyType.Enum:
						if (layer.TryGetData(prop.Name, out string eVal))
						{
							process = true;
							processMaterial.DisableKeyword(prop.Keywords[0]);
							string keyword = prop.GetKeyword(eVal);
							if (!string.IsNullOrEmpty(keyword))
								processMaterial.EnableKeyword(keyword);
						}
						break;
					default:
						break;
				}
			}
			return process;
		}

		private Texture GetLayerTexture(Dataset layer)
		{
			if (!layer.TryGetData("texture", out Texture tex))
				tex = GenerateTex(layer);

			if (tex == null)
				return null;

			return ProcessTexture(layer, tex);
		}

		private Texture ProcessTexture(Dataset layer, Texture tex)
		{
			Texture currTex = tex;
			RenderTexture processTex = worldLayerTexA;
			foreach (var processor in layerProcessors)
			{
				processor.ResetDefaults();

				if (SetProcessorPropertiesFromLayer(layer, processor))
				{
					Graphics.Blit(currTex, processTex, processor.ProcessMaterial, 0);
					currTex = processTex;
					processTex = processTex == worldLayerTexA ? worldLayerTexB : worldLayerTexA;
				}
			}

			return currTex;
		}

		private Texture GenerateTex(Dataset layer)
		{
			bool generated = false;
			foreach (var processor in layerGenerators)
			{
				processor.ResetDefaults();
				if(SetProcessorPropertiesFromLayer(layer, processor))
				{
					generated = true;
					Graphics.Blit(null, worldLayerTexB, processor.ProcessMaterial, 0);
				}
			}

			if(generated)
				return worldLayerTexB;
			else
				return null;
		}
	}
}
