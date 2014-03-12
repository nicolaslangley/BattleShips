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
	public int shipSize;

	private bool selected = false;
	private GameObject system;
	private GameScript gameScript;
	private GridScript gridScript;
	private int[] health;

	private int speed; 
	private static int maxSpeed; //Speed at full health
	private int rotSteps = 1; // increments of 90 degrees. Most ships have 1, Torpedo Boats have 2

	private bool heavyArmor;
	
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
			if (GUI.Button(new Rect(Screen.width - 110, 50, 100, 30), "Fire Cannon")) {
				gameScript.curPlayAction = GameScript.PlayAction.Cannon;
			}
			if (GUI.Button(new Rect(Screen.width - 110, 90, 100, 30), "Rotate Clockwise")) {
				RotateShip(true);
			}
			if (GUI.Button(new Rect(Screen.width - 110, 130, 100, 30), "Rotate Counterclockwise")) {
				RotateShip(false);
			}
		}
	}

	/** GAMELOOP METHODS **/

	// Use this for initialization
	public void Init () {
		system = GameObject.FindGameObjectWithTag("System");
		gameScript = system.GetComponent<GameScript>();
		gridScript = system.GetComponent<GridScript>();
		// Change the size for each sub ship
		shipSize = 2;
		health = new int[shipSize];
		InitArmor ();
	}

	/*
	 * Fill in the health array so that this ship will have armor. 
	 * Normal armor => all cells are 1
	 * Heavy armor => all cells are 2
	 */
	private void InitArmor() {
		int armor = 1;
		if (heavyArmor) {
			armor = 2;
		}
		for (int i = 0; i < shipSize; i++) {
			health [i] = armor;
		}
	}

	/*
	 * Add damage to ship and recalculate speed.
	 */
	public void hit(int section) 
	{
		health [section] -= 1;
		int damageTotal = 0;
		for (int i = 0; i < shipSize; i++) {
			damageTotal += health[i];
		}
		if (damageTotal == 0) {
			Destroy(gameObject);
			//Take care of stats, etc.
		} else {
			if (heavyArmor)
					speed = maxSpeed * (damageTotal / (2 * shipSize));
			else
					speed = maxSpeed * (damageTotal / shipSize);
		}
	}

	public void CustomPlayUpdate () 
	{
		if (gameScript.curPlayAction == GameScript.PlayAction.Move) {

		}
		SetRotation();
	}

	/** COROUTINES **/

	// Coroutine for movement
	IEnumerator MoveShipForward (Vector3 destPos) {
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

	// Display the effects of shooting the cannon over a period of time
	IEnumerator DisplayCannon (GameObject target) {
		float startTime=Time.time; // Time.time contains current frame time, so remember starting point
		while(Time.time-startTime<=0.3){ // until one second passed
			target.renderer.material.color = Color.white; // lerp from A to B in one second
			yield return 1; // wait for next frame
		}
		target.renderer.material.color = Color.blue;
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
		cells.Add(destCell.gameObject);
		switch(curDir) {
		case GameScript.Direction.East:
			for (int i = 1; i < shipSize; i++) {
				GameObject newCell = gridScript.grid[destCell.gridPositionX + i, destCell.gridPositionY];
				CellScript newCellScript = newCell.GetComponent<CellScript>();
				newCellScript.occupier = this.gameObject;
				cells.Add (newCell);
			}
			break;
		case GameScript.Direction.North:
			for (int i = 1; i < shipSize; i++) {
				GameObject newCell = gridScript.grid[destCell.gridPositionX, destCell.gridPositionY + i];
				CellScript newCellScript = newCell.GetComponent<CellScript>();
				newCellScript.occupier = this.gameObject;
				cells.Add (newCell);
			}
			break;
		case GameScript.Direction.South:
			for (int i = 1; i < shipSize; i++) {
				GameObject newCell = gridScript.grid[destCell.gridPositionX, destCell.gridPositionY - i];
				CellScript newCellScript = newCell.GetComponent<CellScript>();
				newCellScript.occupier = this.gameObject;
				cells.Add (newCell);
			}
			break;
		case GameScript.Direction.West:
			for (int i = 1; i < shipSize; i++) {
				GameObject newCell = gridScript.grid[destCell.gridPositionX - i, destCell.gridPositionY];
				CellScript newCellScript = newCell.GetComponent<CellScript>();
				newCellScript.occupier = this.gameObject;
				cells.Add (newCell);
			}
			break;
		}

		foreach (GameObject o in cells) {
			o.GetComponent<CellScript>().occupier = this.gameObject;
			o.GetComponent<CellScript>().selected = true;
			o.GetComponent<CellScript>().DisplaySelection();
		}
	}

	/*
	 * Rotates the 
	 */
	public void RotateShip(bool clockwise) {
		//Calculate new turn direction
		int curRot = (int)curDir;
		int newRot;
		if (!clockwise) {
			newRot =(curRot - rotSteps);
			if (newRot == -1) newRot = 3;
		} else {
			newRot = ((curRot + rotSteps) % 4);
		}

		//Check for obstacles
		bool obstacle = false;
		CellScript cell = cells[0].GetComponent<CellScript>();
		if (curDir == GameScript.Direction.North || curDir == GameScript.Direction.South) {
			int sign = 1;
			if (curDir == GameScript.Direction.North && ! clockwise ||
			    curDir == GameScript.Direction.South && clockwise) sign = -1;

			int ysign = 1;
			if (curDir == GameScript.Direction.South) ysign = -1;
			for (int w = 1; w < shipSize; w++) {
				if (gridScript.GetCell(cell.gridPositionX+sign*w, cell.gridPositionY+ysign*w).GetComponent<CellScript>().curCellState != GameScript.CellState.Available) {
					obstacle = true;
					//break;
				}
				//For debugging
				//gridScript.GetCell(cell.gridPositionX+sign*w, cell.gridPositionY+ysign*w).renderer.material.color = Color.magenta;
			}

		} else {
			int sign = 1;
			if (curDir == GameScript.Direction.East && clockwise ||
			    curDir == GameScript.Direction.West && !clockwise) sign = -1;

			int xsign = 1;
			if (curDir == GameScript.Direction.West) xsign = -1;
			for (int w = shipSize-1; w > 0; w--) {
				if (gridScript.GetCell(cell.gridPositionX+xsign*w, cell.gridPositionY+sign*w).GetComponent<CellScript>().curCellState != GameScript.CellState.Available) {
					obstacle = true;
					break;
				}
				//For debugging
				//gridScript.GetCell(cell.gridPositionX+xsign*w, cell.gridPositionY+sign*w).renderer.material.color = Color.magenta;
			}
		}

		if (!obstacle) {
			curDir = (GameScript.Direction)newRot;
		} else {
			//display an error message
		}
	}

	// Fire cannon at targeted cell
	public void FireCannon(CellScript targetCell) {
		// Call coroutine to display fire outcome
		StartCoroutine(DisplayCannon(targetCell.gameObject));
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
