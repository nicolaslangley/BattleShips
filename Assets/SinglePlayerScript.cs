using UnityEngine;
using System.Collections;

public class SinglePlayerScript : MonoBehaviour {

	void OnGUI () {
		GUI.Box (new Rect ((Screen.width / 2) - 100, (Screen.height / 2) - 80, 200, 25), "BATTLESHIP");
		GUI.Box (new Rect ((Screen.width / 2) - 75, (Screen.height / 2) - 30, 150, 25), "Choose map size"); 
		GUI.Box (new Rect ((Screen.width / 2) - 75, (Screen.height / 2) + 30, 150, 25), "Choose difficulty");
		GUI.Toggle (new Rect ((Screen.width / 2) + 85, (Screen.height / 2) - 30, 30, 20), false, "S");
		GUI.Toggle (new Rect ((Screen.width / 2) + 125, (Screen.height / 2) - 30, 30, 20), false, "M");
		GUI.Toggle (new Rect ((Screen.width / 2) + 165, (Screen.height / 2) - 30, 30, 20), false, "L");
		GUI.Toggle (new Rect ((Screen.width / 2) + 85, (Screen.height / 2) + 30, 50, 20), false, "Easy");
		GUI.Toggle (new Rect ((Screen.width / 2) + 140, (Screen.height / 2) + 30, 50, 20), false, "Hard");

	}
}
