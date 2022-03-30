using Fab.Lua.Core;
using Fab.WorldMod;

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

		public override string ToString()
		{
			if (IsNil())
				return "nil";

			return $"dataset {{ name: {name} }}";
		}
	}
}
