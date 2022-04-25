using System;
using UnityEngine.UIElements;

namespace Fab.WorldMod
{
    public class LogOutputController 
    {
		private static readonly string containerClassname = "output-container";
		private static readonly string entryClassname = "output__entry";
		private static readonly string entryCurrentClassname = entryClassname + "--current";

		VisualElement outputContainer;


		public int maxEntries = 8;

		public LogOutputController(VisualElement root)
		{
			outputContainer = root.Q(className: containerClassname);
		}

		private Label CreateConsoleEntry()
		{
			var entry = new Label();
			entry.enableRichText = true;
			entry.AddToClassList(entryClassname);
			return entry;
		}

		public void Log(string message)
		{
			Label entry; 
			if(outputContainer.childCount > maxEntries)
			{
				entry = outputContainer[0] as Label;
				entry.RemoveFromHierarchy();
			}
			else
			{
				entry = CreateConsoleEntry();
			}

			if (outputContainer.childCount > 0)
				outputContainer[outputContainer.childCount - 1].RemoveFromClassList(entryCurrentClassname);

			entry.AddToClassList(entryCurrentClassname);
			outputContainer.Add(entry);
			entry.text = CreateTimestamp() + ' ' + message;
		}

		public void Clear()
		{
			outputContainer.Clear();
		}

		private static readonly string dateTimeFormat = "HH:mm:ss";

		private string CreateTimestamp()
		{
			return '[' + DateTime.Now.ToString(dateTimeFormat) + ']';
		}
    }
}
