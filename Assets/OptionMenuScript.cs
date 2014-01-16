using UnityEngine;
using System.Collections;

public class OptionMenuScript : MonoBehaviour {

	void OnGUI () {
		GUI.Box (new Rect ((Screen.width / 2) - 100, (Screen.height / 2) - 120, 200, 25), "BATTLESHIP");
		GUI.Box (new Rect ((Screen.width / 2) - 100, (Screen.height / 2) - 80, 200, 25), "Options");
		GUI.Box (new Rect ((Screen.width / 2) - 75, (Screen.height / 2) - 30, 150, 25), "Sound on?"); 
		GUI.Box (new Rect ((Screen.width / 2) - 75, (Screen.height / 2) + 30, 150, 25), "Choose graphics level");
		GUI.Toggle (new Rect ((Screen.width / 2) + 85, (Screen.height / 2) - 30, 30, 20), false, "Y");
		GUI.Toggle (new Rect ((Screen.width / 2) + 125, (Screen.height / 2) - 30, 30, 20), false, "N");
		GUI.Toggle (new Rect ((Screen.width / 2) + 85, (Screen.height / 2) + 30, 50, 20), false, "Low");
		GUI.Toggle (new Rect ((Screen.width / 2) + 140, (Screen.height / 2) + 30, 50, 20), false, "High");

	}
}
