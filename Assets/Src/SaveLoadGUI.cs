using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;

public class SaveLoadGUI : MonoBehaviour {

	GameSaverScript gameSaver;
	
	void OnGUI() {
		if(GUI.Button(new Rect(Screen.width-150,10,100,80), "Load")) {
			GameSaverScript.Load(Path.Combine(Application.persistentDataPath, "ships_saved.xml"),GetComponent<GameScript>());
			Debug.Log("Loaded " + Path.Combine(Application.persistentDataPath, "ships_saved.xml").ToString());
			/*
			 * Things we need to set for things on load:
			 * -ShipScript.gameScript
			 * -ShipScript.gridScript
			 * -ShipScript.rpcScript
			 */

		}
		if (GUI.Button (new Rect (Screen.width-150, 90, 100, 80), "Save")) {
			gameSaver = new GameSaverScript(GetComponent<GameScript>());
			gameSaver.Save(Path.Combine(Application.persistentDataPath, "ships_saved.xml"));
			Debug.Log("Saved to " + Path.Combine(Application.persistentDataPath, "ships_saved.xml").ToString());
		}

	}
}
