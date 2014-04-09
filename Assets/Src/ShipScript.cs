using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class ShipScript : MonoBehaviour {

	#region script references
	private GameObject system;
	public GameScript gameScript;
	public GridScript gridScript;
	protected RPCScript rpcScript;
	#endregion

	#region prefabs
	public GameObject explosion;
	public GameObject waterSplash;
	#endregion

	#region properties
	public string shipID;
	public string player;
	public GameScript.PlayerType myPlayerType;
	public GameScript.Direction curDir;
	public int shipSize;
	public bool selected = false;
	public int[] health;
	public CellScript baseCell;
	public string shipType;
	public List<CellScript> cells;
	public List<GameObject> shipSections;
	public int speed; 
	public int minesLeft = 0;
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
	protected float moveTime;
	#endregion

	#region GUI display booleans
	// Boolean values for GUI display
	public bool hasCannon = false;
	public bool hasMine = false;
	public bool hasTorpedo = false;
	public bool canRotate = false;
	#endregion
	
	#region GUI methods

	// Display movement options for selected ship
	void OnGUI () {
		shipGUI();
	}

	protected virtual void shipGUI() {
		if (gameScript.curGameState == GameScript.GameState.Wait) return;
		if (gameScript.curGameState == GameScript.GameState.SetupWaiting) return;
		if (selected == true) {
			if (!immobile) {
				if (GUI.Button(new Rect(Screen.width - 150, 10, 120, 30), "Move")) {
					gameScript.curPlayAction = GameScript.PlayAction.Move;
					// Display movement range in cells
					DisplayMoveRange(true);
				}
			}
			if (hasCannon) {
				string cannonMessage;
				if (heavyCannon) cannonMessage = "Fire Heavy Cannon";
				else cannonMessage = "Fire Cannon";
				if (GUI.Button(new Rect(Screen.width - 150, 50, 120, 30), cannonMessage)) {
					gameScript.curPlayAction = GameScript.PlayAction.Cannon;
					// Display cannon range in cells
					DisplayCannonRange(true);
				}
			}
			if (hasTorpedo) {
				if (GUI.Button(new Rect(Screen.width - 150, 90, 120, 30), "Fire Torpedo")) {
					FireTorpedo(1);
				}
			}
			if (canRotate) {
				if (GUI.Button(new Rect(Screen.width - 150, 130, 120, 30), "Rotate Clockwise")) {
					RotateShip(true,1,1);
				}
				if (GUI.Button(new Rect(Screen.width - 150, 170, 120, 30), "Rotate Counterclockwise")) {
					RotateShip(false,1,1);
				}
				if (rotSteps > 1) {
					if (GUI.Button(new Rect(Screen.width - 150, 210, 120, 30), "Rotate 180 Clockwise")) {
						RotateShip(true,1,2);
					}
					if (GUI.Button(new Rect(Screen.width - 150, 250, 120, 30), "Rotate 180 Counterclockwise")) {
						RotateShip(false,1,2);
					}
				}
			}
			if (GUI.Button(new Rect(Screen.width - 150, 290, 100, 30), "Cancel Action")) {
				gameScript.curPlayAction = GameScript.PlayAction.None;
				DisplayMoveRange(false);
				DisplayCannonRange(false);
			}
		}
	}

	#endregion

	#region gameloop methods

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

		// Retrieve prefabs from resources
		explosion = Resources.Load("explosion") as GameObject;
		waterSplash = Resources.Load("water_splash") as GameObject;

		// Add all child sections ship
		GameObject[] tShipSection = new GameObject[shipSize];
		//Correct Order of ship instantiation.
		foreach (Transform child in transform) {
			//Debug.Log(child.name);
			//Debug.Log ("Size: " + shipSize + "and arrysize: " + tShipSection.Length);
			if (child.name == "Stern") tShipSection[0] = child.gameObject;
			if (child.name == "Bow") tShipSection[shipSize-1] = child.gameObject;
			string[] mid = child.name.Split('_');
			if (mid[0] == "Mid") {
				int index = int.Parse(mid[1]);
				//Debug.Log(child.name + " : " + index);
				tShipSection[index] = child.gameObject;
			}
		}
		shipSections = new List<GameObject>(tShipSection);

		for (int i = 0; i < shipSize; i++) {
			Debug.Log(shipSections[i].transform.name + " for index: " + i);
		}

//		foreach (GameObject s in shipSections) {
//			Debug.Log(s.transform.name);
//		}

		//Debug.Log(shipSections[0].name + " : " + shipSections[1].name);
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

	#endregion
	
	#region coroutines

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
			amount += offset;
			dest.z += amount;
			break;
		case GameScript.Direction.West:
			amount = destPos.x - start.x;
			amount += offset;
			dest.x += amount;
			break;
		}
		// Time.time contains current frame time, so remember starting point
		float startTime=Time.time;
		// Perform the following until 1 second has passed
		while(Time.time-startTime <= 2) {
			// lerp from A to B in one second
			transform.position = Vector3.Lerp(start,dest,moveTime); 
			moveTime += Time.deltaTime/1; 
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
		while(Time.time-startTime <= 2) {
			// lerp from A to B in one second
			transform.position = Vector3.Lerp(start,destPos,moveTime); 
			moveTime += Time.deltaTime/1; 
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
		while(Time.time-startTime <= 2) {
			// lerp from A to B in one second
			transform.position = Vector3.Lerp(start,dest,moveTime); 
			moveTime += Time.deltaTime/1;  
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
		if (targetCellScript.curCellState == GameScript.CellState.Reef) {
			targetCellScript.renderer.material.color = Color.black;
			Instantiate(explosion, target.transform.position, Quaternion.identity);
		}
		else if (targetCellScript.curCellState == GameScript.CellState.Available) {
			Instantiate(waterSplash, target.transform.position, Quaternion.identity);
			targetCellScript.renderer.material.color = Color.blue;
		}
		else {
			Instantiate(explosion, target.transform.position, Quaternion.identity);
			targetCellScript.renderer.material.color = Color.blue;
		} 
			
	}

	#endregion

	#region section and cell methods

	// Retrive object for section of ship
	public GameObject GetSection(int section) {
		return shipSections[section];
	}
	
	public CellScript GetCellForSection(GameObject section) {
		int index = shipSections.IndexOf(section);
		return cells[index];
	}

	#endregion

	#region movement and rotation

	/*
	 * Handles movement of ship
	 * Moves ship to target cell
	 */
	public virtual void MoveShip (CellScript destCell, int local) {
		// Get front cell of ship

		if (local == 1) {
			rpcScript.NetworkMoveShip(shipID, destCell.gridPositionX, destCell.gridPositionY);
			return;
		}
		bool isMineLayer = false;
		if (this.shipType == "minelayer") { isMineLayer = true;}

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
		bool triggerMine = false;
		GameScript.CellState previousState = GameScript.CellState.Available;

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
//			bool validMove = gridScript.VerifySidewaysMove(destCell.gridPositionX, destCell.gridPositionY, shipSize, curDir,isMineLayer);
			List<CellScript> validMove = gridScript.VerifySidewaysMove(destCell.gridPositionX, destCell.gridPositionY, shipSize, curDir,isMineLayer);

			//rear most cell
			bool doNotMove = false;
			bool isMine= false;

			CellScript tCell = destCell;

			foreach (CellScript c in validMove) {
				if (!c.available) {
					//Do not move
					gameScript.GlobalNotify("Collision at (" +destCell.gridPositionX +","+destCell.gridPositionY+")"); 
					return;
				}

				if (c.isMineRadius) {
					if (!isMineLayer) {
						Debug.Log ("Not mine layer. CHecking hit");
						isMine = true;
						triggerMine = true;
						tCell = c;
					}
				}
			}
			moveTime = 0;
			StartCoroutine(MoveShipSideways(destCell.transform.position));
			destCell = tCell;


		} else if (distance < 0) {
			if (!destCell.available) {
				//Do not move
				gameScript.GlobalNotify("Collision at (" +destCell.gridPositionX +","+destCell.gridPositionY+")"); 
				return;
			}
			if (destCell.isMineRadius) {
				if (!isMineLayer) {
					triggerMine = true;
				}
			}
//			bool validMove = gridScript.VerifyCell(destCell.gridPositionX, destCell.gridPositionY);
//			if (!validMove) {
//				return;
//			}
			moveTime = 0;
			StartCoroutine(MoveShipBackward());
		} else {
			// Verify that destination cell is within correct range
			if (distance > speed) {
				Debug.Log ("Cannot move that far");
				return;
			}

			CellScript validDestCell = gridScript.VerifyCellPath(startX, startY, distance, curDir, destCell, "mine",isMineLayer);
			if (validDestCell != destCell) {
				Debug.Log ("Invalid path, moving up until collision");
				if ( !validDestCell.isMineRadius || isMineLayer) {
					CellScript newDestCell = gridScript.VerifyCellPath(startX, startY, distance, curDir, destCell, "Move",isMineLayer);
					destCell = newDestCell;
					gameScript.GlobalNotify("Collision at (" +destCell.gridPositionX +","+destCell.gridPositionY+")"); 

				} else {
					Debug.Log("Was MINEEE");
					destCell = validDestCell;
					triggerMine = true;
					previousState = destCell.curCellState;
				}


				// TODO: Potentially notify other player of reef collision? Does damage occur?
			}
			forward = true;
			moveTime = 0;
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
			//Debug.Log ("Ship Vals: " + shipX + " " + shipY);
			for (int i = 0; i < shipSize; i++) {
				//Debug.Log ("i: " + i);
				CellScript newCellScript = gridScript.grid[shipX + i, shipY];
				//Debug.Log ("Cell: " + newCellScript.gridPositionX + " " + newCellScript.gridPositionY);
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
			//Debug.Log ("Index: " + cells.IndexOf(oCellScript) + " Position: " + oCellScript.gridPositionX + " " + oCellScript.gridPositionY);
			oCellScript.occupier = this.gameObject;
			oCellScript.available = false;
			oCellScript.curCellState = GameScript.CellState.Ship;
			oCellScript.selected = true;
		}

		//Debug.Log("X: "+ destCell.gridPositionX + " Y: " + destCell.gridPositionY);
		Debug.Log ("Trigger mine value is: " + triggerMine);
		if (triggerMine) {
			if (previousState == GameScript.CellState.Mine) {
				Debug.Log("Explode");
				gridScript.Explode(destCell.gridPositionX,destCell.gridPositionY,GridScript.ExplodeType.Mine);
			}
			if (destCell.isMineRadius) {
				CellScript mine = destCell.mineParentCell;
				Debug.Log("Explode");
				gridScript.Explode(mine.gridPositionX,mine.gridPositionY,GridScript.ExplodeType.Mine);
			}
		}

		// End the current turn
//		gameScript.curGameState = GameScript.GameState.Wait;

		gameScript.EndTurn();
		//rpcScript.EndTurn();
	}

	/*
	 * Rotates the ship
	 */
	public virtual void RotateShip(bool clockwise, int local, int steps) {
		
		if (local == 1){
			rpcScript.NetworkRotateShip(shipID,clockwise, 1);
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
		bool obstacleMine = false;

		CellScript cell = cells[0];
		CellScript mineCell = null;
		// Perform check based on the orientation of the ship
		if (curDir == GameScript.Direction.North || curDir == GameScript.Direction.South) {
			int sign = 1;
			if (curDir == GameScript.Direction.North && ! clockwise ||
			    curDir == GameScript.Direction.South && clockwise) sign = -1;
			
			int ysign = 1;
			if (curDir == GameScript.Direction.South) ysign = -1;
			for (int w = 1; w < shipSize; w++) {
				if (gridScript.GetCell(cell.gridPositionX+sign*w, cell.gridPositionY+ysign*w).curCellState != GameScript.CellState.Available) {
					if (gridScript.GetCell(cell.gridPositionX+sign*w, cell.gridPositionY+ysign*w).isMineRadius ||
					    gridScript.GetCell(cell.gridPositionX+sign*w, cell.gridPositionY+ysign*w).curCellState == GameScript.CellState.Mine)
					{
						obstacleMine = true;
						mineCell = gridScript.GetCell(cell.gridPositionX+sign*w, cell.gridPositionY+ysign*w);
					}
					obstacle = true;
					break;
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
			for (int w = shipSize; w > 0; w--) {
				if (gridScript.GetCell(cell.gridPositionX+xsign*w, cell.gridPositionY+sign*w).curCellState != GameScript.CellState.Available) {
					if (gridScript.GetCell(cell.gridPositionX+xsign*w, cell.gridPositionY+sign*w).isMineRadius ||
					    gridScript.GetCell(cell.gridPositionX+xsign*w, cell.gridPositionY+sign*w).curCellState == GameScript.CellState.Mine)
					{
						obstacleMine = true;
						mineCell = gridScript.GetCell(cell.gridPositionX+xsign*w, cell.gridPositionY+sign*w);
					}
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

		// Handle damage if rotation was supposed to happen and mine occured.
		if (obstacleMine) {
			foreach (CellScript c in cells) {
				List<CellScript> neighbours = gridScript.GetCellNeighbours(c);
				foreach (CellScript nc in cells) {
					if (nc.isMineRadius || nc.curCellState == GameScript.CellState.Mine) {
						HandleDoubleHit(c, 2, c);
						break;
					}
				}
			}
			gridScript.Explode(mineCell.gridPositionX, mineCell.gridPositionY, GridScript.ExplodeType.Mine);
		}

		
		//rpcScript.EndTurn();
		SetRotation();
		gameScript.EndTurn();
	}

	/*
	 * Set rotation of ship based on direction
	 */
	public virtual void SetRotation () {
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

	#endregion

	#region handle action

	public void HandleCannon(GameObject section, int local, int damage) {
		int sectionIndex = shipSections.IndexOf(section);
		if (local==1) {
			rpcScript.fireCannonShip(shipID,sectionIndex,damage);
			return;
		}

		gameScript.NotifyDetonation("Cannon",cells[sectionIndex]);

		HandleHit(section,local,damage);
		gameScript.EndTurn();

	}

	/*
	 * Add damage to ship and recalculate speed.
	 */
	public void HandleHit(GameObject section, int local, int damage) 
	{
		Instantiate(explosion, section.transform.position, Quaternion.identity);

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
			gameScript.ships.Remove(this);
			foreach (CellScript c in cells) 
			{
				c.available = true;
				c.occupier = null;
				c.isVisible = false;
				c.curCellState = GameScript.CellState.Available;
			}
			Destroy(gameObject);
			gameScript.GlobalNotify("Ship at (" +cells[0].gridPositionX + ","+cells[0].gridPositionY+") was destroyed.");
			//Take care of stats, etc.
		} else {
			section.renderer.material.color = (health[sectionIndex] > 0 ? Color.magenta : Color.red);
			Debug.Log ("My Maxspeed: " + maxSpeed);
			Debug.Log ("My shipsize: " + shipSize);


			if (heavyArmor)
				speed = (int)((float)maxSpeed * ((float)damageTotal / (2 * (float)shipSize)));
			else
				speed = (int)((float)maxSpeed * ((float)damageTotal / ((float)shipSize)));
		}
		Debug.Log ("My speed: " + speed);

		//rpcScript.EndTurn();
//		gameScript.EndTurn();

	}

	//Handles Damage on ship given cell.
	public void HandleHit(CellScript cell, int damage) {
		int index = cells.IndexOf(cell);
		Debug.Log("Handing on index: "+index);
		HandleHit(shipSections[index],0,damage);
	}

	public void HandleDoubleHit(CellScript cell, int damage, CellScript origin) {
		int index = cells.IndexOf(cell);
		int originIndex = cells.IndexOf(origin);
		Debug.Log ("Handling mine for index: "+ index);
		//Hits front of ship, then remove front and one behind.
		if (shipSize == 1) {
			HandleHit(shipSections[index],0,damage);
		} else if (index == shipSize - 1) {
			HandleHit(shipSections[index],0,damage);
			if ((index-1 >= 0)) {
				HandleHit(shipSections[index-1],0,damage);
			}
		} else {
			HandleHit(shipSections[index],0,damage);
			if ((index + 1) < shipSize) {
				HandleHit(shipSections[index+1],0,damage);
			}
		}
	}

	public void HandleRepair(GameObject section, int local) {
		// Check that ship is in range of base
		Debug.Log ("Repair handled");

		int index = shipSections.IndexOf(section);
		CellScript cell = cells[index];
		if (cell.availableForDock) {
			for (int i = 0; i < shipSize; i++) {
				Debug.Log("At section " + i);

				if (health[i] <= 0) {
					Debug.Log("Repairing section " + i);
					rpcScript.repairShipWithIndex(shipID,i);
					break;
				}
			}
			for (int i = 0; i < shipSize; i++) {
				Debug.Log ("health of: " +i+ " is " + health[i]);

			}

            // TODO: return cell to original color value
		} else {
			Debug.Log ("This square is not available for repair");
		}

	}

	public void repairSection(int index, int local)
	{
		if (local == 1) {
			rpcScript.repairShipWithIndex(shipID,index);
			return;
		}
		int armor = 1;
		if (heavyArmor) {
			armor = 2;
		}
		health[index] = armor;
		shipSections[index].renderer.material.color = Color.yellow;
		gameScript.EndTurn();
	}

	#endregion

	#region weapons

	/*
	 * Fire cannon at targeted cell
	 */
	public void FireCannon(CellScript targetCell, int local) {
		// Call coroutine to display fire outcome
		if (local == 1) {
			rpcScript.fireCannonCell(shipID,targetCell.gridPositionX, targetCell.gridPositionY);
			return;
		}
		DisplayCannonRange(false);
		StartCoroutine(DisplayHit(targetCell.gameObject));

		if (targetCell.curCellState != GameScript.CellState.Available)
		{
			Debug.Log ("Hit Something at " +targetCell.gridPositionX + ", " + targetCell.gridPositionY);
			gameScript.NotifyDetonation("Cannon",targetCell);
			if (targetCell.curCellState == GameScript.CellState.Mine) {
				// Remove mine at this cell
				targetCell.curCellState = GameScript.CellState.Available;
				int centerX = targetCell.gridPositionX;
				int centerY = targetCell.gridPositionY;
				for (int x = (centerX > 0 ? centerX-1 : centerX); x <= centerX+1 && x < gridScript.grid.GetLength(0); x++) {
					for (int y = (centerY > 0 ? centerY-1 : centerY); y <= centerY+1 && y < gridScript.grid.GetLength(1); y++) {
						if (gridScript.grid[x,y].curCellState == GameScript.CellState.Available && gridScript.grid[x,y].curCellState != GameScript.CellState.Reef) {
							//grid[x,y].curCellState = GameScript.CellState.MineRadius;
							gridScript.grid[x,y].isMineRadius = false;
							gridScript.grid[x,y].mineParentCell = null;
						}
					}
				}
			}
		} else {
			Debug.Log ("Hit Nothing");
			gameScript.messages = "Hit Nothing";
		}

		Debug.Log("Ending turn after shootin");
		gameScript.EndTurn();
		//rpcScript.EndTurn();
	}

	/*
	 * Fire a torpedo in the current direction that the ship is facing
	 */
	public void FireTorpedo(int local) {
		// Compute distance of torpedo

		if (local == 1) {
			rpcScript.fireTorpedo(shipID);
			return;
		}

		CellScript frontCellScript = cells[cells.Count - 1];
		int startX = frontCellScript.gridPositionX;
		int startY = frontCellScript.gridPositionY;
		CellScript hitCell = gridScript.VerifyCellPath(startX, startY, 10, curDir, null, "Torpedo",true);
		if (hitCell == null) {
			gameScript.messages = "Torpedo floats off into the sea.";
		} else {
			// Handle hit on object
			if (hitCell.curCellState == GameScript.CellState.Ship) {
				ShipScript hitShip = hitCell.occupier.GetComponent<ShipScript>();
				if (curDir == GameScript.Direction.East || curDir == GameScript.Direction.West) {
					if (hitShip.curDir == GameScript.Direction.North || hitShip.curDir == GameScript.Direction.South) {
						hitShip.HandleDoubleHit(hitCell, 1, hitCell);
					} else {
						// Regular hit occured
						hitShip.HandleHit(hitCell, 1);
					}
				} else {
					if (curDir == GameScript.Direction.East || curDir == GameScript.Direction.West) {
						hitShip.HandleDoubleHit(hitCell, 1, hitCell);
					} else {
						// Regular hit occured
						hitShip.HandleHit(hitCell, 0);
					}
				}
			} else if (hitCell.curCellState == GameScript.CellState.Base) {
				BaseScript hitBase = hitCell.occupier.GetComponent<BaseScript>();
				hitBase.HandleHit(hitCell,1);
			} else if (hitCell.curCellState == GameScript.CellState.Mine) {
				gridScript.Explode(hitCell.gridPositionX, hitCell.gridPositionY, GridScript.ExplodeType.Mine);
			}
			gameScript.NotifyDetonation("torpedo", hitCell); 
			StartCoroutine(DisplayHit(hitCell.gameObject));
		}
		gameScript.EndTurn();
	}
	
	/*
	 * To make mine laying fit into the overall structure of Cellscript calling back to ShipScript,
	 * we need to have this method in every ShipScript. 
	 */
	public void LayMine(CellScript cell, int local) {
		if (local == 1) {
			rpcScript.PlaceMine(cell.gridPositionX, cell.gridPositionY);
		}
		DisplayMineRange (false);
	}

	public void DecreaseMines() {
		minesLeft --;
	}
	
	public void IncreaseMines() {
		minesLeft++;
	}

	public void PickupMine(CellScript cell, int local) {
		if (local == 1) {
			rpcScript.RemoveMine(cell.gridPositionX, cell.gridPositionY);
		}
		DisplayMineRange (false);
	}

	public virtual void Detonate (CellScript targetCell, int local) {}

	#endregion

	#region display
	/** DISPLAY **/

	public virtual void DisplayMoveRange (bool status) {

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
	
	public void DisplayMineRange(bool display) {
		foreach (CellScript cell in this.cells) {
			foreach (CellScript neighbour in gridScript.GetCellNeighbours(cell)) {
				if (neighbour.curCellState == GameScript.CellState.Available) 
					gridScript.DisplayCellForShoot(display, neighbour);
			}
		}
	}

	#endregion

	#region visibility

	// Display range of cannon
	public void UpdateRadarVisibility (bool status) {
		// Get front cell of ship
		CellScript backCellScript = cells[0];
		backCellScript.SetVisible(true);
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
					if (curCellScript.curCellState == GameScript.CellState.Mine && shipType == "minelayer") {
						curCellScript.renderer.material.color = Color.gray;
					}
				}
			}
			break;
		case GameScript.Direction.West:
			for (int x = radarRangeStart; x < radarRangeStart + radarRangeForward; x++) {
				for (int y = -sideDist; y <= sideDist; y++) {
					if (startX - x < 0 || startX - x > 29 || startY + y < 0 || startY + y > 29) continue;
					CellScript curCellScript = gridScript.grid[startX - x, startY + y];
					curCellScript.SetVisible(true);
					if (curCellScript.curCellState == GameScript.CellState.Mine && shipType == "minelayer") {
						curCellScript.renderer.material.color = Color.gray;
					}
				}
			}
			break;
		case GameScript.Direction.North:
			for (int x = -sideDist; x <= sideDist; x++) {
				for (int y = radarRangeStart; y < radarRangeStart + radarRangeForward; y++) {
					if (startX + x < 0 || startX + x > 29 || startY + y < 0 || startY + y > 29) continue;
					CellScript curCellScript = gridScript.grid[startX + x, startY + y];
					curCellScript.SetVisible(true);
					if (curCellScript.curCellState == GameScript.CellState.Mine && shipType == "minelayer") {
						curCellScript.renderer.material.color = Color.gray;
					}
				}
			}
			break;
		case GameScript.Direction.South:
			for (int x = -sideDist; x <= sideDist; x++) {
				for (int y = radarRangeStart; y < radarRangeStart + radarRangeForward; y++) {
					if (startX + x < 0 || startX + x > 29 || startY - y < 0 || startY - y > 29) continue;
					CellScript curCellScript = gridScript.grid[startX + x, startY - y];
					curCellScript.SetVisible(true);
					if (curCellScript.curCellState == GameScript.CellState.Mine && shipType == "minelayer") {
						curCellScript.renderer.material.color = Color.gray;
					}
				}
			}
			break;
		}
	}

	/**
	 * Function to update visibility of shipsections
	 */
	public void UpdateShipVisibility() {
		//Debug.Log ("Updating ship visiblity for ship " + name);
		for (int i = 0; i < cells.Count; i++) {
			//Debug.Log ("ship " + name + "cell " + i + " position is " + cells[i].gridPositionX + " " + cells[i].gridPositionY + " visibility is " + cells[i].isVisible);
			if (!cells[i].isVisible) {
				shipSections[i].renderer.enabled = false;
			} else {
				//Debug.Log ("Setting visibility for ship " + name + shipID + " to be true");
				shipSections[i].renderer.enabled = true;
			}
		}
	}

	#endregion
	
}
