using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class ShipScript : MonoBehaviour {
	[XmlAttribute("player")]
	public string player;
	public List<GameObject> cells;
	private bool selected = false;
	private GameObject system;
	private GUIScript guiScript;

	public ShipScript() {
		player = "Horatio";
	}

	// Use this for initialization
	public void Init () {
		system = GameObject.FindGameObjectWithTag("System");
		guiScript = system.GetComponent<GUIScript>();
	}

	// Handle clicking on object
	void OnMouseDown () {
		selected = !selected;
		if (selected == true) {
			gameObject.renderer.material.color = Color.cyan;
			foreach (GameObject o in cells) {
				CellScript cs = o.GetComponent<CellScript>();
				cs.selected = true;
				cs.DisplaySelection();
			}
		} else {
			gameObject.renderer.material.color = Color.white;
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

	// Handles movement of ship - INCOMPLETE
	void MoveShip (List<GameObject> newCells) {
		Vector3 startPos = newCells[0].transform.position;
		Vector3 endPos = newCells[newCells.Count - 1].transform.position;
		float newX = ((endPos.x - startPos.x) / 2) + startPos.x;
		float newZ = ((endPos.z - startPos.z) / 2) + startPos.z;
		float newY = 0.5f;
		Vector3 pos = new Vector3(newX, newY, newZ);
		gameObject.transform.position = pos;



	}


}
