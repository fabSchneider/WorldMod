using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Fab.Common;
using Fab.WorldMod.Localization;
using UnityEngine;
using UnityEngine.UIElements;

namespace Fab.WorldMod.UI
{
	public class DatasetControlView : VisualElement
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

		private Texture wireTexture;

		public Color WireColor { get; set; } = new Color(0.5f, 0.5f, 0.5f);
		public DatasetControlView(DatasetStock stock)
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

			Signals.Get<OnChangeLocaleSignal>().AddListener(HideOnLocaleChange);

		}

		private void HideOnLocaleChange(Locale locale)
		{
			SetDatasetItem(null);
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
				Dataset dataset = stock[item.Id];
				AddControls(dataset);

				currentItem.parent.Add(this);
				BuildWiresAsync().Forget();

			}
		}

		private static readonly string controlsKey = "controls";
		private void AddControls(Dataset dataset)
		{
			if (dataset.TryGetData(controlsKey, out IEnumerable<object> controls))
			{
				foreach (object item in controls)
				{
					ValueControl control = null;
					string style = null;
					if(item is object[] arr)
					{
						control = (ValueControl)arr[0];
						style = (string)arr[1];
						Debug.Log(style);
					}
					else
					{
						control = (ValueControl)item;
					}

					switch (control)
					{
						case SliderControl sc:
							AddSliderControl(dataset, sc);
							break;
						case RangeControl rc:
							AddRangeControl(dataset, rc);
							break;
						case ChoiceControl cc:
							AddChoiceControl(dataset, cc);
							break;
						case ValueControl<bool> bc:
							AddFieldControl<bool>(dataset, bc, new Toggle(bc.Name));
							break;
						case null:
							Debug.LogWarning("Control is null and will be skipped");
							break;
						default:
							AddLabel(dataset, control);
							break;
					}
				}
			}
		}

		private void AddFieldControl<T>(Dataset dataset, ValueControl<T> valueControl, BaseField<T> field)
		{
			field.value = valueControl.Value;
			field.RegisterValueChangedCallback(evt =>
			{
				if(valueControl.SetValue(evt.newValue))
					Signals.Get<DatasetUpdatedSignal>().Dispatch(dataset);
			});
			content.Add(field);
			field.Q<TextElement>().WithLocalizable();
		}

		private void AddRangeControl(Dataset dataset, RangeControl rangeControl)
		{
			MinMaxSlider slider = new MinMaxSlider(rangeControl.Name, rangeControl.Value.x, rangeControl.Value.y, rangeControl.Min, rangeControl.Max);
			slider.value = rangeControl.Value;
			slider.RegisterValueChangedCallback(evt =>
			{
				if (rangeControl.SetValue(evt.newValue))
					Signals.Get<DatasetUpdatedSignal>().Dispatch(dataset);
			});
			content.Add(slider);
			slider.Q<TextElement>().WithLocalizable();
		}

		private void AddSliderControl(Dataset dataset, SliderControl sliderControl)
		{
			Slider slider = new Slider(sliderControl.Name, sliderControl.Min, sliderControl.Max);
			slider.value = sliderControl.Value;
			slider.RegisterValueChangedCallback(evt =>
			{
				if(sliderControl.SetValue(evt.newValue))
					Signals.Get<DatasetUpdatedSignal>().Dispatch(dataset);
			});
			content.Add(slider);
			slider.Q<TextElement>().WithLocalizable();
		}

		private void AddChoiceControl(Dataset dataset, ChoiceControl choiceControl)
		{
			Func<string, string> formatItems = null;
			if (choiceControl.ChoiceDisplayNames != null)
			{
				formatItems = (choice) =>
				{
					string name = choiceControl.ChoiceDisplayNames[choiceControl.Choices.IndexOf(choice)];

					if (LocalizationComponent.Localization.TryGetLocalizedString(name, out string localString))
						return localString;
					return name;
				};
			}

			DropdownField choiceField = new DropdownField(choiceControl.Name, choiceControl.Choices, choiceControl.DefaultValue, formatItems, formatItems);
			choiceField.value = choiceControl.CurrentChoice;
			choiceField.RegisterValueChangedCallback(evt =>
			{
				if(choiceControl.SetValue(evt.newValue))
					Signals.Get<DatasetUpdatedSignal>().Dispatch(dataset);
			});
			content.Add(choiceField);
			choiceField.Query<TextElement>().ForEach(elem => elem.WithLocalizable());
		}

		private void AddLabel(Dataset dataset, ValueControl control)
		{
			VisualElement container = new VisualElement();
			container.style.flexDirection = FlexDirection.Row;
			Label controlLabel = new Label(control.Name).WithLocalizable();

			Label controlValueLabel = new Label(control.GetDefaultValue().ToString());
			if (control is ValueControl<string> stringControl)
				controlValueLabel.WithLocalizable();
			container.Add(controlLabel);
			container.Add(controlValueLabel);
			content.Add(container);
		}

		private async UniTaskVoid BuildWiresAsync()
		{
			await UniTask.DelayFrame(1);

			if (currentItem == null)
				return;

			Vector2 anchor = GetRightAnchor(currentItem.localBound);
			float yPos = Mathf.Max(0f, anchor.y - localBound.height / 2f);
			transform.position = new Vector2(anchor.x, yPos);

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
