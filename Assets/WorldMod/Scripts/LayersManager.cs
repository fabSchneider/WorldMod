using System.Collections;
using Fab.Common;
using UnityEngine;
using UnityEngine.Rendering;

namespace Fab.WorldMod
{
	[RequireComponent(typeof(DatasetsComponent))]
    public class LayersManager : MonoBehaviour
    {
		private DatasetsComponent datasetsComp;

		[SerializeField]
		private Renderer worldRenderer;

		private MaterialPropertyBlock worldMpb;

		private Material worldBaseMaterial;

		[SerializeField]
		private Material worldLayerMaterial;

		public RenderTexture worldOverlayTexture;

		private RenderTexture worldOverlayBaseTexture;

		public Material blitMaterial;

		private int baseMapId;
		private int bumpMapId;
		private int overlayMapId;

		void Start()
		{
			datasetsComp = GetComponent<DatasetsComponent>();
			datasetsComp.Layers.layersChanged += OnLayersChanged;
			worldMpb = new MaterialPropertyBlock();
			worldBaseMaterial = worldRenderer.material;

			baseMapId = Shader.PropertyToID("_BaseMap");
			bumpMapId = Shader.PropertyToID("_BumpMap");
			overlayMapId = Shader.PropertyToID("_OverlayMap");

			worldOverlayBaseTexture = new RenderTexture(worldOverlayTexture.descriptor);
			worldOverlayBaseTexture.Create();

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
				worldRenderer.SetPropertyBlock(null);
				return;
			}


			worldMpb.Clear();
			blitMaterial.SetTexture("_BaseTex", null);

			foreach (var layer in datasetsComp.Layers)
			{
				if (layer.TryGetData("texture", out Texture tex))
				{
					if (layer.TryGetData("mode", out string mode))
					{
						switch (mode)
						{
							case "base":
								worldMpb.SetTexture(baseMapId, tex);
								break;
							case "normal":
								worldMpb.SetTexture(bumpMapId, tex);
								break;
							case "overlay":

								if (layer.TryGetData("opacity", out double opacity))
									blitMaterial.SetFloat("_Opacity", (float)opacity);
								else
									blitMaterial.SetFloat("_Opacity", 1f);
								if (layer.TryGetData("color", out Color color))
									blitMaterial.SetColor("_Tint", color);
								else
									blitMaterial.SetColor("_Tint", Color.white);

								Graphics.Blit(tex, worldOverlayTexture, blitMaterial, 0);
								Graphics.Blit(worldOverlayTexture, worldOverlayBaseTexture);
								// todo: do not use property block
								worldMpb.SetTexture(overlayMapId, worldOverlayTexture);
								blitMaterial.SetTexture("_BaseTex", worldOverlayBaseTexture);
								break;
							default:
								break;
						}
					}
				}
			}

			worldRenderer.sharedMaterial = worldLayerMaterial;
			worldRenderer.SetPropertyBlock(worldMpb);
		}
    }
}
