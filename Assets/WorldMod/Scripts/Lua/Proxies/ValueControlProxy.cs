using Fab.Lua.Core;
using MoonSharp.Interpreter;
using UnityEngine;

namespace Fab.WorldMod.Lua
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

		[LuaHelpInfo("Sets the value of the control")]
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

	[LuaHelpInfo("A boolean control")]
	public class BoolControlProxy : ValueControlProxy<bool> { }

	[LuaHelpInfo("A number control")]
	public class FloatControlProxy : ValueControlProxy<float> { }

	[LuaHelpInfo("A vector control")]
	public class VectorControlProxy : ValueControlProxy<Vector3> { }

	[LuaHelpInfo("A color control")]
	public class ColorControlProxy : ValueControlProxy<Color> { }

	[LuaHelpInfo("A text control")]
	public class StringControlProxy : ValueControlProxy<string> { }
}
