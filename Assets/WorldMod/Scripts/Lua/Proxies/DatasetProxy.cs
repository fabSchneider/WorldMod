using Fab.Geo.Lua.Interop;
using Fab.Lua.Core;
using Fab.WorldMod;
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

		[LuaHelpInfo("Sets the texture of this dataset")]
		public ImageProxy texture
		{
			set => target.SetData("texture", value.Target);
		}

		[LuaHelpInfo("Sets the mode of this dataset")]
		public string mode
		{
			set => target.SetData("mode", value);
		}

		public DatasetProxy(Dataset target)
		{
			this.target = target;
		}

		public override string ToString()
		{
			if (IsNil())
				return "nil";

			return $"dataset {{ name: {name} }}";
		}
	}
}
