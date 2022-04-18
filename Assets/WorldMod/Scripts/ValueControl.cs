using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fab.WorldMod
{
	public abstract class ValueControl
	{
		protected string name;
		protected Type type;

		public string Name => name;

		public Type Type => type;

		public abstract object GetValue();
		public abstract object GetDefaultValue();

		public void RegisterChangeCallback<T>(Action<T> onChange)
		{
			if (this is ValueControl<T> controlT)
				controlT.RegisterChangeCallback(onChange);
			else
				throw new Exception($"Cannot register callback of type \"{typeof(T).Name}\" on this control");
		}

		public void UnregisterChangeCallback<T>(Action<T> onChange)
		{
			if (this is ValueControl<T> controlT)
				controlT.UnregisterChangeCallback(onChange);
			else
				throw new Exception($"Cannot register callback of type \"{typeof(T).Name}\" on this control");
		}
	}

	public class ValueControl<T> : ValueControl
	{
		protected T defaultValue;
		public T DefaultValue => defaultValue;

		protected T value;
		public T Value => value;

		public ValueControl(string name, T defaultValue)
		{
			this.name = name;
			this.defaultValue = defaultValue;
			this.value = defaultValue;
			type = typeof(T);
		}

		public virtual bool SetValue(T value)
		{
			this.value = value;
			InvokeChange(value);
			return true;
		}

		public override object GetValue()
		{
			return value;
		}

		public override object GetDefaultValue()
		{
			return defaultValue;
		}

		protected event Action<T> changeHandler;

		public void RegisterChangeCallback(Action<T> onChange)
		{
			changeHandler -= onChange;
			changeHandler += onChange;
		}

		public void UnregisterChangeCallback(Action<T> onChange)
		{
			changeHandler -= onChange;
		}

		protected void InvokeChange(T value)
		{
			changeHandler?.Invoke(value);
		}
	}

	public class RangeControl : ValueControl<float>
	{
		private float min;
		private float max;

		public float Min => min;
		public float Max => max;

		public RangeControl(string name, float defaultValue, float min, float max) 
			: base(name, Mathf.Clamp(defaultValue, min, max))
		{
			this.min = Mathf.Min(min, max);
			this.max = Mathf.Max(min, max);
		}

		public override bool SetValue(float value)
		{
			base.SetValue(Mathf.Clamp(value, min, max));
			return true;
		}
	}

	public class IntervalControl : ValueControl<Vector3>
	{
		private float min;
		private float max;

		public float Min => min;
		public float Max => max;

		public IntervalControl(string name, float default_lower, float default_upper, float min, float max)
			: base(name, new Vector2(Mathf.Min(default_lower, default_upper), Mathf.Max(default_lower, default_upper)))
		{
			this.min = Mathf.Min(min, max);
			this.max = Mathf.Max(min, max);
		}

		public override bool SetValue(Vector3 value)
		{
			base.SetValue(new Vector3(
				Mathf.Max(Min, Mathf.Min(value.x, value.y)),
				Mathf.Min(Max, Mathf.Max(value.x, value.y)), 0f));
			return true;
		}
	}

	public class ChoiceControl : ValueControl<string>
	{
		private List<string> choices;
		private List<string> choiceDisplayNames;
		public List<string> Choices => choices;
		public string CurrentChoice => value;
		public List<string> ChoiceDisplayNames => choiceDisplayNames;

		public ChoiceControl(string name, string defaultValue, string[] choices, string[] choiceDisplayNames) : base(name, defaultValue)
		{
			if (choices == null)
				throw new ArgumentNullException(nameof(choices));

			if (choiceDisplayNames != null && choices.Length != choiceDisplayNames.Length)
				throw new ArgumentException("The choice display names must be the same length as the choices.", nameof(choiceDisplayNames));

			this.choices = new List<string>(choices);

			if(choiceDisplayNames != null)
				this.choiceDisplayNames = new List<string>(choiceDisplayNames);

			if (!this.choices.Contains(defaultValue))
				throw new ArgumentException("default value is not part of the choice set");
		}

		public override bool SetValue(string value)
		{
			if (!choices.Contains(value))
				return false;

			base.SetValue(value);
			return true;
		}
	}
}
