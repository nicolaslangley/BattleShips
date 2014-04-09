using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class GridScript : MonoBehaviour {
	
	#region enum
	public enum ExplodeType {Mine = 0, Kamikaze = 1};
	#endregion

	#region prefabs
	public GameObject gridCell;
	public GameObject destroyer;
	public GameObject mineLayer;
	public GameObject radar;
	public GameObject cruiser;
	public GameObject torpedo;
	public GameObject kamikaze;
	public GameObject playerBase;
	#endregion

	#region properties
	public int gridSize;
	public Vector2 cellSize;
	public CellScript[,] grid;
	public List<CellScript> currentSelection;

	private GameObject system;
	private GameScript gameScript;
	private RPCScript rpcScript;

	private int refreshCounter;
	private int reefSeed;
	private Stack idSeedStack;
	private List<CellScript> reefCells;
	#endregion




	#region initialization functions

	// Use this for initialization
	public void Init () {
		// Create grid of cells

		//rpcScript.shareReefSeed(reefSeed);

		/*For testing
		CellScript[,] testCells = range (3, 4, 5, 2);

		for (int x = 0; x < 5; x ++) {
			for (int y = 0; y < 2; y++) {
				testCells[x, y].selected = true;
			}
		}*/

		// Initialize references to system and current selection
		currentSelection = new List<CellScript>();
		reefCells = new List<CellScript>();;
		refreshCounter = 0;
		system = GameObject.FindGameObjectWithTag("System");
		gameScript = system.GetComponent<GameScript>();
		rpcScript = system.GetComponent<RPCScript>();

		CreateGrid ();


		idSeedStack = new Stack();
		// CRUDE WAY OF MAKING ID. CHANGE LATER
		for (int i=0; i<50;i++)
		{
			idSeedStack.Push(i);
		}
	}

	public void setReefSeed(int seed)
	{
		reefSeed = seed;
	}

	public void refreshReef()
	{
		System.DateTime current = System.DateTime.Now;
		int hours = System.DateTime.Now.Hour;
		int mins = System.DateTime.Now.Minute;
		
		reefSeed = hours+mins+refreshCounter;

		Random.seed = reefSeed;
		Debug.Log(Random.seed);
		// Create reefs on grid

		foreach (CellScript c in reefCells) 
		{
			c.SetAvailable();
			c.SetVisible(false);
		}

		reefCells.Clear();

		for (int i = 0; i < 24; i++) {
			int xVal = Random.Range(11, 20);
			int yVal = Random.Range(3, 27);
			while(grid[xVal, yVal].curCellState == GameScript.CellState.Reef) {
				xVal = Random.Range(11, 20);
				yVal = Random.Range(3, 27);
			}
			grid[xVal, yVal].SetReef();
			grid[xVal, yVal].SetVisible(false);
			reefCells.Add(grid[xVal,yVal]);
		}
		refreshCounter++;
		
	}

	public void setShip(GameScript.ShipTypes shipType)
	{
		Debug.Log ("Set Ship called");
		if (currentSelection.Count == 1 && currentSelection.Count != 0){
			Vector3 startPos = currentSelection[0].transform.position;
			Vector3 endPos = startPos;
			if (gameScript.myPlayerType == GameScript.PlayerType.Player1) {
				endPos.x += 1;
			} else {
				endPos.x -= 1;
			}

			CellScript startCell = grid[(int)startPos.x, (int)startPos.z];
			Debug.Log ("Start Cell Pos: " + startCell.gridPositionX + " " + startCell.gridPositionY);
			Debug.Log ("End Pos: " + endPos + " Start Pos: " + startPos);
			if (startCell.availableForDock) {
				Debug.Log ("Placing ship because cell is available for dock");
				PlaceShip(startPos.x, startPos.z, endPos.x, endPos.z, 1, gameScript.myname, shipType, gameScript.myPlayerType);
				int playerNum = (int)gameScript.myPlayerType - 1;
				BaseScript myBase = gameScript.bases[playerNum];
				myBase.DisplayDockingRegion(false);
			}
			currentSelection.Clear();

		}
	}

	public ShipScript PlaceShip (float startPosX, float startPosZ, float endPosX, float endPosZ, int local, string Player, GameScript.ShipTypes types, GameScript.PlayerType playerType) {
		if (local == 1)
		{
			rpcScript.setShip(startPosX, startPosZ, endPosX, endPosZ, Player, types, playerType);
		}
		float newX = ((endPosX - startPosX)/2) + startPosX - 0.5f;
		float newZ = ((endPosZ - startPosZ)/2) + startPosZ - 0.5f;
		float newY = 0.5f;
		// Update newZ and newX based on orientation
		GameScript.Direction shipDir = GameScript.Direction.East;
		// Ship is facing either North or South
		if (endPosX - startPosX == 0) {
			if (endPosZ > startPosZ) shipDir = GameScript.Direction.North;
			else shipDir = GameScript.Direction.South;
			newX += 0.5f;
		}
			// Ship is facing either East or West
		if (endPosZ - startPosZ == 0) { 
			if (endPosX > startPosX) shipDir = GameScript.Direction.East;
			else { 
				shipDir = GameScript.Direction.West;
				newX += 1.0f;
			}
			newZ += 0.5f;
		}
		Vector3 pos = new Vector3(newX, newY, newZ);
		// Create ship
		// TODO: Place new ship at correct orientation
		GameObject newShip = null;


		switch(types) {
		case(GameScript.ShipTypes.Destroyer):
			newShip = Instantiate(destroyer, pos, Quaternion.identity) as GameObject; 
			break;
		case(GameScript.ShipTypes.Mine):
			newShip = Instantiate(mineLayer,pos,Quaternion.identity) as GameObject; 
			break;
		case(GameScript.ShipTypes.Cruiser):
			Debug.Log("Making new cruiser");
			newShip = Instantiate(cruiser,pos,Quaternion.identity) as GameObject; 
			break;
		case(GameScript.ShipTypes.Torpedo):
			newShip = Instantiate(torpedo,pos,Quaternion.identity) as GameObject; 
			break;
		case(GameScript.ShipTypes.Radar):
			newShip = Instantiate(radar,pos,Quaternion.identity) as GameObject;
			break;
		case(GameScript.ShipTypes.Kamikaze):
			newShip = Instantiate(kamikaze,pos,Quaternion.identity) as GameObject;
			break;
		default: break;
		}
	
	

		//GameObject newShip = Instantiate(destroyer, pos, Quaternion.identity) as GameObject;
		ShipScript newShipScript = newShip.GetComponent<ShipScript>();
		// Add ship to list of ships in GameScript
		gameScript.ships.Add(newShipScript);
		newShipScript.Init();
		newShipScript.curDir = shipDir;
		newShipScript.SetRotation();
		newShipScript.player = Player;
		newShipScript.myPlayerType = playerType;
		Debug.Log("Player: "+Player);
		newShipScript.shipID = Player + idSeedStack.Pop().ToString();
		Debug.Log("New Ship ID: "+ newShipScript.shipID);

		List<CellScript> shipCells = newShipScript.cells;
			
		int roundedStartPosX = (int)Mathf.RoundToInt(startPosX);
		int roundedStartPosZ = (int)Mathf.RoundToInt(startPosZ);

		switch(newShipScript.curDir) {
		case GameScript.Direction.East:
			for (int i = 0; i < newShipScript.shipSize; i++) {
				CellScript newCellScript = grid[roundedStartPosX + i, roundedStartPosZ];
				InitializeShipCell(newCellScript, newShip);
				shipCells.Add(newCellScript);
			}
			break;
		case GameScript.Direction.North:
			for (int i = 0; i < newShipScript.shipSize; i++) {
				CellScript newCellScript = grid[roundedStartPosX, roundedStartPosZ + i];
				InitializeShipCell(newCellScript, newShip);
				shipCells.Add(newCellScript);
			}
			break;
		case GameScript.Direction.South:
			for (int i = 0; i < newShipScript.shipSize; i++) {
				CellScript newCellScript = grid[roundedStartPosX, roundedStartPosZ - i];
				InitializeShipCell(newCellScript, newShip);
				shipCells.Add(newCellScript);
			}
			break;
		case GameScript.Direction.West:
			for (int i = 0; i < newShipScript.shipSize; i++) {
				CellScript newCellScript = grid[roundedStartPosX - i, roundedStartPosZ];
				InitializeShipCell(newCellScript, newShip);
				shipCells.Add(newCellScript);
			}
			break;
		}
		currentSelection.Clear();


		return newShipScript;

		
		// Reset selection of ship and cells
//			foreach (GameObject o in currentSelection) {
//				CellScript cs = o.GetComponent<CellScript>();
//				cs.selected = false;
//				cs.DisplaySelection();
//				shipCells.Add(o);
//			}
//			currentSelection.Clear();
//		}
	}


	/** HELPER METHODS **/

	private void InitializeShipCell(CellScript cell, GameObject ship) {
		cell.occupier = ship;
		cell.selected = false;
		cell.available = false;
		cell.curCellState = GameScript.CellState.Ship;
		cell.DisplaySelection();
	}

	// Initialize grid of specified size
	private void CreateGrid () {
		// Create cell objects to from grid
		grid = new CellScript[gridSize,gridSize];
		for (int i = 0; i < gridSize; i++) {
			for (int j = 0; j < gridSize; j++) {
				Vector3 cellPos = new Vector3(i * cellSize[0], 0, j * cellSize[1]);
				GameObject newCell = Instantiate(gridCell, cellPos, Quaternion.identity) as GameObject;
				CellScript newCellScript = newCell.GetComponent<CellScript>();
				newCellScript.Init();
				newCellScript.SetGridPosition(i, j);
				newCellScript.SetVisible(false);
				newCell.transform.localScale = new Vector3(cellSize[0], 0.5f, cellSize[1]);
				grid[i,j] = newCellScript;
			}
		}

		// Create bases for each player
		// Create base on grid for Player 1
		GameObject player1Base = Instantiate(playerBase, new Vector3(0,0.5f,10), Quaternion.identity) as GameObject;
		BaseScript player1BaseScript = player1Base.GetComponent<BaseScript>();
		gameScript.bases.Add(player1BaseScript);
		player1BaseScript.Init();
		player1BaseScript.myPlayerType = GameScript.PlayerType.Player1;
		Debug.Log("Player1 Playertype: " + player1BaseScript.myPlayerType);

		for (int i = 0; i < 10; i++) {
			grid[0, 10 + i].SetBase(Color.magenta);
			player1BaseScript.GetSection(i).renderer.material.color = Color.magenta;
			player1BaseScript.cells.Add(grid[0,10+i]);
		}

		// Create base on grid for Player 2
		GameObject player2Base = Instantiate(playerBase, new Vector3(29,0.5f,10), Quaternion.identity) as GameObject;
		BaseScript player2BaseScript = player2Base.GetComponent<BaseScript>();
		gameScript.bases.Add(player2BaseScript);
		player2BaseScript.Init();
		player2BaseScript.myPlayerType = GameScript.PlayerType.Player2;
		Debug.Log("Player2 Playertype: " + player2BaseScript.myPlayerType);

		for (int i = 0; i < 10; i++) {
			grid[29, 10 + i].SetBase(Color.cyan);
			player2BaseScript.GetSection(i).renderer.material.color = Color.cyan;
			player2BaseScript.cells.Add(grid[29,10+i]);
		}

		refreshReef();

	}

	#endregion



	#region cell verification functions

	/*
	 * Returns the valid destination cell within the given path
	 */
	public CellScript VerifyCellPath (int positionX, int positionY, int dist, GameScript.Direction dir, CellScript destCell, string type, bool isMineLayer) {
		bool obstacleEncountered = false;
		int offset = 0;
		if (type == "Move") offset = 1;
		CellScript encounteredObstacle = destCell;
		switch (dir) {
		case GameScript.Direction.East:
			for (int x = 1; x <= dist; x++) {
				if (positionX + x < 0 || positionX + x > 29) break;
				CellScript curCellScript = grid[positionX + x, positionY];
				// if mine then minelayer stops before, everyone else hits mineradius
				// if minelayer, minelayer goes through, everyone else stops
				// everything else stops minelayer

				if (isMineLayer) {
					if (curCellScript.available != true) {
						obstacleEncountered = true;
						encounteredObstacle = grid[positionX + (x-offset), positionY].GetComponent<CellScript>();
						break;
					}

				} else {
					if (curCellScript.available != true || curCellScript.isMineRadius) {
						obstacleEncountered = true;
						encounteredObstacle = grid[positionX + (x-offset), positionY].GetComponent<CellScript>();
						break;
					}
				}

//				if (curCellScript.available != true || curCellScript.isMineRadius) {
//						obstacleEncountered = true;
//						encounteredObstacle = grid[positionX + (x-offset), positionY].GetComponent<CellScript>();
//						break;
//					
//				}
			}
			break;
		case GameScript.Direction.West:
			for (int x = 1; x <= dist; x++) {
				if (positionX - x < 0 || positionX - x > 29) break;
				CellScript curCellScript = grid[positionX - x, positionY];

				if (isMineLayer) {
					if (curCellScript.available != true) {
						obstacleEncountered = true;
						encounteredObstacle = grid[positionX - (x-offset), positionY].GetComponent<CellScript>();
						break;
					}
					
				} else {
					if (curCellScript.available != true || curCellScript.isMineRadius) {
						obstacleEncountered = true;
						encounteredObstacle = grid[positionX - (x-offset), positionY].GetComponent<CellScript>();
						break;
					}
				}
//				if (curCellScript.available != true || curCellScript.isMineRadius) {
//					obstacleEncountered = true;
//					encounteredObstacle = grid[positionX - (x-offset), positionY].GetComponent<CellScript>();
//					break;
//				}
			}
			break;
		case GameScript.Direction.North:
			for (int y = 1; y <= dist; y++) {
				if (positionY + y < 0 || positionY + y > 29) break;
				CellScript curCellScript = grid[positionX, positionY + y];

				if (isMineLayer) {
					if (curCellScript.available != true) {
						obstacleEncountered = true;
						encounteredObstacle = grid[positionX, positionY + (y-offset)].GetComponent<CellScript>();
						break;
					}
					
				} else {
					if (curCellScript.available != true || curCellScript.isMineRadius) {
						obstacleEncountered = true;
						encounteredObstacle = grid[positionX, positionY + (y-offset)].GetComponent<CellScript>();
						break;
					}
				}
//				if (curCellScript.available != true || curCellScript.isMineRadius) {
//					obstacleEncountered = true;
//					encounteredObstacle = grid[positionX, positionY + (y-offset)].GetComponent<CellScript>();
//					break;
//				}
			}
			break;
		case GameScript.Direction.South:
			for (int y = 1; y <= dist; y++) {
				if (positionY - y < 0 || positionY - y > 29) break;
				CellScript curCellScript = grid[positionX, positionY - y];

				if (isMineLayer) {
					if (curCellScript.available != true) {
						obstacleEncountered = true;
						encounteredObstacle = grid[positionX, positionY - (y-offset)].GetComponent<CellScript>();
						break;
					}
					
				} else {
					if (curCellScript.available != true || curCellScript.isMineRadius) {
						obstacleEncountered = true;
						encounteredObstacle = grid[positionX, positionY - (y-offset)].GetComponent<CellScript>();
						break;
					}
				}
//				if (curCellScript.available != true || curCellScript.isMineRadius) {
//					obstacleEncountered = true;
//					encounteredObstacle = grid[positionX, positionY - (y-offset)].GetComponent<CellScript>();
//					break;
//				}
			}
			break;
		}

		return encounteredObstacle;
	}

	/*
	 * Returns a boolean value denoting whether side move is valid: true = valid, false = invalid
	 */
	public List<CellScript> VerifySidewaysMove(int startX, int startY, int length, GameScript.Direction dir, bool isMineLayer) {
		bool obstacleEncountered = false;
		List<CellScript> sides = new List<CellScript>();
		switch (dir) {
		case GameScript.Direction.East:
			for (int i = 0; i < length; i++) {
				CellScript curCellScript = grid[startX + i, startY];
				sides.Add(curCellScript);

//				if (isMineLayer) {
//					if (curCellScript.available != true && !curCellScript.isMineRadius) {
//						obstacleEncountered = true;
//						break;
//					}
//				} else {
//
//				
//				if (curCellScript.available != true || curCellScript.isMineRadius) {
//					obstacleEncountered = true;
//					break;
//				}
//				}
			}
			break;
		case GameScript.Direction.West:
			for (int i = 0; i < length; i++) {
				CellScript curCellScript = grid[startX - i, startY];
				sides.Add(curCellScript);

//				if (isMineLayer) {
//					if (curCellScript.available != true && !curCellScript.isMineRadius) {
//						obstacleEncountered = true;
//						break;
//					}
//				} else {
//					
//				
//				if (curCellScript.available != true || curCellScript.isMineRadius) {
//					obstacleEncountered = true;
//					break;
//				}
//				}
			}
			break;
		case GameScript.Direction.North:
			for (int i = 0; i < length; i++) {
				CellScript curCellScript = grid[startX, startY + i];
				sides.Add(curCellScript);

//				if (isMineLayer) {
//					if (curCellScript.available != true && !curCellScript.isMineRadius) {
//						
//						obstacleEncountered = true;
//						break;
//					}
//				} else {
//					
//				
//				if (curCellScript.available != true || curCellScript.isMineRadius) {
//
//					obstacleEncountered = true;
//					break;
//				}
//				}
			}
			break;
		case GameScript.Direction.South:
			for (int i = 0; i < length; i++) {
				CellScript curCellScript = grid[startX, startY - i];
				sides.Add(curCellScript);

//				if (isMineLayer) {
//
//					if (curCellScript.available != true || curCellScript.isMineRadius) {
//						
//						obstacleEncountered = true;
//						break;
//					}
//				} else {
//					if (curCellScript.available != true || curCellScript.isMineRadius) {
//						
//						obstacleEncountered = true;
//						break;
//					}
//				}
//				if (curCellScript.available != true || curCellScript.isMineRadius) {
//
//					obstacleEncountered = true;
//					break;
//				}
			}
			break;
		}
		return sides;

		//return !obstacleEncountered;

	}

	// Verify that a single cell is available
	public bool VerifyCell (int x, int y) {
		CellScript curCellScript = grid[x, y];
		return curCellScript.available;
	}

	public bool VerifyCellForRepair (int x, int y) {
		CellScript curCellScript = grid[x, y];
		return curCellScript.availableForDock;
	}

	#endregion

	#region cell retrieval methods

	public CellScript GetCell(int x, int y) {
		return grid [x, y];
	}

	// Returns the neighbours of the given cell in the grid
	public List<CellScript> GetCellNeighbours (CellScript cell) {
		List<CellScript> neighbours = new List<CellScript>();
		for (int i = 0; i < grid.GetLength(0); i++) {
			for (int j = 0; j < grid.GetLength(1); j++) {
				if (grid[i,j] == cell) {
					if (i-1 >= 0) neighbours.Add(grid[i-1, j]);
					if (j-1 >= 0) neighbours.Add(grid[i, j-1]);
					if (i < grid.GetLength(0)) neighbours.Add(grid[i+1, j]);
					if (j < grid.GetLength(1)) neighbours.Add(grid[i, j+1]);
				}
			}
		}
		return neighbours;
	}
	
	/*
	 * Returns the cells in a given rectangle on the grid, for the purposes of moving,
	 * torpedo intersecting, and radar searching. 
	 */
	public CellScript[,] GetCellsInRange (int positionX, int positionY, int dx, int dy) {
		CellScript[,] cells = new CellScript[dx, dy];
		for (int x = 0; x < dx; x++) {
			for (int y = 0; y < dy; y++) {
				cells[x, y] = grid[positionX+x, positionY+y];
			}
		}
		return cells;
	}
	#endregion

	#region display cells for action
	
	// Display cell as being available for movement based on status 
	public void DisplayCellForMove(bool status, int x, int y) {
		if (x < 0 || x > 29 || y < 0 || y > 29) return;
		Color setColor;
		if (status) setColor = Color.cyan;
		else setColor = Color.blue;
		CellScript cellScript = grid[x, y];
		if (status) cellScript.availableForMove = true;
		else cellScript.availableForMove = false;
		if (cellScript.curCellState == GameScript.CellState.Reef && !status) 
			cellScript.renderer.material.color = Color.black;
		else cellScript.renderer.material.color = setColor;
	}

	// Display cell as being available to shoot based on status 
	public void DisplayCellForShoot(bool status, int x, int y) {
		if (x < 0 || x > 29 || y < 0 || y > 29) return;
		Color setColor;
		if (status) setColor = Color.red;
		else setColor = Color.blue;
		CellScript cellScript = grid[x, y];
		if (status) cellScript.availableForShoot = true;
		else cellScript.availableForShoot = false;
		if (cellScript.curCellState == GameScript.CellState.Reef && !status) 
			cellScript.renderer.material.color = Color.black;
		else cellScript.renderer.material.color = setColor;
	}

	public void DisplayCellForShoot(bool status, CellScript cellScript) {
		Color setColor;
		if (status) setColor = Color.red;
		else setColor = Color.blue;
		if (status) cellScript.availableForShoot = true;
		else cellScript.availableForShoot = false;
		if (cellScript.curCellState == GameScript.CellState.Reef && !status) 
			cellScript.renderer.material.color = Color.black;
		else cellScript.renderer.material.color = setColor;
	}

	public void DisplayCellForDock(bool status, int x, int y) {
		if (x < 0 || x > 29 || y < 0 || y > 29) return;
		Color setColor;
		if (status) setColor = Color.green;
		else setColor = Color.blue;
		CellScript cellScript = grid[x, y];
		if (status) cellScript.availableForDock = true;
		else cellScript.availableForDock = false;
		if (!cellScript.selected)cellScript.renderer.material.color = setColor;
	}

	#endregion

	#region explosion and mine functions

	public void Explode(int centerX, int centerY, ExplodeType type) {
		Debug.Log("explode with " + centerX);
		//Tell cells around it that it was hit by explosion

		for (int x = (centerX > 0 ? centerX-1 : centerX); x <= centerX+1 && x < this.grid.GetLength(0); x++) {
			for (int y = (centerY > 0 ? centerY-1 : centerY); y <= centerY+1 && y < this.grid.GetLength(1); y++) {
				GetCell(x,y).handleCellDamage(2,type,GetCell(centerX,centerY));
				if (type == ExplodeType.Mine) {
					CellScript curCell = GetCell(x,y);
					if (curCell.curCellState == GameScript.CellState.Mine || curCell.isMineRadius) {
						curCell.curCellState = GameScript.CellState.Available;
						curCell.isMineRadius = false;
						curCell.mineParentCell = null;
					}
				}
			}
		}
	}

	public void PlaceMine(int centerX, int centerY) {
		Debug.Log("Place mine at: " + centerX + " " + centerY);
		CellScript cell = grid[centerX, centerY];
		cell.curCellState = GameScript.CellState.Mine;
		cell.isMineRadius = true;
		for (int x = (centerX > 0 ? centerX-1 : centerX); x <= centerX+1 && x < this.grid.GetLength(0); x++) {
			for (int y = (centerY > 0 ? centerY-1 : centerY); y <= centerY+1 && y < this.grid.GetLength(1); y++) {
				if (grid[x,y].curCellState == GameScript.CellState.Available && grid[x,y].curCellState != GameScript.CellState.Reef) {
					//grid[x,y].curCellState = GameScript.CellState.MineRadius;
					grid[x,y].isMineRadius = true;
					grid[x,y].mineParentCell = cell;
				}
			}
		}
		cell.available = false;
		gameScript.EndTurn();
	}

	public void RemoveMine(int centerX, int centerY) {
		CellScript cell = grid[centerX, centerY];
		cell.curCellState = GameScript.CellState.Available;
		cell.isMineRadius = false;
		for (int x = (centerX > 0 ? centerX-1 : centerX); x <= centerX+1 && x < this.grid.GetLength(0); x++) {
			for (int y = (centerY > 0 ? centerY-1 : centerY); y <= centerY+1 && y < this.grid.GetLength(1); y++) {
				if (grid[x,y].curCellState == GameScript.CellState.MineRadius && grid[x,y].curCellState != GameScript.CellState.Reef) {
					//grid[x,y].curCellState = GameScript.CellState.MineRadius;
					grid[x,y].isMineRadius = false;
					grid[x,y].mineParentCell = null;
				}
			}
		}
		cell.available = true;
		gameScript.EndTurn();
	}

	#endregion

	#region selection functions
	// Adds given cell to current selection - returns FALSE if not a valid selection
	public bool AddToSelection (CellScript cell) {
		bool valid = false;
		if (currentSelection.Count > 0) {
			List<CellScript> neighbours = GetCellNeighbours(cell);
			foreach (CellScript oCellScript in currentSelection) {
				if (neighbours.Contains(oCellScript)) {
					valid = true;
				}
			}
		} else {
			valid = true;
		}
		if (valid == true) {
			currentSelection.Add(cell);
			return true;
		} else {
			return false;
		}
	}

	// Removes given cell from current selection - if cell is not in selection, does nothing
	public void RemoveFromSelection (CellScript cell) {
		if (currentSelection.Contains(cell)) currentSelection.Remove(cell);
	}

	#endregion

	#region reset functions

	// Reset the visibility of all cells to be false
	public void ResetVisibility() {
		for (int x = 0; x < 30; x++) {
			for (int y = 0; y < 30; y++) {
				grid[x, y].SetVisible(false);
			}
		}
	}

	/**
	 * Resets cell selection to be false
	 */
	public void ResetSelection() {
		for (int x = 0; x < 30; x++) {
			for (int y = 0; y < 30; y++) {
				grid[x, y].selected = false;
			}
		}
	}

	#endregion

}
