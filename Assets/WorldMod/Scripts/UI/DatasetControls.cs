using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Fab.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace Fab.WorldMod.UI
{
	public class DatasetControls : VisualElement
	{
		private static readonly string classname = "dataset-controls";
		private static readonly string visibleClassname = classname + "--visible";
		private static readonly string contentClassname = classname + "__content";
		private static readonly string wireContainerClassname = classname + "__wire-container";

		private DatasetStock stock;
		private DatasetElement currentItem;
		private Vector2 itemAnchor;
		private ObjectPool<Wire> wirePool;

		private VisualElement content;
		private VisualElement wireContainer;
		public override VisualElement contentContainer => content;

		//private ObjectPool<Label> controlsPool;
		public float XOffset { get; set; } = 300;

		private Texture wireTexture;

		public Color WireColor { get; set; } = new Color(0.5f, 0.5f, 0.5f);
		public DatasetControls(DatasetStock stock)
		{
			this.stock = stock;
			wireTexture = Resources.Load<Texture2D>("WorldMod/Wire");
			wirePool = new ObjectPool<Wire>(8, true, CreateWire, ResetWire);


			AddToClassList(classname);
			pickingMode = PickingMode.Ignore;

			wireContainer = new VisualElement().WithClass(wireContainerClassname);
			wireContainer.pickingMode = PickingMode.Ignore;
			hierarchy.Add(wireContainer);
			content = new VisualElement();
			content.AddToClassList(contentClassname);
			hierarchy.Add(content);

			//controlsPool = new ObjectPool<Label>(8, true, () => new Label(), label => label.text = null);

		}

		public void SetDatasetItem(DatasetElement item)
		{
			wireContainer.Query<Wire>().ForEach(wire => wirePool.ReturnToPool(wire));
			RemoveFromClassList(visibleClassname);

			if (item == null)
			{
				currentItem = null;
				RemoveFromHierarchy();
			}
			else
			{
				currentItem = item;
				content.Clear();
				//content.Query<>().ForEach(control =>
				//{
				//	control.RemoveFromHierarchy();
				//	//controlsPool.ReturnToPool(control);
				//});

				Dataset dataset = stock[item.Id];
				AddControls(dataset);

				currentItem.parent.Add(this);
				BuildWiresAsync().Forget();

			}
		}

		private static readonly string controlsKey = "controls";
		private void AddControls(Dataset dataset)
		{
			if (dataset.TryGetData(controlsKey, out IEnumerable controls))
			{
				foreach (string controlKey in controls.OfType<string>())
				{
					if(dataset.TryGetData(controlKey, out object controlVal))
					{
						if(controlVal is float number)
						{
							Slider slider = new Slider(controlKey, 0f, 1f);
							slider.value = number;
							slider.RegisterValueChangedCallback(evt =>
							{
								dataset.SetData(controlKey, evt.newValue);
								Signals.Get<DatasetUpdatedSignal>().Dispatch(dataset);
							});
							content.Add(slider);
						}
						else
						{
							Label control = new Label();
							control.text = $"{controlKey}: {controlVal}";
							content.Add(control);
						}
					}
				}
			}
		}

		private async UniTaskVoid BuildWiresAsync()
		{
			await UniTask.DelayFrame(1);
			Debug.Log(worldBound.height);

			Vector2 anchor = GetRightAnchor(currentItem.localBound);
			transform.position = new Vector2(anchor.x, anchor.y - localBound.height / 2f);

			SetWires();

			AddToClassList(visibleClassname);
		}


		private void SetWires()
		{
			itemAnchor = GetRightAnchor(currentItem.worldBound);
			itemAnchor = wireContainer.WorldToLocal(itemAnchor);

			foreach (var child in content.Children())
			{
				Vector2 targetAnchor = new Vector2(wireContainer.worldBound.max.x, child.worldBound.min.y + child.worldBound.height / 2f);

				targetAnchor = wireContainer.WorldToLocal(targetAnchor);

				Wire wire = wirePool.GetPooled();
				wire.SetEndpoints(itemAnchor, targetAnchor);
				wireContainer.Add(wire);
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
