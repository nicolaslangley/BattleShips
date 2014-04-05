using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class ShipScript : MonoBehaviour {

	/** Properties **/
	public string shipID;
	public string player;
	//cells are ordered from first cell added when creating to last one
	public List<CellScript> cells;
	public GameScript.Direction curDir;
	public int shipSize;
	public GameScript gameScript;
	public GridScript gridScript;
	public bool selected = false;
	
	private List<GameObject> shipSections;
	
	private GameObject system;

	protected RPCScript rpcScript;
	public int[] health;
	public CellScript baseCell;
	
	public int speed; 
	protected bool immobile = false;
	protected int maxSpeed; //Speed at full health
	protected int rotSteps = 1; // increments of 90 degrees. Most ships have 1, Torpedo Boats have 2
	// Values to determine range of cannon and radar
	protected int cannonRangeForward = 4;
	protected int cannonRangeSide = 4;
	protected int cannonRangeStart = -1;
	protected int radarRangeForward = 6;
	protected int radarRangeSide = 5;
	protected int radarRangeStart = -2;

	public bool heavyCannon;
	protected bool heavyArmor;

	public string shipType;

	/** UNITY METHODS **/

	// Display movement options for selected ship
	void OnGUI () {
		if (gameScript.curGameState == GameScript.GameState.Wait) return;
		if (selected == true) {
			if (!immobile) {
				if (GUI.Button(new Rect(Screen.width - 110, 10, 100, 30), "Move")) {
					gameScript.curPlayAction = GameScript.PlayAction.Move;
					// Display movement range in cells
					DisplayMoveRange(true);
				}
			}
			if (GUI.Button(new Rect(Screen.width - 110, 50, 100, 30), "Fire Cannon")) {
				gameScript.curPlayAction = GameScript.PlayAction.Cannon;
				// Display cannon range in cells
				DisplayCannonRange(true);
			}
			if (GUI.Button(new Rect(Screen.width - 110, 90, 100, 30), "Fire Torpedo")) {
				FireTorpedo();
			}
			if (GUI.Button(new Rect(Screen.width - 110, 130, 100, 30), "Rotate Clockwise")) {
				RotateShip(true,1);
			}
			if (GUI.Button(new Rect(Screen.width - 110, 170, 100, 30), "Rotate Counterclockwise")) {
				RotateShip(false,1);
			}
			if (GUI.Button(new Rect(Screen.width - 110, 230, 100, 30), "Cancel Action")) {
				gameScript.curPlayAction = GameScript.PlayAction.None;
				DisplayMoveRange(false);
				DisplayCannonRange(false);
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
		speed = maxSpeed;
		health = new int[shipSize];
		InitArmor ();
		// Add all child sections ship
		shipSections = new List<GameObject>();
		//Correct Order of ship instantiation.
		foreach (Transform child in transform) {
			Debug.Log(child.name);
			shipSections.Add(child.gameObject);
		}
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

	public void CustomPlayUpdate () {
		// Handle visibility update for all surrounding cells

		if (player == gameScript.myname)
		{
			UpdateRadarVisibility(true);
			if (gameScript.curPlayAction == GameScript.PlayAction.Move) {
				
			}
		}
		SetRotation();
	}

	/** COROUTINES **/

	// Coroutine for forward movement
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
			amount -= offset - shipSize;
			dest.z += amount;
			break;
		case GameScript.Direction.West:
			amount = destPos.x - start.x;
			amount -= offset - shipSize;
			dest.x += amount;
			break;
		}
		// Time.time contains current frame time, so remember starting point
		float startTime=Time.time;
		// Perform the following until 1 second has passed
		while(Time.time-startTime <= 1) {
			// lerp from A to B in one second
			transform.position = Vector3.Lerp(start,dest,Time.time-startTime); 
			// Wait for next frame
			yield return 1; 
		}
	}

	IEnumerator MoveShipSideways (Vector3 destPos) {
		destPos.y += 0.5f;
		Vector3 start = transform.position;
		// Time.time contains current frame time, so remember starting point
		float startTime=Time.time;
		// Perform the following until 1 second has passed
		while(Time.time-startTime <= 1) {
			// lerp from A to B in one second
			transform.position = Vector3.Lerp(start,destPos,Time.time-startTime); 
			// Wait for next frame
			yield return 1; 
		}
	}

	// Move ship backwards
	IEnumerator MoveShipBackward () {
		Vector3 start = transform.position;
		Vector3 dest = transform.position;
		int amount = 0;
		switch(curDir) {
		case GameScript.Direction.East:
			amount = -1;
			dest.x += amount;
			break;
		case GameScript.Direction.North:
			amount = -1;
			dest.z += amount;
			break;
		case GameScript.Direction.South:
			amount = 1;
			dest.z += amount;
			break;
		case GameScript.Direction.West:
			amount = 1;
			dest.x += amount;
			break;
		}
		// Time.time contains current frame time, so remember starting point
		float startTime=Time.time;
		// Perform the following until 1 second has passed
		while(Time.time-startTime <= 1) {
			// lerp from A to B in one second
			transform.position = Vector3.Lerp(start,dest,Time.time-startTime); 
			// Wait for next frame
			yield return 1; 
		}
	}

	// Display the effects of shooting the cannon over a period of time
	IEnumerator DisplayHit (GameObject target) {
		float startTime=Time.time; // Time.time contains current frame time, so remember starting point
		while(Time.time-startTime <= 0.3){ // until one second passed
			target.renderer.material.color = Color.white; // lerp from A to B in one second
			yield return 1; // wait for next frame
		}

		CellScript targetCellScript = target.GetComponent<CellScript>();
		if (targetCellScript.curCellState == GameScript.CellState.Reef) targetCellScript.renderer.material.color = Color.black;
		else targetCellScript.renderer.material.color = Color.blue;
	}

	/** HELPER METHODS **/

	// Retrive object for section of ship
	public GameObject GetSection(int section) {
		return shipSections[section];
	}

	/*
	 * Handles movement of ship
	 * Moves ship to target cell
	 */
	public void MoveShip (CellScript destCell, int local) {
		// Get front cell of ship

		if (local == 1) {
			rpcScript.NetworkMoveShip(shipID, destCell.gridPositionX, destCell.gridPositionY);
			return;
		}

		Debug.Log("cell count: "+ cells.Count.ToString());
		CellScript frontCellScript = cells[cells.Count - 1];
		int startX = frontCellScript.gridPositionX;
		int startY = frontCellScript.gridPositionY;
		CellScript backCellScript = cells[0];
		int backX = backCellScript.gridPositionX;
		int backY = backCellScript.gridPositionY;
		// Calculate distance to movement cell
		int distance = 0;
		bool forward = false;

		if (curDir == GameScript.Direction.East || curDir == GameScript.Direction.West) {
			if (destCell.gridPositionY < startY) destCell = gridScript.GetCell(backX, backY - 1);
			else if (destCell.gridPositionY > startY) destCell = gridScript.GetCell(backX, backY + 1);
			else {

				distance = destCell.gridPositionX - startX;
			} 
		} else {
			if (destCell.gridPositionX < startX) destCell = gridScript.GetCell(backX - 1, backY);
			else if (destCell.gridPositionX > startX) destCell = gridScript.GetCell(backX + 1, backY);
			else distance = destCell.gridPositionY - startY;
		}

		if (curDir == GameScript.Direction.West || curDir == GameScript.Direction.South) {
			distance = -distance;
		}

		// Distance will be 0 only if the move is sideways
		if (distance == 0) {
			bool validMove = gridScript.VerifySidewaysMove(destCell.gridPositionX, destCell.gridPositionY, shipSize, curDir);
			if (!validMove) {
				return;
			}
			StartCoroutine(MoveShipSideways(destCell.transform.position));
		} else if (distance < 0) {
			bool validMove = gridScript.VerifyCell(destCell.gridPositionX, destCell.gridPositionY);
			if (!validMove) {
				return;
			}
			StartCoroutine(MoveShipBackward());
		} else {
			// Verify that destination cell is within correct range
			if (distance > speed) {
				Debug.Log ("Cannot move that far");
				return;
			}
		
			CellScript validDestCell = gridScript.VerifyCellPath(startX, startY, distance, curDir, destCell, "Move");
			if (validDestCell != destCell) {
				Debug.Log ("Invalid path, moving up until collision");
				// TODO: Potentially notify other player of reef collision? Does damage occur?
				destCell = validDestCell;
			}
			forward = true;
			StartCoroutine(MoveShipForward(destCell.transform.position));
		}
		DisplayMoveRange(false);

		// Update occupied cells
		// Reset currently occupied cells
		foreach (CellScript oCellScript in cells) {
			oCellScript.occupier = null;
			oCellScript.selected = false;
			oCellScript.available = true;
			oCellScript.curCellState = GameScript.CellState.Available;
			//oCellScript.DisplaySelection();
		}
		cells.Clear();

		// Get new cell that ship is on
		CellScript shipCell = destCell;
		int shipX = shipCell.gridPositionX;
		int shipY = shipCell.gridPositionY;
		Debug.Log ("Ship Vals: " + shipX + " " + shipY);
		// Add newly occupied cells
		switch(curDir) {
		case GameScript.Direction.East:
			if (forward) shipX -= shipSize-1;
			Debug.Log ("Ship Vals: " + shipX + " " + shipY);
			for (int i = 0; i < shipSize; i++) {
				Debug.Log ("i: " + i);
				CellScript newCellScript = gridScript.grid[shipX + i, shipY];
				Debug.Log ("Cell: " + newCellScript.gridPositionX + " " + newCellScript.gridPositionY);
				newCellScript.occupier = this.gameObject;
				cells.Add(newCellScript);
			}
			break;
		case GameScript.Direction.North:
			if (forward) shipY -= shipSize-1;
			Debug.Log ("Ship Vals: " + shipX + " " + shipY);
			for (int i = 0; i < shipSize; i++) {
				Debug.Log ("i: " + i);
				CellScript newCellScript = gridScript.grid[shipX, shipY + i];
				Debug.Log ("Cell: " + newCellScript.gridPositionX + " " + newCellScript.gridPositionY);
				newCellScript.occupier = this.gameObject;
				cells.Add(newCellScript);
			}
			break;
		case GameScript.Direction.South:
			if (forward) shipY += shipSize-1;
			Debug.Log ("Ship Vals: " + shipX + " " + shipY);
			for (int i = 0; i < shipSize; i++) {
				Debug.Log ("i: " + i);
				CellScript newCellScript = gridScript.grid[shipX, shipY - i];
				Debug.Log ("Cell: " + newCellScript.gridPositionX + " " + newCellScript.gridPositionY);
				newCellScript.occupier = this.gameObject;
				cells.Add(newCellScript);
			}
			break;
		case GameScript.Direction.West:
			if (forward) shipX += shipSize-1;
			Debug.Log ("Ship Vals: " + shipX + " " + shipY);
			for (int i = 0; i < shipSize; i++) {
				Debug.Log ("i: " + i);
				CellScript newCellScript = gridScript.grid[shipX - i, shipY];
				Debug.Log ("Cell: " + newCellScript.gridPositionX + " " + newCellScript.gridPositionY);
				newCellScript.occupier = this.gameObject;
				cells.Add(newCellScript);
			}
			break;
		}

		foreach (CellScript oCellScript in cells) {
			Debug.Log ("Index: " + cells.IndexOf(oCellScript) + " Position: " + oCellScript.gridPositionX + " " + oCellScript.gridPositionY);
			oCellScript.occupier = this.gameObject;
			oCellScript.available = false;
			oCellScript.curCellState = GameScript.CellState.Ship;
			oCellScript.selected = true;
		}

		Debug.Log("X: "+ destCell.gridPositionX + " Y: " + destCell.gridPositionY);




		// End the current turn
//		gameScript.curGameState = GameScript.GameState.Wait;

		gameScript.EndTurn();
		//rpcScript.EndTurn();
	}

	/*
	 * Add damage to ship and recalculate speed.
	 */
	public void HandleHit(GameObject section, int local, int damage) 
	{
		int sectionIndex = shipSections.IndexOf(section);
		if (local == 1)
		{
			Debug.Log("Local");
			rpcScript.fireCannonShip(shipID,sectionIndex, damage);
			return;
		}
		DisplayCannonRange(false);
		Debug.Log ("Hit handled on section: " + sectionIndex);
		health [sectionIndex] -= damage;
		if (health [sectionIndex] < 0) { health[sectionIndex]=0;}
		int damageTotal = 0;
		for (int i = 0; i < shipSize; i++) {
			Debug.Log ("health" + health[i]);
			damageTotal += health[i];
		}
		Debug.Log ("Damage total is: " + damageTotal);
		if (damageTotal == 0) {
			Destroy(gameObject);
			//Take care of stats, etc.
		} else {
			section.renderer.material.color = Color.red;
			if (heavyArmor)
				speed = maxSpeed * (damageTotal / (2 * shipSize));
			else
				speed = maxSpeed * (damageTotal / shipSize);
		}

		//rpcScript.EndTurn();
//		gameScript.EndTurn();

	}

	//Handles Damage on ship given cell.
	public void HandleHit(CellScript cell, int damage) {
		int index = cells.IndexOf(cell);
		Debug.Log("Handing on index: "+index);
		HandleHit(shipSections[index],0,damage);
	}

	public void HandleMine(CellScript cell, int damage, CellScript origin) {
		int index = cells.IndexOf(origin);
		int originIndex = cells.IndexOf(origin);
		Debug.Log ("Handling mine for index: "+ index);
		//Hits front of ship, then remove front and one behind.
		if (shipSize == 1) {
			HandleHit(shipSections[index],0,damage);
		} else if (index == shipSize - 1) {
			HandleHit(shipSections[index],0,damage);
			HandleHit(shipSections[index-1],0,damage);
		} else {
			HandleHit(shipSections[index],0,damage);
			HandleHit(shipSections[index+1],0,damage);
		}
	}

	public void HandleRepair(GameObject section, int local) {
		// Check that ship is in range of base
		Debug.Log ("Repair handled");

		int index = shipSections.IndexOf(section);

		if (local == 1)
		{
			rpcScript.handleShipRepair(shipID,index);
			return;
		}
		CellScript cell = cells[index];
		if (cell.availableForRepair) {
			health[index] += 1;
			section.renderer.material.color = Color.yellow;
            // TODO: return cell to original color value
		}
	}

	/*
	 * Rotates the ship
	 */
	public void RotateShip(bool clockwise, int local) {

		if (local == 1){
			rpcScript.NetworkRotateShip(shipID,clockwise);
			return;
		}

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
		CellScript cell = cells[0];
		// Perform check based on the orientation of the ship
		if (curDir == GameScript.Direction.North || curDir == GameScript.Direction.South) {
			int sign = 1;
			if (curDir == GameScript.Direction.North && ! clockwise ||
			    curDir == GameScript.Direction.South && clockwise) sign = -1;

			int ysign = 1;
			if (curDir == GameScript.Direction.South) ysign = -1;
			for (int w = 1; w < shipSize; w++) {
				if (gridScript.GetCell(cell.gridPositionX+sign*w, cell.gridPositionY+ysign*w).curCellState != GameScript.CellState.Available) {
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
				if (gridScript.GetCell(cell.gridPositionX+xsign*w, cell.gridPositionY+sign*w).curCellState != GameScript.CellState.Available) {
					obstacle = true;
					break;
				}
				//For debugging
				//gridScript.GetCell(cell.gridPositionX+xsign*w, cell.gridPositionY+sign*w).renderer.material.color = Color.magenta;
			}
		}

		if (!obstacle) {
			curDir = (GameScript.Direction)newRot;
			// Reset all except rotation base cells to be unoccupied
			CellScript baseCellScript = cells[0];
			foreach (CellScript oCellScript in cells) {
				if (oCellScript == baseCellScript) continue;

				Debug.Log ("Resetting cell at position: " + oCellScript.gridPositionX + " " + oCellScript.gridPositionY);
				oCellScript.occupier = null;
				oCellScript.selected = false;
				oCellScript.available = true;
				oCellScript.curCellState = GameScript.CellState.Available;
			}
			cells.Clear();
			cells.Add(baseCellScript);
	
			// Based on direction of ship set currently occupied cells
			switch(curDir) {
			case GameScript.Direction.East:
				for (int i = 1; i < shipSize; i++) {
					CellScript newCellScript = gridScript.grid[baseCellScript.gridPositionX + i, baseCellScript.gridPositionY];
					newCellScript.occupier = this.gameObject;
					cells.Add(newCellScript);
				}
				break;
			case GameScript.Direction.North:
				for (int i = 1; i < shipSize; i++) {
					Debug.Log (baseCellScript.gridPositionX + " " + baseCellScript.gridPositionY);
					CellScript newCellScript = gridScript.grid[baseCellScript.gridPositionX, baseCellScript.gridPositionY + i];
					newCellScript.occupier = this.gameObject;
					cells.Add(newCellScript);
				}
				break;
			case GameScript.Direction.South:
				for (int i = 1; i < shipSize; i++) {
					CellScript newCellScript = gridScript.grid[baseCellScript.gridPositionX, baseCellScript.gridPositionY - i];
					newCellScript.occupier = this.gameObject;
					cells.Add(newCellScript);
				}
				break;
			case GameScript.Direction.West:
				for (int i = 1; i < shipSize; i++) {
					CellScript newCellScript = gridScript.grid[baseCellScript.gridPositionX - i, baseCellScript.gridPositionY];
					newCellScript.occupier = this.gameObject;
					cells.Add(newCellScript);
				}
				break;
			}

			foreach (CellScript oCellScript in cells) {
				oCellScript.occupier = this.gameObject;
				oCellScript.available = false;
				oCellScript.curCellState = GameScript.CellState.Ship;
				oCellScript.selected = true;
			}
		} else {
			Debug.Log ("Obstacle in rotation path");
			//display an error message
		}

		//rpcScript.EndTurn();
		SetRotation();
		gameScript.EndTurn();
	}

	/*
	 * Fire cannon at targeted cell
	 */
	public void FireCannon(CellScript targetCell) {
		// Call coroutine to display fire outcome
		DisplayCannonRange(false);
		StartCoroutine(DisplayHit(targetCell.gameObject));
		rpcScript.fireCannonCell(shipID,targetCell.gridPositionX, targetCell.gridPositionY);
		rpcScript.EndTurn();
	}

	/*
	 * Fire a torpedo in the current direction that the ship is facing
	 */
	public void FireTorpedo() {
		// Compute distance of torpedo
		CellScript frontCellScript = cells[cells.Count - 1];
		int startX = frontCellScript.gridPositionX;
		int startY = frontCellScript.gridPositionY;
		CellScript hitCell = gridScript.VerifyCellPath(startX, startY, 30, curDir, null, "Torpedo");
		if (hitCell == null) {
			// Nothing was hit by the torpedo
			Debug.Log ("Nothing was hit by the torpedo");
		} else {
			// Handle hit on object
			if (hitCell.curCellState == GameScript.CellState.Ship) {
				Debug.Log("Hit a ship");
				ShipScript hitShip = hitCell.occupier.GetComponent<ShipScript>();
				hitShip.HandleHit(hitCell, 1);
			} else if (hitCell.curCellState == GameScript.CellState.Base) {
				Debug.Log("Hit a base");
				BaseScript hitBase = hitCell.occupier.GetComponent<BaseScript>();
			}
			StartCoroutine(DisplayHit(hitCell.gameObject));
		}
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

		// Display movement options in forward and backwards direction
		CellScript frontCellScript = cells[cells.Count - 1];
		int startXFront = frontCellScript.gridPositionX;
		int startYFront = frontCellScript.gridPositionY;
		CellScript backCellScript = cells[0];
		int backX = backCellScript.gridPositionX;
		int backY = backCellScript.gridPositionY;

		// Display cell options based on direction that ship is facing
		switch (curDir) {
		case GameScript.Direction.East:
			for (int i = 0; i <= speed; i++) {
				gridScript.DisplayCellForMove(status, startXFront + i, startYFront);
			}
			gridScript.DisplayCellForMove(status, backX - 1, backY);
			break;
		case GameScript.Direction.West:
			for (int i = 0; i <= speed; i++) {
				gridScript.DisplayCellForMove(status, startXFront - i, startYFront);
			}
			gridScript.DisplayCellForMove(status, backX + 1, backY);
			break;
		case GameScript.Direction.North:
			for (int i = 0; i <= speed; i++) {
				gridScript.DisplayCellForMove(status, startXFront, startYFront + i);
			}
			gridScript.DisplayCellForMove(status, backX, backY - 1);
			break;
		case GameScript.Direction.South:
			for (int i = 0; i <= speed; i++) {
				gridScript.DisplayCellForMove(status, startXFront, startYFront - i);
			}
			gridScript.DisplayCellForMove(status, backX, backY + 1);
			break;
		}

		// Display movement options to the side
		if (curDir == GameScript.Direction.North || curDir == GameScript.Direction.South) {
			foreach (CellScript cell in cells) {
				gridScript.DisplayCellForMove(status, cell.gridPositionX + 1, cell.gridPositionY);
				gridScript.DisplayCellForMove(status, cell.gridPositionX - 1, cell.gridPositionY);
			}
		} else {
			foreach (CellScript cell in cells) {
				gridScript.DisplayCellForMove(status, cell.gridPositionX, cell.gridPositionY + 1);
				gridScript.DisplayCellForMove(status, cell.gridPositionX, cell.gridPositionY - 1);
			}
		}

	}

	// Display range of cannon
	public void DisplayCannonRange (bool status) {
		Color setColor;
		if (status) setColor = Color.red;
		else setColor = Color.blue;
		
		// Get front cell of ship
		CellScript backCellScript = cells[0];
		int startX = backCellScript.gridPositionX;
		int startY = backCellScript.gridPositionY;
		int sideDist = cannonRangeSide / 2;
		switch (curDir) {
		case GameScript.Direction.East:
			for (int x = cannonRangeStart; x < cannonRangeStart + cannonRangeForward; x++) {
				for (int y = -sideDist; y <= sideDist; y++) {
					gridScript.DisplayCellForShoot(status, startX + x, startY + y);
				}
			}
			break;
		case GameScript.Direction.West:
			for (int x = cannonRangeStart; x < cannonRangeStart + cannonRangeForward; x++) {
				for (int y = -sideDist; y <= sideDist; y++) {
					gridScript.DisplayCellForShoot(status, startX - x, startY + y);
				}
			}
			break;
		case GameScript.Direction.North:
			for (int x = -sideDist; x <= sideDist; x++) {
				for (int y = cannonRangeStart; y < cannonRangeStart + cannonRangeForward; y++) {
					gridScript.DisplayCellForShoot(status, startX + x, startY + y);
				}
			}
			break;
		case GameScript.Direction.South:
			for (int x = -sideDist; x <= sideDist; x++) {
				for (int y = cannonRangeStart; y < cannonRangeStart + cannonRangeForward; y++) {
					gridScript.DisplayCellForShoot(status, startX + x, startY - y);
				}
			}
			break;
		}
	}

	// Display range of cannon
	public void UpdateRadarVisibility (bool status) {
		// Get front cell of ship
		CellScript backCellScript = cells[0];
		int startX = backCellScript.gridPositionX;
		int startY = backCellScript.gridPositionY;
		int sideDist = radarRangeSide / 2;
		switch (curDir) {
		case GameScript.Direction.East:
			for (int x = radarRangeStart; x < radarRangeStart + radarRangeForward; x++) {
				for (int y = -sideDist; y <= sideDist; y++) {
					if (startX + x < 0 || startX + x > 29 || startY + y < 0 || startY + y > 29) continue;
					CellScript curCellScript = gridScript.grid[startX + x, startY + y];
					curCellScript.SetVisible(true);
				}
			}
			break;
		case GameScript.Direction.West:
			for (int x = radarRangeStart; x < radarRangeStart + radarRangeForward; x++) {
				for (int y = -sideDist; y <= sideDist; y++) {
					if (startX + x < 0 || startX + x > 29 || startY + y < 0 || startY + y > 29) continue;
					CellScript curCellScript = gridScript.grid[startX - x, startY + y];
					curCellScript.SetVisible(true);
				}
			}
			break;
		case GameScript.Direction.North:
			for (int x = -sideDist; x <= sideDist; x++) {
				for (int y = radarRangeStart; y < radarRangeStart + radarRangeForward; y++) {
					if (startX + x < 0 || startX + x > 29 || startY + y < 0 || startY + y > 29) continue;
					CellScript curCellScript = gridScript.grid[startX + x, startY + y];
					curCellScript.SetVisible(true);
				}
			}
			break;
		case GameScript.Direction.South:
			for (int x = -sideDist; x <= sideDist; x++) {
				for (int y = radarRangeStart; y < radarRangeStart + radarRangeForward; y++) {
					if (startX + x < 0 || startX + x > 29 || startY + y < 0 || startY + y > 29) continue;
					CellScript curCellScript = gridScript.grid[startX + x, startY - y];
					curCellScript.SetVisible(true);
				}
			}
			break;
		}
	}



	/*
	 * To make mine laying fit into the overall structure of Cellscript calling back to ShipScript,
	 * we need to have this method in every ShipScript. 
	 */
	public virtual void LayMine(CellScript cell) {
		Debug.LogWarning ("Attempted to drop a mine from a ship that is not a MineLayer. Something is horribly wrong.");
	}

}
