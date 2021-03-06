using UnityEngine;

namespace Fab.WorldMod
{
    public class MultiDisplayManager : MonoBehaviour
    {
        void Awake()
        {
			if (Display.displays.Length > 1)
			{
				Debug.Log("Found multiple displays. Activating display 1");
				Display.displays[1].Activate();
			}
		}
    }
}
