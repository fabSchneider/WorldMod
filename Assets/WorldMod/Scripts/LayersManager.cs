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

		private RenderTexture worldOverlayBaseTexture;
		private RenderTexture worldOverlayProcessorTexture;

		[SerializeField]
		private Material layerBlendMaterial;
		private Material layerBlendMaterialInst;

		[Serializable]
		public class LayerProcessor
		{
			[Serializable]
			public class Property
			{
				[SerializeField]
				private string name;
				[SerializeField]
				private string shaderName;

				public string Name { get => name; }
				public string ShaderName { get => shaderName; }

				public Property(string name, string shaderName)
				{
					this.name = name;
					this.shaderName = shaderName;
				}
			}

			[SerializeField]
			private Material processMaterial;

			private Material processMaterialInst;
			[SerializeField]
			private Property[] properties;

			public Material ProcessMaterial
			{
				get
				{ 
					if(processMaterialInst == null)
						processMaterialInst = new Material(processMaterial);
					return processMaterialInst;
				}
			}

			public IEnumerable<Property> Properties => properties;

			public LayerProcessor(Material processMaterial, Property[] processParams)
			{
				this.processMaterial = processMaterial;
				properties = processParams;
			}

			public void ResetDefaults()
			{
				if(processMaterialInst == null)
				{
					processMaterialInst = new Material(processMaterial);
				}
				else
				{
					processMaterialInst.CopyPropertiesFromMaterial(processMaterial);
				}
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

			worldOverlayBaseTexture = new RenderTexture(worldOverlayTexture.descriptor);
			worldOverlayProcessorTexture = new RenderTexture(worldOverlayTexture.descriptor);
			worldOverlayBaseTexture.Create();

			layerBlendMaterialInst = new Material(layerBlendMaterial);

			Signals.Get<DatasetUpdatedSignal>().AddListener(OnDatasetUpdated);

			layerUpdateFlag = true;

		}
		private void OnDestroy()
		{
			if(datasetsComp)
				datasetsComp.Layers.layersChanged -= OnLayersChanged;

			Signals.Get<DatasetUpdatedSignal>().RemoveListener(OnDatasetUpdated);

			worldOverlayBaseTexture.Release();

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

		public void UpdateLayers()
		{
			if(datasetsComp.Layers.Count == 0)
			{
				worldRenderer.sharedMaterial = worldBaseMaterial;
				return;
			}


			worldLayerMaterialInst.CopyPropertiesFromMaterial(worldLayerMaterial);
			layerBlendMaterialInst.CopyPropertiesFromMaterial(layerBlendMaterial);

			foreach (var processor in layerProcessors)
				processor.ResetDefaults();

			foreach (var layer in datasetsComp.Layers)
			{
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


								bool processed = false;
								//check for blit processors
								foreach (var processor in layerProcessors)
								{
									processor.ResetDefaults();
									bool process = false;
									foreach (var prop in processor.Properties)
									{
										if (layer.TryGetData(prop.Name, out float val))
										{
											process = true;
											processor.ProcessMaterial.SetFloat(prop.ShaderName, val);
										}
									}

									if (process)
									{
										Graphics.Blit(tex, worldOverlayProcessorTexture, processor.ProcessMaterial, 0);
										processed = true;
									}
								}

								if(processed)
										Graphics.Blit(worldOverlayProcessorTexture, worldOverlayTexture, layerBlendMaterialInst, 0);
								else
									Graphics.Blit(tex, worldOverlayTexture, layerBlendMaterialInst, 0);

								Graphics.Blit(worldOverlayTexture, worldOverlayBaseTexture);
								
								worldLayerMaterialInst.SetTexture(overlayMapId, worldOverlayTexture);
								layerBlendMaterialInst.SetTexture("_BaseTex", worldOverlayBaseTexture);
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
