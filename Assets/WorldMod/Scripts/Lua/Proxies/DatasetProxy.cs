using System;
using System.Linq;
using Fab.Lua.Core;
using MoonSharp.Interpreter;

namespace Fab.WorldMod.Lua
{

	[LuaHelpInfo("A dataset")]
	[LuaName("dataset")]
	public class DatasetProxy : LuaProxy<Dataset>
	{
		[LuaHelpInfo("The name of the dataset")]
		public string name
		{
			get => Target.Name;
			set => Target.Name = value;
		}

		public DatasetProxy(Dataset target)
		{
			this.target = target;
		}

		[LuaHelpInfo("Adds data to the data set")]
		public void add(Table table)
		{
			foreach (var pair in table.Pairs)
			{
				DynValue value = pair.Value;
				object obj = GetValue(value);
				target.SetData(pair.Key.String, obj);
			}
		}

		[LuaHelpInfo("Gets data from the data set")]
		public object get(DynValue key)
		{
			if (target.TryGetData(key.String, out object data))
			{
				return data;
			}
			else
			{
				return null;
			}
		}

		private object GetValue(DynValue value)
		{
			Table valTable = value.Table;
			if (valTable != null)
			{
				//// NOTE: this is really hacky
				//if (TryConvert(value, "Color", out Color color))
				//	obj = color;
				//else if (TryConvert(value, "Vector", out Vector3 vector))
				//	obj = vector;
				//else
				return valTable.Values.Select(dynVal => GetValue(dynVal)).ToArray();

			}
			else
			{
				object obj = value.ToObject();
				if (obj is double)
					return Convert.ToSingle(obj);
				else if (obj is LuaProxy proxy)
					return proxy.TargetObject;
				else
					return obj;
			}
		}

		public override string ToString()
		{
			if (IsNil())
				return "nil";

			return $"dataset {{ name: {name} }}";
		}
	}
}
