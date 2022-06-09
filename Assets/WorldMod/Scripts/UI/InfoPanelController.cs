using Fab.Common;
using Fab.Localization;
using UnityEngine.UIElements;

namespace Fab.WorldMod.UI
{
    public class InfoPanelController 
    {
		private static readonly string textClassname = "info-panel__text";

		private static readonly string colorDataKey = "color";

		VisualElement infoPanel;
		Localizable infoText;
		public InfoPanelController(VisualElement root)
		{
			infoPanel = root.Q(name: "info-panel");
			Label infoLabel = new Label();
			infoLabel.enableRichText = true;	
			infoLabel.AddToClassList(textClassname);

			infoText = new Localizable(LocalizationComponent.Localization);
			infoLabel.AddManipulator(infoText);
			infoPanel.Add(infoLabel);

			Signals.Get<DatasetSelectedSignal>().AddListener(OnDatasetSelected);
		}

		private void OnDatasetSelected(Dataset dataset)
		{
			if(dataset == null)
			{
				//infoText.target.style.display = DisplayStyle.None;
				infoText.SetKey("$PROJECT_INFO");
			}
			else
			{
				//infoText.target.style.display = DisplayStyle.Flex;
				infoText.SetKey(dataset.Name + "_INFO");
			}	
		}

		public void SetInfoText(string textKey)
		{
			infoText.SetKey(textKey);
		}
	}
}
