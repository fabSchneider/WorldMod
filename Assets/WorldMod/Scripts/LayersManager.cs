using System;
using System.Collections;
using System.Collections.Generic;
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
		private RenderTexture worldLayerProcessorTexA;
		private RenderTexture worldLayerProcessorTexB;

		[SerializeField]
		private Material layerBlendMaterial;
		private Material layerBlendMaterialInst;

		[Serializable]
		public class LayerProcessor
		{
			[Serializable]
			public class Property
			{
				public enum PropertyType
				{
					Float,
					Color,
					Enum
				}

				[SerializeField]
				private string name;
				[SerializeField]
				PropertyType type;
				[SerializeField]
				private string shaderName;

				public string Name { get => name; }
				public PropertyType Type { get => type; }
				public string ShaderName { get => shaderName; }

				public Property(string name, string shaderName)
				{
					this.name = name;
					this.shaderName = shaderName;
				}
			}

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
		private List<LayerProcessor> layerProcessors;

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
			worldLayerProcessorTexA = new RenderTexture(worldOverlayTexture.descriptor);
			worldLayerProcessorTexA.name = "WorldLayerProcessorTex A";
			worldLayerProcessorTexB = new RenderTexture(worldOverlayTexture.descriptor);
			worldLayerProcessorTexB.name = "WorldLayerProcessorTex B";

			layerBlendMaterialInst = new Material(layerBlendMaterial);

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

			for (int i = 0; i < datasetsComp.Layers.Count; i++)
			{
				var layer = datasetsComp.Layers[i];

				layerBlendMaterialInst.CopyPropertiesFromMaterial(layerBlendMaterial);

				if (i > 0)
				{
					worldLayerMaterialInst.SetTexture(overlayMapId, worldOverlayTexture);
					layerBlendMaterialInst.SetTexture("_BaseTex", worldLayerBaseTex);
				}

				if (layer.TryGetData("texture", out Texture tex))
				{
					if (layer.TryGetData("mode", out string mode))
					{
						switch (mode)
						{
							case "base":
								worldLayerMaterialInst.SetTexture(baseMapId, tex);
								break;
							case "normal":
								worldLayerMaterialInst.SetTexture(bumpMapId, tex);
								break;
							case "add":
								if (layer.TryGetData("opacity", out float opacity))
									layerBlendMaterialInst.SetFloat("_Opacity", opacity);
								if (layer.TryGetData("color", out Color color))
									layerBlendMaterialInst.SetColor("_Tint", color);
								if (layer.TryGetData("mask", out string mask))
								{
									mask = mask.ToUpper();
									if (mask == "R" || mask == "G" || mask == "B")
									{
										layerBlendMaterialInst.DisableKeyword("_MASK_RGBA");
										layerBlendMaterialInst.EnableKeyword("_MASK_" + mask.ToUpper());
									}
								}

								//check for blit processors
								Texture currTex = tex;
								RenderTexture processTex = worldLayerProcessorTexA;
								foreach (var processor in layerProcessors)
								{

									processor.ResetDefaults();
									bool process = false;
									foreach (var prop in processor.Properties)
									{
										switch (prop.Type)
										{
											case LayerProcessor.Property.PropertyType.Float:
												if (layer.TryGetData(prop.Name, out float val))
												{
													process = true;
													processor.ProcessMaterial.SetFloat(prop.ShaderName, val);
												}
												break;
											case LayerProcessor.Property.PropertyType.Color:
												if (layer.TryGetData(prop.Name, out Color cVal))
												{
													process = true;
													processor.ProcessMaterial.SetColor(prop.ShaderName, cVal);
												}
												break;
											case LayerProcessor.Property.PropertyType.Enum:
												break;
											default:
												break;
										}
									}

									if (process)
									{
										Graphics.Blit(currTex, processTex, processor.ProcessMaterial, 0);
										currTex = processTex;
										processTex = processTex == worldLayerProcessorTexA ? worldLayerProcessorTexB : worldLayerProcessorTexA;
									}
								}

								Graphics.Blit(currTex, worldOverlayTexture, layerBlendMaterialInst, 0);

								Graphics.Blit(worldOverlayTexture, worldLayerBaseTex);
								break;
							default:
								break;
						}
					}
				}
			}

			worldRenderer.sharedMaterial = worldLayerMaterialInst;
		}
	}
}
