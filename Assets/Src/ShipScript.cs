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
			gameScript.selectedShip = this;
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
		if (gameScript.curPlayAction == GameScript.PlayAction.Move) {

		}
		SetRotation();
	}

	/** COROUTINES **/

	// Coroutine for movement
	IEnumerator MoveShipForward (Vector3 destPos){
		Vector3 start = transform.position;
		Vector3 dest = transform.position;
		float amount;
		switch(curDir) {
		case GameScript.Direction.East:
			amount = destPos.x - start.x;
			dest.x += amount;
			break;
		case GameScript.Direction.North:
			amount = destPos.z - start.z;
			dest.z += amount;
			break;
		case GameScript.Direction.South:
			amount = destPos.z - start.z;
			dest.z += amount;
			break;
		case GameScript.Direction.West:
			amount = destPos.x - start.x;
			dest.x += amount;
			break;
		}
		float startTime=Time.time; // Time.time contains current frame time, so remember starting point
		while(Time.time-startTime<=1){ // until one second passed
			transform.position=Vector3.Lerp(start,dest,Time.time-startTime); // lerp from A to B in one second
			yield return 1; // wait for next frame
		}
	}

	/** HELPER METHODS **/

	// Handles movement of ship - INCOMPLETE
	public void MoveShip (CellScript destCell) {
		// TODO: Check that destination cell is a valid destination and otherwise modify path
		StartCoroutine(MoveShipForward(destCell.transform.position));
		// Update occupied cells
		// Reset currently occupied cells
		foreach (GameObject o in cells) {
			o.GetComponent<CellScript>().occupier = null;
			o.GetComponent<CellScript>().selected = false;
			o.GetComponent<CellScript>().DisplaySelection();
		}
		cells.Clear();
		// Add newly occupied cells
		destCell.occupier = this.gameObject;
		destCell.selected = true;
		destCell.DisplaySelection();
		cells.Add(destCell.gameObject);
	}

	// Set rotation of ship based on direction
	public void SetRotation () {
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
