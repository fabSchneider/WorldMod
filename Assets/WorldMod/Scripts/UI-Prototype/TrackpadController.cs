using UnityEngine;
using UnityEngine.UIElements;

namespace Fab.WorldMod.UI
{
    public class TrackpadController 
    {
		public TrackpadController(VisualElement root)
		{
			root.Q<Trackpad>().RegisterCallback<ChangeEvent<Vector2>>(OnTrackpadAxis);
		}

		public void OnTrackpadAxis(ChangeEvent<Vector2> evt)
		{
			Debug.Log(evt.newValue);
		}
    }
}
