using Fab.Geo.Lua.Interop;
using Fab.Lua.Core;
using Fab.WorldMod;
using MoonSharp.Interpreter;
using UnityEngine;

namespace WorldMod.Lua
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

		public void set(Table table)
		{
			foreach (var pair in table.Pairs)
			{
				object obj = pair.Value.ToObject();
				if (obj is LuaProxy proxy)
					obj = proxy.TargetObject;

				target.SetData(pair.Key.String, obj);
			}
		}

		public object get(DynValue key)
		{
			if(target.TryGetData(key.String, out object data))
			{
				return data;
			}else
			{
				return null;
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
