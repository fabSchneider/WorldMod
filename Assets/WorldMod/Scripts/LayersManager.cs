using System.Collections;
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

		public Material blitMaterial;
		private Material blitMaterialInst;

		private int baseMapId;
		private int bumpMapId;
		private int overlayMapId;

		void Start()
		{
			datasetsComp.Layers.layersChanged += OnLayersChanged;
			worldBaseMaterial = worldRenderer.material;

			baseMapId = Shader.PropertyToID("_BaseMap");
			bumpMapId = Shader.PropertyToID("_BumpMap");
			overlayMapId = Shader.PropertyToID("_OverlayMap");

			worldLayerMaterialInst = new Material(worldLayerMaterial);

			worldOverlayBaseTexture = new RenderTexture(worldOverlayTexture.descriptor);
			worldOverlayBaseTexture.Create();

			blitMaterialInst = new Material(blitMaterial);

			StartCoroutine(DelayedStart());
		}

		private IEnumerator DelayedStart()
		{
			yield return null;
			UpdateLayers();
		}

		private void OnDestroy()
		{
			if(datasetsComp)
				datasetsComp.Layers.layersChanged -= OnLayersChanged;

			worldOverlayBaseTexture.Release();

		}
		private void OnLayersChanged()
		{
			Debug.Log("Updating layers");
			UpdateLayers();
		}

		public void UpdateLayers()
		{
			if(datasetsComp.Layers.Count == 0)
			{
				worldRenderer.sharedMaterial = worldBaseMaterial;
				return;
			}

			worldLayerMaterialInst.CopyPropertiesFromMaterial(worldLayerMaterial);
			blitMaterialInst.CopyPropertiesFromMaterial(blitMaterial);

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
							case "overlay":

								if (layer.TryGetData("opacity", out double opacity))
									blitMaterialInst.SetFloat("_Opacity", (float)opacity);
								if (layer.TryGetData("color", out Color color))
									blitMaterialInst.SetColor("_Tint", color);

								Graphics.Blit(tex, worldOverlayTexture, blitMaterialInst, 0);
								Graphics.Blit(worldOverlayTexture, worldOverlayBaseTexture);
								
								worldLayerMaterialInst.SetTexture(overlayMapId, worldOverlayTexture);
								blitMaterialInst.SetTexture("_BaseTex", worldOverlayBaseTexture);
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
