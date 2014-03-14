using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class ShipScript : MonoBehaviour {
	[XmlAttribute("player")]

	/** Properties **/

	public string shipID;

	public string player;
	public List<GameObject> cells;
	public GameScript.Direction curDir;
	public int shipSize;

	private bool selected = false;
	private GameObject system;
	private GameScript gameScript;
	private GridScript gridScript;
	private RPCScript rpcScript;
	private int[] health;
	private CellScript baseCell;

	private int speed; 
	private static int maxSpeed; //Speed at full health
	private int rotSteps = 1; // increments of 90 degrees. Most ships have 1, Torpedo Boats have 2

	private bool heavyArmor;
	
	public ShipScript() {
		player = "Horatio";
		shipID = "ABC";
	}

	/** UNITY METHODS **/

	// Handle clicking on object
	void OnMouseDown () {
		// Don't act on mouse click if in wait state
		if (gameScript.curGameState == GameScript.GameState.Wait) return;

		selected = !selected;
		if (selected == true) {
			//TODO: Fix this on selection for gameobject
			//gameObject.renderer.material.color = Color.cyan;
			foreach (GameObject o in cells) {
				CellScript cs = o.GetComponent<CellScript>();
				cs.selected = true;
				//cs.DisplaySelection();
			}
			gameScript.selectedShip = this;
		} else {
			//gameObject.renderer.material.color = Color.white;
			foreach (GameObject o in cells) {
				CellScript cs = o.GetComponent<CellScript>();
				cs.selected = false;
				//cs.DisplaySelection();
			}
		} 
	}

	// Display movement options for selected ship
	void OnGUI () {
		if (selected == true) {
			if (GUI.Button(new Rect(Screen.width - 110, 10, 100, 30), "Move")) {
				gameScript.curPlayAction = GameScript.PlayAction.Move;
				// Display movement range in cells
				DisplayMoveRange(true);
			}
			if (GUI.Button(new Rect(Screen.width - 110, 50, 100, 30), "Fire Cannon")) {
				gameScript.curPlayAction = GameScript.PlayAction.Cannon;
				// Display cannon range in cells
			}
			if (GUI.Button(new Rect(Screen.width - 110, 90, 100, 30), "Rotate Clockwise")) {
				RotateShip(true);
			}
			if (GUI.Button(new Rect(Screen.width - 110, 130, 100, 30), "Rotate Counterclockwise")) {
				RotateShip(false);
			}
			if (GUI.Button(new Rect(Screen.width - 110, 170, 100, 30), "Cancel Action")) {
				gameScript.curPlayAction = GameScript.PlayAction.None;
				DisplayMoveRange(false);
			}
		}
	}

	/** GAMELOOP METHODS **/

	// Use this for initialization
	public void Init () {
		system = GameObject.FindGameObjectWithTag("System");
		gameScript = system.GetComponent<GameScript>();
		gridScript = system.GetComponent<GridScript>();
		rpcScript = system.GetComponent<RPCScript>();
		// Change the size for each sub ship
		shipSize = 2;
		speed = 4;
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

	public void CustomPlayUpdate () 
	{
		// Handle visibility update for all surrounding cells




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
		float offset = shipSize - 1;
		switch(curDir) {
		case GameScript.Direction.East:
			amount = destPos.x - start.x;
			amount -= offset;
			dest.x += amount;
			break;
		case GameScript.Direction.North:
			amount = destPos.z - start.z;
			amount -= offset;
			dest.z += amount;
			break;
		case GameScript.Direction.South:
			amount = destPos.z - start.z;
			amount -= offset;
			dest.z += amount;
			break;
		case GameScript.Direction.West:
			amount = destPos.x - start.x;
			amount -= offset;
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
		// Get front cell of ship
		CellScript frontCellScript = cells[cells.Count - 1].GetComponent<CellScript>();
		int startX = frontCellScript.gridPositionX;
		int startY = frontCellScript.gridPositionY;
		// Calculate distance to movement cell
		int distance = 0;
		if (destCell.gridPositionX == startX) distance = destCell.gridPositionY - startY;
		else if (destCell.gridPositionY == startY) distance = destCell.gridPositionX - startX;
		// Verify that destination cell is within correct range
		if (distance > speed) {
			Debug.Log ("Cannot move that far");
			return;
		}
		CellScript validDestCell = gridScript.VerifyCellPath(startX, startY, distance, curDir, destCell);
		if (validDestCell != destCell) {
			// TODO: only move up until given cell
			Debug.Log ("Invalid path");
			return;
		}
		DisplayMoveRange(false);

		StartCoroutine(MoveShipForward(destCell.transform.position));

		// Update occupied cells
		// Reset currently occupied cells
		foreach (GameObject o in cells) {
			CellScript oCellScript = o.GetComponent<CellScript>();
			oCellScript.occupier = null;
			oCellScript.selected = false;
			oCellScript.available = true;
			oCellScript.curCellState = GameScript.CellState.Available;
			//oCellScript.DisplaySelection();
		}
		cells.Clear();
		// Add newly occupied cells
		switch(curDir) {
		case GameScript.Direction.East:
			for (int i = (-shipSize+1); i <= 0; i++) {
				GameObject newCell = gridScript.grid[destCell.gridPositionX + i, destCell.gridPositionY];
				CellScript newCellScript = newCell.GetComponent<CellScript>();
				newCellScript.occupier = this.gameObject;
				cells.Add (newCell);
			}
			break;
		case GameScript.Direction.North:
			for (int i = (-shipSize+1); i <= 0; i++) {
				GameObject newCell = gridScript.grid[destCell.gridPositionX, destCell.gridPositionY + i];
				CellScript newCellScript = newCell.GetComponent<CellScript>();
				newCellScript.occupier = this.gameObject;
				cells.Add (newCell);
			}
			break;
		case GameScript.Direction.South:
			for (int i = (-shipSize+1); i <= 0; i++) {
				GameObject newCell = gridScript.grid[destCell.gridPositionX, destCell.gridPositionY - i];
				CellScript newCellScript = newCell.GetComponent<CellScript>();
				newCellScript.occupier = this.gameObject;
				cells.Add (newCell);
			}
			break;
		case GameScript.Direction.West:
			for (int i = (-shipSize+1); i <= 0; i++) {
				GameObject newCell = gridScript.grid[destCell.gridPositionX - i, destCell.gridPositionY];
				CellScript newCellScript = newCell.GetComponent<CellScript>();
				newCellScript.occupier = this.gameObject;
				cells.Add (newCell);
			}
			break;
		}

		foreach (GameObject o in cells) {
			CellScript oCellScript = o.GetComponent<CellScript>();
			Debug.Log ("Index: " + cells.IndexOf(o) + " Position: " + oCellScript.gridPositionX + " " + oCellScript.gridPositionY);
			oCellScript.occupier = this.gameObject;
			oCellScript.available = false;
			oCellScript.curCellState = GameScript.CellState.Ship;
			o.GetComponent<CellScript>().selected = true;
			//o.GetComponent<CellScript>().DisplaySelection();
		}

		Debug.Log("X: "+ destCell.gridPositionX + " Y: " + destCell.gridPositionY);
		
		//rpcScript.NetworkMoveShip(shipID, destCell.gridPositionX, destCell.gridPositionY);


		// End the current turn
		gameScript.curGameState = GameScript.GameState.Wait;
	}

	/*
	 * Add damage to ship and recalculate speed.
	 */
	public void HandleHit(int section) 
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
	
	/*
	 * Rotates the ship
	 */
	public void RotateShip(bool clockwise) {
		//Calculate new turn direction
		Debug.Log("Turning " + clockwise);
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
				gridScript.GetCell(cell.gridPositionX+xsign*w, cell.gridPositionY+sign*w).renderer.material.color = Color.magenta;
			}
		}

		if (!obstacle) {
			curDir = (GameScript.Direction)newRot;
			// Reset all except rotation base cells to be unoccupied
			GameObject baseCell = cells[0];
			foreach (GameObject o in cells) {
				if (o == baseCell) continue;

				CellScript oCellScript = o.GetComponent<CellScript>();
				Debug.Log ("Resetting cell at position: " + oCellScript.gridPositionX + " " + oCellScript.gridPositionY);
				oCellScript.occupier = null;
				oCellScript.selected = false;
				oCellScript.available = true;
				oCellScript.curCellState = GameScript.CellState.Available;
			}
			cells.Clear();
			cells.Add(baseCell);

			CellScript baseCellScript = baseCell.GetComponent<CellScript>();
			// Based on direction of ship set currently occupied cells
			switch(curDir) {
			case GameScript.Direction.East:
				for (int i = 1; i < shipSize; i++) {
					GameObject newCell = gridScript.grid[baseCellScript.gridPositionX + i, baseCellScript.gridPositionY];
					CellScript newCellScript = newCell.GetComponent<CellScript>();
					newCellScript.occupier = this.gameObject;
					cells.Add (newCell);
				}
				break;
			case GameScript.Direction.North:
				for (int i = 1; i < shipSize; i++) {
					Debug.Log (baseCellScript.gridPositionX + " " + baseCellScript.gridPositionY);
					GameObject newCell = gridScript.grid[baseCellScript.gridPositionX, baseCellScript.gridPositionY + i];
					CellScript newCellScript = newCell.GetComponent<CellScript>();
					newCellScript.occupier = this.gameObject;
					cells.Add (newCell);
				}
				break;
			case GameScript.Direction.South:
				for (int i = 1; i < shipSize; i++) {
					GameObject newCell = gridScript.grid[baseCellScript.gridPositionX, baseCellScript.gridPositionY - i];
					CellScript newCellScript = newCell.GetComponent<CellScript>();
					newCellScript.occupier = this.gameObject;
					cells.Add (newCell);
				}
				break;
			case GameScript.Direction.West:
				for (int i = 1; i < shipSize; i++) {
					GameObject newCell = gridScript.grid[baseCellScript.gridPositionX - i, baseCellScript.gridPositionY];
					CellScript newCellScript = newCell.GetComponent<CellScript>();
					newCellScript.occupier = this.gameObject;
					cells.Add (newCell);
				}
				break;
			}
			
			foreach (GameObject o in cells) {
				CellScript oCellScript = o.GetComponent<CellScript>();
				oCellScript.occupier = this.gameObject;
				oCellScript.available = false;
				oCellScript.curCellState = GameScript.CellState.Ship;
				o.GetComponent<CellScript>().selected = true;
			}
		} else {
			Debug.Log ("Obstacle in rotation path");
			//display an error message
		}
	}

	// Fire cannon at targeted cell
	public void FireCannon(CellScript targetCell) {
		// Call coroutine to display fire outcome
		StartCoroutine(DisplayCannon(targetCell.gameObject));
		rpcScript.fireCannon(shipID,targetCell.gridPositionX, targetCell.gridPositionY);

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

	/** DISPLAY **/

	public void DisplayMoveRange (bool status) {
		Color setColor;
		if (status) setColor = Color.cyan;
		else setColor = Color.blue;

		// Get front cell of ship
		CellScript frontCellScript = cells[cells.Count - 1].GetComponent<CellScript>();
		int startX = frontCellScript.gridPositionX;
		int startY = frontCellScript.gridPositionY;
		switch (curDir) {
		case GameScript.Direction.East:
			for (int i = 1; i <= shipSize; i++) {
				GameObject curCell = gridScript.grid[startX + i, startY];
				CellScript curCellScript = curCell.GetComponent<CellScript>();
				if (curCellScript.curCellState == GameScript.CellState.Reef && !status) 
					curCellScript.renderer.material.color = Color.black;
				else curCellScript.renderer.material.color = setColor;
			}
			break;
		case GameScript.Direction.West:
			for (int i = 1; i <= shipSize; i++) {
				GameObject curCell = gridScript.grid[startX - i, startY];
				CellScript curCellScript = curCell.GetComponent<CellScript>();
				if (curCellScript.curCellState == GameScript.CellState.Reef && !status) 
					curCellScript.renderer.material.color = Color.black;
				else curCellScript.renderer.material.color = setColor;
			}
			break;
		case GameScript.Direction.North:
			for (int i = 1; i <= shipSize; i++) {
				GameObject curCell = gridScript.grid[startX, startY + i];
				CellScript curCellScript = curCell.GetComponent<CellScript>();
				if (curCellScript.curCellState == GameScript.CellState.Reef && !status) 
					curCellScript.renderer.material.color = Color.black;
				else curCellScript.renderer.material.color = setColor;
			}
			break;
		case GameScript.Direction.South:
			for (int i = 1; i <= shipSize; i++) {
				GameObject curCell = gridScript.grid[startX, startY - i];
				CellScript curCellScript = curCell.GetComponent<CellScript>();
				if (curCellScript.curCellState == GameScript.CellState.Reef && !status) 
					curCellScript.renderer.material.color = Color.black;
				else curCellScript.renderer.material.color = setColor;
			}
			break;
		}
	}

	// TODO: Finish this function
	public void DisplayCannonRange (bool status) {
		Color setColor;
		if (status) setColor = Color.cyan;
		else setColor = Color.blue;
		
		// Get front cell of ship
		CellScript frontCellScript = cells[cells.Count - 1].GetComponent<CellScript>();
		int startX = frontCellScript.gridPositionX;
		int startY = frontCellScript.gridPositionY;
		switch (curDir) {
		case GameScript.Direction.East:
			for (int i = 1; i <= shipSize; i++) {
				GameObject curCell = gridScript.grid[startX + i, startY];
				CellScript curCellScript = curCell.GetComponent<CellScript>();
				if (curCellScript.curCellState == GameScript.CellState.Reef) curCellScript.renderer.material.color = Color.black;
				else curCellScript.renderer.material.color = setColor;
			}
			break;
		case GameScript.Direction.West:
			for (int i = 1; i <= shipSize; i++) {
				GameObject curCell = gridScript.grid[startX - i, startY];
				CellScript curCellScript = curCell.GetComponent<CellScript>();
				if (curCellScript.curCellState == GameScript.CellState.Reef) curCellScript.renderer.material.color = Color.black;
				else curCellScript.renderer.material.color = setColor;
			}
			break;
		case GameScript.Direction.North:
			for (int i = 1; i <= shipSize; i++) {
				GameObject curCell = gridScript.grid[startX, startY + i];
				CellScript curCellScript = curCell.GetComponent<CellScript>();
				if (curCellScript.curCellState == GameScript.CellState.Reef) curCellScript.renderer.material.color = Color.black;
				else curCellScript.renderer.material.color = setColor;
			}
			break;
		case GameScript.Direction.South:
			for (int i = 1; i <= shipSize; i++) {
				GameObject curCell = gridScript.grid[startX, startY - i];
				CellScript curCellScript = curCell.GetComponent<CellScript>();
				if (curCellScript.curCellState == GameScript.CellState.Reef) curCellScript.renderer.material.color = Color.black;
				else curCellScript.renderer.material.color = setColor;
			}
			break;
		}
	}

}
