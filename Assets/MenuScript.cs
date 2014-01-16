using UnityEngine;
using System.Collections;

public class MenuScript : MonoBehaviour {

	void OnGUI () {
		GUI.Box (new Rect ((Screen.width / 2) - 100, (Screen.height / 2) - 80, 200, 25), "BATTLESHIP");
		GUI.Box (new Rect ((Screen.width / 2) - 50, (Screen.height / 2) - 30, 100, 25), "Single Player"); 
		GUI.Box (new Rect ((Screen.width / 2) - 50, (Screen.height / 2) + 30, 100, 25), "Multi Player");
	}
}
