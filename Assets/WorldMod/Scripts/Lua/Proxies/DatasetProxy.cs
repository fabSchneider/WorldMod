using System;
using System.Linq;
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
				object obj;
				DynValue value = pair.Value;
				Table valTable = value.Table;
				if (valTable != null)
				{
					// NOTE: this is really hacky
					if (TryConvert(value, "Color", out Color color))
						obj = color;
					else if (TryConvert(value, "Vector", out Vector3 vector))
						obj = vector;
					else
						obj = valTable.Values.AsObjects().ToArray();
					
				}
				else
				{
					obj = pair.Value.ToObject();
					if(obj is double)
						obj = Convert.ToSingle(obj);
					else if (obj is LuaProxy proxy)
						obj = proxy.TargetObject;
				}

				target.SetData(pair.Key.String, obj);
			}
		}

		private bool TryConvert<T>(DynValue value, string varName, out T converted)
		{
			Table table = value.Table;
			if (table != null)
			{
				Script script = table.OwnerScript;
				var colorTbl = script.Globals.RawGet(varName);

				if (colorTbl != null &&
					table.MetaTable != null &&
					table.MetaTable.MetaTable != null &&
					colorTbl.Table.MetaTable.ReferenceID == table.MetaTable.MetaTable.ReferenceID)
				{
					converted = value.ToObject<T>();
					return true;
				}
			}

			converted = default;
			return false;
		}

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

		public override string ToString()
		{
			if (IsNil())
				return "nil";

			return $"dataset {{ name: {name} }}";
		}
	}
}
