using Fab.WorldMod;
using Fab.Lua.Core;
using UnityEngine;

namespace WorldMod.Lua
{
	[LuaName("controls")]
	public class ControlsModule : LuaObject, ILuaObjectInitialize
	{
		public void Initialize() { }

		public FloatControlProxy number(string name, float default_value)
		{
			ValueControl<float> control = new ValueControl<float>(name, default_value);
			FloatControlProxy proxy = new FloatControlProxy();
			proxy.SetTarget(control);
			return proxy;
		}

		public FloatControlProxy slider(string name, float default_value, float min = 0f, float max = 1f)
		{
			SliderControl control = new SliderControl(name, default_value, min, max);
			FloatControlProxy proxy = new FloatControlProxy();
			proxy.SetTarget(control);
			return proxy;
		}
		public VectorControlProxy range(string name, float default_lower, float default_upper, float min, float max)
		{
			RangeControl control = new RangeControl(name, default_lower, default_upper, min, max);
			VectorControlProxy proxy = new VectorControlProxy();
			proxy.SetTarget(control);
			return proxy;
		}

		public BoolControlProxy toggle(string name, bool default_value)
		{
			ValueControl<bool> control = new ValueControl<bool>(name, default_value);
			BoolControlProxy proxy = new BoolControlProxy();
			proxy.SetTarget(control);
			return proxy;
		}

		public StringControlProxy text(string name, string default_value)
		{
			ValueControl<string> control = new ValueControl<string>(name, default_value);
			StringControlProxy proxy = new StringControlProxy();
			proxy.SetTarget(control);
			return proxy;
		}

		public StringControlProxy choice(string name, string default_value, string[] choices, string[] choice_display_names = null)
		{
			ChoiceControl control = new ChoiceControl(name, default_value, choices, choice_display_names);
			StringControlProxy proxy = new StringControlProxy();
			proxy.SetTarget(control);
			return proxy;
		}

		public VectorControlProxy vector(string name, Vector3 default_value)
		{
			ValueControl<Vector3> control = new ValueControl<Vector3>(name, default_value);
			VectorControlProxy proxy = new VectorControlProxy();
			proxy.SetTarget(control);
			return proxy;
		}

		public ColorControlProxy color(string name, Color default_value)
		{
			ValueControl<Color> control = new ValueControl<Color>(name, default_value);
			ColorControlProxy proxy = new ColorControlProxy();
			proxy.SetTarget(control);
			return proxy;
		}
	}
}
