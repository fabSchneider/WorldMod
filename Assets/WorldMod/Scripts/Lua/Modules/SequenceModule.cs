using Fab.Lua.Core;
using Fab.WorldMod;

namespace WorldMod.Lua
{
	[LuaHelpInfo("Module to alter the data layers")]
	[LuaName("sequence")]
	public class SequenceModule : LuaObject, ILuaObjectInitialize
	{
		private DatasetSequence sequence;
		public void Initialize()
		{
			var comp = UnityEngine.Object.FindObjectOfType<DatasetsComponent>();

			if (comp == null)
				throw new LuaObjectInitializationException("Could not find dataset component");
			sequence = comp.Sequence;
		}


		[LuaHelpInfo("Adds a dataset to the end of the sequence")]
		public void add(DatasetProxy dataset)
		{
			sequence.InsertIntoSequence(dataset.Target, sequence.Count);
			//Signals.Get<DatasetUpdatedSignal>().Dispatch(dataset.Target);
		}

		[LuaHelpInfo("Inserts a dataset into the sequence")]
		public void insert(DatasetProxy dataset, int index)
		{
			sequence.InsertIntoSequence(dataset.Target, index);
			//Signals.Get<DatasetUpdatedSignal>().Dispatch(dataset.Target);
		}

		[LuaHelpInfo("Removes a dataset from the sequence")]
		public void remove(DatasetProxy dataset, int index)
		{
			sequence.RemoveFromSequence(dataset.Target);
			//Signals.Get<DatasetUpdatedSignal>().Dispatch(dataset.Target);
		}
	}
}
