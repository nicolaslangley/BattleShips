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
	private GameScript gameScript;

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
				 gameScript.curPlayAction = GameScript.PlayAction.Move;
			}
		}
	}

	/** GAMELOOP METHODS **/

	// Use this for initialization
	public void Init () {
		system = GameObject.FindGameObjectWithTag("System");
		gameScript = system.GetComponent<GameScript>();
	}

	public void CustomPlayUpdate () {
		SetRotation();
	}

	/** HELPER METHODS **/

	// Handles movement of ship - INCOMPLETE
	void MoveShip (int amount, GameScript.Direction dir) {
		Vector3 dest = transform.position;
		switch(dir) {
		case GameScript.Direction.East:
			dest.x += amount;
			break;
		}
		Vector3.Lerp(transform.position, dest, 2.0f);
	}

	// Set rotation of ship based on direction
	void SetRotation () {
		Quaternion tempRot = Quaternion.identity;
		switch(curDir) {
		case GameScript.Direction.East:
			tempRot.eulerAngles = new Vector3(0, 90, 0);
			break;
		case GameScript.Direction.North:
			tempRot = Quaternion.identity;
			break;
		case GameScript.Direction.South:
			tempRot.eulerAngles = new Vector3(0, 180, 0);
			break;
		case GameScript.Direction.West:
			tempRot.eulerAngles = new Vector3(0, 270, 0);
			break;
		default:
			break;
		}
		transform.rotation = tempRot;
	}
}
