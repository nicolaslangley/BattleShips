using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;

public class SaveLoadGUI : MonoBehaviour {

	public ShipContainer ships = new ShipContainer();
	void OnGUI() {
		if(GUI.Button(new Rect(10,10,100,80), "Load")) {
			ships = ShipContainer.Load(Path.Combine(Application.persistentDataPath, "ships_saved.xml"));
			Debug.Log("Loaded " + Path.Combine(Application.persistentDataPath, "ships_saved.xml").ToString());

		}
		if (GUI.Button (new Rect (10, 90, 100, 80), "Save")) {
			ships.Save(Path.Combine(Application.persistentDataPath, "ships_saved.xml"));
			Debug.Log("Saved to " + Path.Combine(Application.persistentDataPath, "ships_saved.xml").ToString());
		}

	}
}
