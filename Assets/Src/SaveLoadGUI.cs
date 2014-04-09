using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;

public class SaveLoadGUI : MonoBehaviour {

	GameSaverScript gameSaver;
	
	void OnGUI() {
		GameScript.GameState gamestate = GetComponent<GameScript> ().curGameState;
		if (Network.peerType == NetworkPeerType.Server && gamestate == GameScript.GameState.Play
		    || gamestate == GameScript.GameState.Wait) {
			if (GUI.Button (new Rect (10, Screen.height-40, 100, 40), "Save")) {
				gameSaver = new GameSaverScript(GetComponent<GameScript>());
				string filename = gameSaver.myname + "_" + gameSaver.opponentname + ".xml";
				gameSaver.Save(Path.Combine("Assets/Saves", filename));
				Debug.Log("Saved to " + Path.Combine("Assets/Saves", filename).ToString());
			}
		}
	}
}
