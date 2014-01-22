using UnityEngine;
using System.Collections;

public class GUIScript : MonoBehaviour {
		
	public GameObject ship;
	public bool allowShipPlacement = false;

	void OnGUI () {
		if (allowShipPlacement == false) {
			if (GUI.Button(new Rect(10, 10, 100, 30), "Place Ship")) {
				Debug.Log("Place ship button hit");
				// Check squares have been selected that match ship requirements
				allowShipPlacement = true;
			}
		} else {
			if (GUI.Button(new Rect(10, 10, 100, 30), "Done")) {
				Debug.Log("Done button hit");
				// Check squares have been selected that match ship requirements
				allowShipPlacement = false;
			}
		}
	}
}
