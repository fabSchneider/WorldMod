using Fab.Common;
using Fab.WorldMod.Localization;
using UnityEngine.UIElements;

namespace Fab.WorldMod.UI
{
    public class InfoPanelController 
    {
		VisualElement infoPanel;
		Localizable infoText;
		public InfoPanelController(VisualElement root)
		{
			infoPanel = root.Q(name: "info-panel");
			Label infoLabel = new Label();
			infoText = new Localizable(LocalizationComponent.Localization);
			infoLabel.AddManipulator(infoText);
			infoPanel.Add(infoLabel);

			Signals.Get<DatasetActivatedSignal>().AddListener(OnDatasetActivated);
		}

		private void OnDatasetActivated(Dataset dataset)
		{
			infoText.SetKey(dataset.Name + "_INFO");
		}
	}
}
