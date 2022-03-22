using UnityEngine;

namespace Fab.WorldMod
{
    public class DisplayManager : MonoBehaviour
    {
        void Awake()
        {
			if (Display.displays.Length > 1)
				Display.displays[1].Activate();
		}
    }
}
