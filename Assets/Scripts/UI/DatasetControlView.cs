using System;
using System.Collections.Generic;
using Fab.Common;
using Fab.Localization;
using UnityEngine;
using UnityEngine.UIElements;

namespace Fab.WorldMod.UI
{
	public abstract class ControlBinding
	{
		protected Dataset dataset;

		public abstract VisualElement VisualElement { get;  }

		public abstract void Bind();
		public abstract void Unbind();
	}

	public class ControlBinding<T> : ControlBinding
	{
		protected BaseField<T> field;
		protected ValueControl<T> control;

		public override VisualElement VisualElement => field;

		public ControlBinding(Dataset dataset, ValueControl<T> control, BaseField<T> field)
		{
			this.dataset = dataset;
			this.control = control;
			this.field = field;
		}

		public override void Bind()
		{
			field.RegisterValueChangedCallback(SetValueOnControl);
			control.RegisterChangeCallback(SetValueOnField);
			control.RegisterEnableCallback(SetEnableOnField);
		}

		public override void Unbind()
		{
			field.UnregisterValueChangedCallback(SetValueOnControl);
			control.UnregisterChangeCallback(SetValueOnField);
			control.UnregisterEnableCallback(SetEnableOnField);
		}

		private void SetValueOnField(T value)
		{
				field.SetValueWithoutNotify(value);
		}

		private void SetValueOnControl(ChangeEvent<T> evt)
		{
			if (control.SetValue(evt.newValue))
				Signals.Get<DatasetUpdatedSignal>().Dispatch(dataset);
		}

		private void SetEnableOnField(bool state)
		{
			field.SetEnabled(state);
		}
	}


	public class DatasetControlView : VisualElement
	{
		private static readonly string classname = "dataset-controls";
		private static readonly string controlsKey = "controls";

		private Dataset dataset;
		private List<ControlBinding> bindings;

		public DatasetControlView(Dataset dataset)
		{
			if (dataset == null)
				throw new ArgumentNullException(nameof(dataset));

			this.dataset = dataset;

			AddToClassList(classname);
			pickingMode = PickingMode.Ignore;
		}

		public void RebuildView()
		{
			if(bindings == null)
			{
				bindings = new List<ControlBinding>();
			}
			else
			{
				foreach(ControlBinding binding in bindings)
				{
					binding.Unbind();
					binding.VisualElement.RemoveFromHierarchy();
				}

				bindings.Clear();
			}

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
							bindings.Add(AddSliderControl(dataset, sc));
							break;
						case IntervalControl rc:
							bindings.Add(AddRangeControl(dataset, rc));
							break;
						case ChoiceControl cc:
							bindings.Add(AddChoiceControl(dataset, cc));
							break;
						case ValueControl<bool> bc:
							bindings.Add(AddFieldControl<bool>(dataset, bc, new Toggle(bc.Name)));
							break;
						case ValueControl<string> tc:
							bindings.Add(AddLabel(dataset, tc));
							break;
						case null:
							Debug.LogWarning("Control is null and will be skipped");
							break;
						default:
							Debug.LogWarning("Control is undetermined and will be skipped");
							break;
					}
				}
			}

			foreach (ControlBinding binding in bindings)
			{
				binding.Bind();
			}

		}

		private ControlBinding AddFieldControl<T>(Dataset dataset, ValueControl<T> valueControl, BaseField<T> field)
		{
			field.value = valueControl.Value;

			ControlBinding binding = new ControlBinding<T>(dataset, valueControl, field);
			Add(field);
			field.Q<TextElement>().WithLocalizable();
			field.SetEnabled(valueControl.Enabled);

			return binding;
		}

		private ControlBinding AddRangeControl(Dataset dataset, IntervalControl intervalControl)
		{
			MinMaxSlider slider = new MinMaxSlider(intervalControl.Name, intervalControl.Value.x, intervalControl.Value.y, intervalControl.Min, intervalControl.Max);
			slider.value = intervalControl.Value;
			ControlBinding binding = new ControlBinding<Vector2>(dataset, intervalControl, slider);
			Add(slider);
			slider.Q<TextElement>().WithLocalizable();
			slider.SetEnabled(intervalControl.Enabled);
			return binding;
		}

		private ControlBinding AddSliderControl(Dataset dataset, RangeControl rangeControl)
		{
			Slider slider = new Slider(rangeControl.Name, rangeControl.Min, rangeControl.Max);
			slider.value = rangeControl.Value;
			ControlBinding binding = new ControlBinding<float>(dataset, rangeControl, slider);
			Add(slider);
			slider.Q<TextElement>().WithLocalizable();
			slider.SetEnabled(rangeControl.Enabled);
			return binding;
		}

		private ControlBinding AddChoiceControl(Dataset dataset, ChoiceControl choiceControl)
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
			ControlBinding binding = new ControlBinding<string>(dataset, choiceControl, choiceField);
			Add(choiceField);
			choiceField.Query<TextElement>().ForEach(elem => elem.WithLocalizable());
			choiceField.SetEnabled(choiceControl.Enabled);
			return binding;
		}

		private ControlBinding AddLabel(Dataset dataset, ValueControl<string> control)
		{
			TextField textField = new TextField(control.Name);

			textField.Query<TextElement>().ForEach(elem => elem.WithLocalizable());
			textField.value = control.DefaultValue;
			textField.isReadOnly = true;
			textField.SetEnabled(control.Enabled);
			ControlBinding binding = new ControlBinding<string>(dataset, control, textField);
			Add(textField);

			return binding;
		}
	}
}
