using Fab.Lua.Core;
using MoonSharp.Interpreter;
using UnityEngine;

namespace Fab.WorldMod.Lua
{
	[LuaHelpInfo("Module to alter the data layers")]
	[LuaName("sequence")]
	public class SequenceModule : LuaObject, ILuaObjectInitialize
	{
		private Sequence<Dataset> sequence;

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
			sequence.Insert(dataset.Target, sequence.Count);
			//Signals.Get<DatasetUpdatedSignal>().Dispatch(dataset.Target);
		}

		[LuaHelpInfo("Inserts a dataset into the sequence")]
		public void insert(DatasetProxy dataset, int index)
		{
			sequence.Insert(dataset.Target, index -  1);
			//Signals.Get<DatasetUpdatedSignal>().Dispatch(dataset.Target);
		}

		[LuaHelpInfo("Removes a dataset from the sequence")]
		public void remove(DatasetProxy dataset)
		{
			sequence.Remove(dataset.Target);
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

		private void OnSequenceChange(SequenceChangedEvent<Dataset> evt)
		{
			onChange?.Call(new DatasetProxy(evt.data), evt.changeType.ToString(), evt.lastIndex + 1);
		}

		[LuaHelpInfo("Returns the index of the dataset in the sequence. Returns 0 if the dataset is not in the sequence")]
		public int index_of(DatasetProxy proxy)
		{
			return sequence.IndexOf(proxy.Target) + 1;
		}
	}
}
