using Fab.Common;
using UnityEngine;

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

		//public RenderTexture worldLayerTexture;

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
								worldMpb.SetTexture(overlayMapId, tex);
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
