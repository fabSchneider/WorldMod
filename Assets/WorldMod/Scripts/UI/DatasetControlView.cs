using System;
using System.Collections.Generic;
using Fab.Common;
using Fab.WorldMod.Localization;
using UnityEngine;
using UnityEngine.UIElements;

namespace Fab.WorldMod.UI
{
	public class DatasetControlView : VisualElement
	{
		private static readonly string classname = "dataset-controls";

		public DatasetControlView(Dataset dataset)
		{
			if (dataset == null)
				throw new ArgumentNullException(nameof(dataset));


			AddToClassList(classname);
			pickingMode = PickingMode.Ignore;

			AddControls(dataset);
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
					if (item is object[] arr)
					{
						control = (ValueControl)arr[0];
						style = (string)arr[1];
						// TODO: Handle styles
					}
					else
					{
						control = (ValueControl)item;
					}

					switch (control)
					{
						case RangeControl sc:
							AddSliderControl(dataset, sc);
							break;
						case IntervalControl rc:
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
				if (valueControl.SetValue(evt.newValue))
					Signals.Get<DatasetUpdatedSignal>().Dispatch(dataset);
			});
			Add(field);
			field.Q<TextElement>().WithLocalizable();

			valueControl.RegisterChangeCallback(val => field.SetValueWithoutNotify(val));
		}

		private void AddRangeControl(Dataset dataset, IntervalControl rangeControl)
		{
			MinMaxSlider slider = new MinMaxSlider(rangeControl.Name, rangeControl.Value.x, rangeControl.Value.y, rangeControl.Min, rangeControl.Max);
			slider.value = rangeControl.Value;
			slider.RegisterValueChangedCallback(evt =>
			{
				if (rangeControl.SetValue(evt.newValue))
					Signals.Get<DatasetUpdatedSignal>().Dispatch(dataset);
			});
			Add(slider);
			slider.Q<TextElement>().WithLocalizable();

			rangeControl.RegisterChangeCallback(val => slider.SetValueWithoutNotify(val));
		}

		private void AddSliderControl(Dataset dataset, RangeControl sliderControl)
		{
			Slider slider = new Slider(sliderControl.Name, sliderControl.Min, sliderControl.Max);
			slider.value = sliderControl.Value;
			slider.RegisterValueChangedCallback(evt =>
			{
				if (sliderControl.SetValue(evt.newValue))
					Signals.Get<DatasetUpdatedSignal>().Dispatch(dataset);
			});
			Add(slider);
			slider.Q<TextElement>().WithLocalizable();

			sliderControl.RegisterChangeCallback(val => slider.SetValueWithoutNotify(val));
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
				if (choiceControl.SetValue(evt.newValue))
					Signals.Get<DatasetUpdatedSignal>().Dispatch(dataset);
			});
			Add(choiceField);
			choiceField.Query<TextElement>().ForEach(elem => elem.WithLocalizable());

			choiceControl.RegisterChangeCallback(val => choiceField.SetValueWithoutNotify(val));
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
			Add(container);
		}
	}
}
