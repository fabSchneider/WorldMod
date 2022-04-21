using Fab.Lua.Core;
using MoonSharp.Interpreter;
using UnityEngine;

namespace Fab.WorldMod.Lua
{
	[LuaHelpInfo("Module to alter the data layers")]
	[LuaName("sequence")]
	public class SequenceModule : LuaObject, ILuaObjectInitialize
	{
		private DatasetSequence sequence;

		private Closure onChange;

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
			sequence.InsertIntoSequence(dataset.Target, index -  1);
			//Signals.Get<DatasetUpdatedSignal>().Dispatch(dataset.Target);
		}

		[LuaHelpInfo("Removes a dataset from the sequence")]
		public void remove(DatasetProxy dataset)
		{
			sequence.RemoveFromSequence(dataset.Target);
			//Signals.Get<DatasetUpdatedSignal>().Dispatch(dataset.Target);
		}

		[LuaHelpInfo("Adds a function to be executed when the sequence changes")]
		public void on_change(Closure callback)
		{
			onChange = callback;
			sequence.sequenceChanged -= OnSequenceChange;
			if (onChange != null)
				sequence.sequenceChanged += OnSequenceChange;
		}

		[LuaHelpInfo("Returns the index of the dataset in the sequence. Returns 0 if the dataset is not in the sequence")]
		public int index_of(DatasetProxy proxy)
		{
			return sequence.GetIndexInSequence(proxy.Target) + 1;
		}

		protected void OnSequenceChange(Dataset dataset, DatasetSequence.ChangeEventType eventType, int lastIndex)
		{
			onChange?.Call(new DatasetProxy(dataset), eventType.ToString(), lastIndex + 1);
		}
	}
}
