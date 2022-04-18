using Fab.Lua.Core;
using Fab.WorldMod;
using MoonSharp.Interpreter;
using UnityEngine;

namespace WorldMod.Lua
{
	public interface IValueControlProxy
	{
		ValueControl Target { get; }
	}

	public abstract class ValueControlProxy<T> : LuaProxy<ValueControl<T>>, IValueControlProxy
	{
		private Closure onValueChange;

		[LuaHelpInfo("The name of the control")]
		public string name => target.Name;

		[LuaHelpInfo("The default value of the control")]
		public T default_value => target.DefaultValue;

		[LuaHelpInfo("The current value of the control")]
		public T value => target.Value;

		ValueControl IValueControlProxy.Target => Target;

		[LuaHelpInfo("Add a function to be executed when the value of this control changes")]
		public void on_change(Closure callback)
		{
			ThrowIfNil();

			onValueChange = callback;
			Target.UnregisterChangeCallback(OnValueChange);
			if (onValueChange != null)
				Target.RegisterChangeCallback(OnValueChange);
		}

		public void set(T value)
		{
			Target.SetValue(value);
		}


		protected void OnValueChange(T value)
		{
			onValueChange?.Call(value);
		}


		public override string ToString()
		{
			if (IsNil())
				return "Nil";

			return $"{name} ({target.Type.Name}-Control)";
		}
	}

	public class BoolControlProxy : ValueControlProxy<bool> { }
	public class FloatControlProxy : ValueControlProxy<float> { }
	public class VectorControlProxy : ValueControlProxy<Vector3> { }
	public class ColorControlProxy : ValueControlProxy<Color> { }
	public class StringControlProxy : ValueControlProxy<string> { }
}
