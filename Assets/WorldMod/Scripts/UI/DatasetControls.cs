using Cysharp.Threading.Tasks;
using Fab.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace Fab.WorldMod.UI
{
	public class DatasetControls : VisualElement
	{
		private static readonly string classname = "dataset-controls";
		private static readonly string contentClassname = classname + "__content";
		private static readonly string wireClassname = classname + "__wire";

		private DatasetStock stock;
		private DatasetElement currentItem;
		private Vector2 itemAnchor;
		private ObjectPool<Wire> wirePool;

		private VisualElement content;

		public override VisualElement contentContainer => content;

		private ObjectPool<Label> controlsPool;
		public float XOffset { get; set; } = 300;

		private Texture wireTexture;

		public Color WireColor { get; set; } = new Color(1f, 1f, 1f, 0.5f);
		public DatasetControls(DatasetStock stock)
		{
			this.stock = stock;
			wireTexture = Resources.Load<Texture2D>("WorldMod/Wire");
			wirePool = new ObjectPool<Wire>(8, true, CreateWire, ResetWire);


			AddToClassList(classname);
			content = new VisualElement();
			content.AddToClassList(contentClassname);
			hierarchy.Add(content);

			controlsPool = new ObjectPool<Label>(8, true, () => new Label(), label => label.text = null);

		}

		public void SetDatasetItem(DatasetElement item)
		{
			this.Query<Wire>().ForEach(wire => wirePool.ReturnToPool(wire));

			if (item == null)
			{
				currentItem = null;
				RemoveFromHierarchy();
			}
			else
			{
				currentItem = item;

				content.Query<Label>().ForEach(control =>
				{
					control.RemoveFromHierarchy();
					controlsPool.ReturnToPool(control);
				});

				Dataset dataset = stock[item.Id];
				foreach (string key in dataset.DataKeys)
				{
					Label control = controlsPool.GetPooled();
					control.text = key;
					content.Add(control);
				}

				style.opacity = 0f;
				currentItem.parent.Add(this);
				BuildWiresAsync().Forget();
				//RegisterCallback<GeometryChangedEvent>(OnGeometryChangedAfterAdd);

			}
		}

		private async UniTaskVoid BuildWiresAsync()
		{
			await UniTask.DelayFrame(1);
			Debug.Log(worldBound.height);

			style.opacity = StyleKeyword.Null;
			Vector2 anchor = GetRightAnchor(currentItem.localBound);
			transform.position =  new Vector2(XOffset, anchor.y  - localBound.height / 2f);

			SetWires();
		}


		private void SetWires()
		{
			itemAnchor = GetRightAnchor(currentItem.worldBound);
			itemAnchor = this.WorldToLocal(itemAnchor);

			foreach (var child in content.Children())
			{
				Vector2 targetAnchor = GetLeftAnchor(child.localBound);

				Wire wire = wirePool.GetPooled();
				wire.SetEndpoints(itemAnchor, targetAnchor);
				hierarchy.Add(wire);
			}

		}

		private Vector2 GetRightAnchor(Rect rect)
		{
			return new Vector2(rect.max.x, rect.min.y + rect.height / 2f);
		}
		private Vector2 GetLeftAnchor(Rect rect)
		{
			return new Vector2(rect.min.x, rect.min.y + rect.height / 2f);
		}


		private Wire CreateWire()
		{
			Wire wire = new Wire();
			wire.AddToClassList(wireClassname);
			wire.Texture = wireTexture;
			wire.Tint = WireColor;
			wire.UseAdaptiveResolution = true;
			return wire;
		}
		private void ResetWire(Wire wire)
		{
			wire.RemoveFromHierarchy();
		}
	}
}
