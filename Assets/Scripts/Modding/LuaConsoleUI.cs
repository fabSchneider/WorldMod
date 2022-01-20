using Fab.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace Fab.Geo.Modding
{
    [RequireComponent(typeof(LuaConsole))]
    [RequireComponent(typeof(UIDocument))]
    public class LuaConsoleUI : MonoBehaviour
    {
        private static readonly string className = "lua-console";
        private static readonly string textFieldClassName = className + "__text-field";
        private static readonly string historyClassName = className + "__history";
        private static readonly string historyContainerName = "history-container";
        private static readonly string errorMsgClassName = className + "__error-msg";

        private LuaConsole console;
        private UIDocument doc;
        private TextField consoleTextField;
        private ScrollView consoleHistory;
        private Label errorMsg;

        private ObjectPool<HistoryEntryElement> historyEntryPool;

        /// <summary>
        /// The current position in the history triggered through the up arrow
        /// </summary>
        private int selectedHistoryEntry = 0;

        private void Start()
        {
            console = GetComponent<LuaConsole>();

            doc = GetComponent<UIDocument>();
            consoleTextField = doc.rootVisualElement.Q<TextField>(className: textFieldClassName);

            consoleTextField.RegisterCallback<KeyDownEvent>(OnTextFieldKeyDown);
            consoleTextField.RegisterCallback<NavigationSubmitEvent>(OnTextFieldSubmit);

            consoleHistory = doc.rootVisualElement.Q<ScrollView>(name: historyContainerName);
            historyEntryPool = new ObjectPool<HistoryEntryElement>(
                console.ConsoleHistory.MaxEntries,
                false,
                () => new HistoryEntryElement(),
                entry => entry.Reset());

            errorMsg = doc.rootVisualElement.Q<Label>(className: errorMsgClassName);
            errorMsg.style.display = DisplayStyle.None;
        }

        private void OnTextFieldSubmit(NavigationSubmitEvent evt)
        {
            consoleTextField.Focus();
            string code = consoleTextField.text;
            if (string.IsNullOrWhiteSpace(code))
                return;

            LuaConsole.Result res = console.Execute(code);
            if (res.success)
            {
                errorMsg.style.display = DisplayStyle.None;
                consoleTextField.SetValueWithoutNotify(string.Empty);
                UpdateHistory();
            }
            else
            {
                errorMsg.text = res.errorMsg;
                errorMsg.style.display = DisplayStyle.Flex;
            }


        }

        private void OnTextFieldKeyDown(KeyDownEvent evt)
        {
            if (console.ConsoleHistory.Count == 0)
                return;

            //navigate through history
            if (evt.keyCode == KeyCode.UpArrow && selectedHistoryEntry - 1 >= 0)
            {
                //deselect last
                if (selectedHistoryEntry < consoleHistory.childCount)
                    ((HistoryEntryElement)consoleHistory[selectedHistoryEntry]).SetSelected(false);

                //select next
                selectedHistoryEntry--;
                LuaConsole.HistoryEntry entry = console.ConsoleHistory[selectedHistoryEntry];
                ((HistoryEntryElement)consoleHistory[selectedHistoryEntry]).SetSelected(true);
                consoleTextField.SetValueWithoutNotify(entry.Code);
                evt.StopPropagation();
            }
            else if (evt.keyCode == KeyCode.DownArrow && selectedHistoryEntry + 1 < console.ConsoleHistory.Count)
            {
                //deselect last
                if (selectedHistoryEntry >= 0)
                    ((HistoryEntryElement)consoleHistory[selectedHistoryEntry]).SetSelected(false);

                //select next
                selectedHistoryEntry++;
                LuaConsole.HistoryEntry entry = console.ConsoleHistory[selectedHistoryEntry];
                ((HistoryEntryElement)consoleHistory[selectedHistoryEntry]).SetSelected(true);
                consoleTextField.SetValueWithoutNotify(entry.Code);
                evt.StopPropagation();
            }
            else
            {
                //deselect last 
                if(selectedHistoryEntry >= 0 && selectedHistoryEntry < consoleHistory.childCount)
                    ((HistoryEntryElement)consoleHistory[selectedHistoryEntry]).SetSelected(false);
            }


        }

        private void UpdateHistory()
        {
            for (int i = consoleHistory.childCount - 1; i >= 0; i--)
                historyEntryPool.ReturnToPool((HistoryEntryElement)consoleHistory[i]);

            foreach (var entry in console.ConsoleHistory)
            {
                HistoryEntryElement entryElement = historyEntryPool.GetPooled();
                entryElement.Set(entry.Code, entry.Print, entry.image);
                entryElement.SetSelected(false);
                consoleHistory.Add(entryElement);
            }

            selectedHistoryEntry = console.ConsoleHistory.Count;
        }
    }

    public class HistoryEntryElement : VisualElement
    {
        private static readonly string className = "history-entry";
        private static readonly string selectedClassName = className + "--selected";
        private static readonly string codeClassName = "history-entry__code";
        private static readonly string imgClassName = "history-entry__image";
        private static readonly string imageHiddenClassName = imgClassName + "--hidden";
        private static readonly string printClassName = "history-entry__print";
        private static readonly string printHiddenClassName = printClassName + "--hidden";

        private Label codeText;
        private Label printText;
        private UnityEngine.UIElements.Image img;

        public HistoryEntryElement()
        {
            AddToClassList(className);
            codeText = new Label();
            codeText.AddToClassList(codeClassName);
            img = new UnityEngine.UIElements.Image();
            img.AddToClassList(imgClassName);
            printText = new Label();
            printText.enableRichText = true;
            printText.AddToClassList(printClassName);

            Add(codeText);
            Add(img);
            Add(printText);
        }

        public void Set(string code, string print, Texture2D image)
        {
            codeText.text = code;

            if (string.IsNullOrEmpty(print))
            {
                printText.text = string.Empty;
                printText.AddToClassList(printHiddenClassName);
            }
            else
            {
                printText.text = print;
                printText.RemoveFromClassList(printHiddenClassName);
            }

            if (image)
            {
                img.image = image;
                img.RemoveFromClassList(imageHiddenClassName);
            }
            else
            {
                img.image = null;
                img.AddToClassList(imageHiddenClassName);
            }
        }

        public void Reset()
        {
            codeText.text = string.Empty;
            printText.text = string.Empty;
            img.image = null;
        }

        public void SetSelected(bool selected)
        {
            if (selected)
                AddToClassList(selectedClassName);
            else
                RemoveFromClassList(selectedClassName);
        }
    }
}
