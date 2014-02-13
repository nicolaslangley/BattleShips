using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class ShipScript : MonoBehaviour {
	[XmlAttribute("player")]

	/** Properties **/

	public string player;
	public List<GameObject> cells;
	public GameScript.Direction curDir;

	private bool selected = false;
	private GameObject system;
	private GUIScript guiScript;

	public ShipScript() {
		player = "Horatio";
	}

	/** UNITY METHODS **/

	// Handle clicking on object
	void OnMouseDown () {
		selected = !selected;
		if (selected == true) {
			//TODO: Fix this on selection for gameobject
			//gameObject.renderer.material.color = Color.cyan;
			foreach (GameObject o in cells) {
				CellScript cs = o.GetComponent<CellScript>();
				cs.selected = true;
				cs.DisplaySelection();
			}
		} else {
			//gameObject.renderer.material.color = Color.white;
			foreach (GameObject o in cells) {
				CellScript cs = o.GetComponent<CellScript>();
				cs.selected = false;
				cs.DisplaySelection();
			}
		} 
	}

	// Display movement options for selected ship
	void OnGUI () {
		if (selected == true) {
			if (GUI.Button(new Rect(Screen.width - 110, 10, 100, 30), "Move")) {
				guiScript.allowShipMovement = true;
			}
		}
	}

	/** GAMELOOP METHODS **/

	// Use this for initialization
	public void Init () {
		system = GameObject.FindGameObjectWithTag("System");
		guiScript = system.GetComponent<GUIScript>();
	}

	/** HELPER METHODS **/

	// Handles movement of ship - INCOMPLETE
	void MoveShip (int amount, GameScript.Direction dir) {

	}
}
