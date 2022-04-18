using Fab.Lua.Core;
using UnityEngine;

namespace Fab.WorldMod.Lua
{
	[LuaHelpInfo("Module for creating a range of different controls")]
	[LuaName("controls")]
	public class ControlsModule : LuaObject, ILuaObjectInitialize
	{
		public void Initialize() { }

		[LuaHelpInfo("A control holding a number")]
		public FloatControlProxy number(string name, float default_value)
		{
			ValueControl<float> control = new ValueControl<float>(name, default_value);
			FloatControlProxy proxy = new FloatControlProxy();
			proxy.SetTarget(control);
			return proxy;
		}

		[LuaHelpInfo("A control holding a number within a range")]
		public FloatControlProxy range(string name, float default_value, float min = 0f, float max = 1f)
		{
			RangeControl control = new RangeControl(name, default_value, min, max);
			FloatControlProxy proxy = new FloatControlProxy();
			proxy.SetTarget(control);
			return proxy;
		}

		[LuaHelpInfo("A control holding an interval with a lower and upper bound")]
		public VectorControlProxy interval(string name, float default_lower, float default_upper, float min, float max)
		{
			IntervalControl control = new IntervalControl(name, default_lower, default_upper, min, max);
			VectorControlProxy proxy = new VectorControlProxy();
			proxy.SetTarget(control);
			return proxy;
		}

		[LuaHelpInfo("A control holding a boolean value")]
		public BoolControlProxy toggle(string name, bool default_value)
		{
			ValueControl<bool> control = new ValueControl<bool>(name, default_value);
			BoolControlProxy proxy = new BoolControlProxy();
			proxy.SetTarget(control);
			return proxy;
		}

		[LuaHelpInfo("A control holding text")]
		public StringControlProxy text(string name, string default_value)
		{
			ValueControl<string> control = new ValueControl<string>(name, default_value);
			StringControlProxy proxy = new StringControlProxy();
			proxy.SetTarget(control);
			return proxy;
		}

		[LuaHelpInfo("A control holding a set of choices")]
		public StringControlProxy choice(string name, string default_value, string[] choices, string[] choice_display_names = null)
		{
			ChoiceControl control = new ChoiceControl(name, default_value, choices, choice_display_names);
			StringControlProxy proxy = new StringControlProxy();
			proxy.SetTarget(control);
			return proxy;
		}

		[LuaHelpInfo("A control holding a vector")]
		public VectorControlProxy vector(string name, Vector3 default_value)
		{
			ValueControl<Vector3> control = new ValueControl<Vector3>(name, default_value);
			VectorControlProxy proxy = new VectorControlProxy();
			proxy.SetTarget(control);
			return proxy;
		}

		[LuaHelpInfo("A control holding a color")]
		public ColorControlProxy color(string name, Color default_value)
		{
			ValueControl<Color> control = new ValueControl<Color>(name, default_value);
			ColorControlProxy proxy = new ColorControlProxy();
			proxy.SetTarget(control);
			return proxy;
		}
	}
}
