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
		private DatasetItem currentItem;
		private Vector2 itemAnchor;
		private ObjectPool<Wire> wirePool;

		private VisualElement content;

		public override VisualElement contentContainer => content;

		public DatasetControls(DatasetStock stock)
		{
			this.stock = stock;
			wirePool = new ObjectPool<Wire>(8, true, CreateWire, ResetWire);
			AddToClassList(classname);
			content = new VisualElement();
			content.AddToClassList(contentClassname);
			hierarchy.Add(content);

			Add(new Label("Label 1"));
			Add(new Label("Label 2"));
			Add(new Label("Label 3"));
			Add(new Label("Label 4"));
		}

		public void SetDatasetItem(DatasetItem item)
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
				transform.position = item.localBound.max + new Vector2(100, 0);
				RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);

				item.parent.Add(this);
			}
		}

		private void OnAttachToPanel(AttachToPanelEvent evt)
		{
			UnregisterCallback<AttachToPanelEvent>(OnAttachToPanel);
			SetWires();
		}

		private void SetWires()
		{
			itemAnchor = GetRightAnchor(currentItem.worldBound);
			itemAnchor = this.WorldToLocal(itemAnchor);

			foreach (var child in contentContainer.Children())
			{
				Vector2 targetAnchor = GetLefAnchor(child.localBound);

				Wire wire = wirePool.GetPooled();
				wire.SetEndpoints(itemAnchor, targetAnchor);
				hierarchy.Add(wire);
			}

		}

		private Vector2 GetRightAnchor(Rect rect)
		{
			return new Vector2(rect.max.x, rect.min.y + rect.height / 2f);
		}
		private Vector2 GetLefAnchor(Rect rect)
		{
			return new Vector2(rect.min.x, rect.min.y + rect.height / 2f);
		}


		private Wire CreateWire()
		{
			Wire wire = new Wire();
			wire.AddToClassList(wireClassname);
			wire.UseAdaptiveResolution = true;
			return wire;
		}
		private void ResetWire(Wire wire)
		{
			wire.RemoveFromHierarchy();
		}
	}
}
