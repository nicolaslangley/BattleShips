﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class GridScript : MonoBehaviour {

	/** PROPERTIES **/

	//Prefabs
	public GameObject gridCell;
	public GameObject destroyer;
	
	public int gridSize;
	public Vector2 cellSize;
	public GameObject[,] grid;
	public List<GameObject> currentSelection;

	private GameObject system;
	private GameScript gameScript;
	private RPCScript rpcScript;


	private int reefSeed;
	private Stack idSeedStack;

	/** GAMELOOP METHODS **/

	// Use this for initialization
	public void Init () {
		// Create grid of cells
		reefSeed = 42;
		//rpcScript.shareReefSeed(reefSeed);
		CreateGrid ();

		/*For testing
		CellScript[,] testCells = range (3, 4, 5, 2);

		for (int x = 0; x < 5; x ++) {
			for (int y = 0; y < 2; y++) {
				testCells[x, y].selected = true;
			}
		}*/

		// Initialize references to system and current selection
		currentSelection = new List<GameObject>();
		system = GameObject.FindGameObjectWithTag("System");
		gameScript = system.GetComponent<GameScript>();
		rpcScript = system.GetComponent<RPCScript>();
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

	public void setShip()
	{
		if (currentSelection.Count == 2 && currentSelection.Count != 0){
			Vector3 startPos = currentSelection[0].transform.position;
			Vector3 endPos = currentSelection[currentSelection.Count - 1].transform.position;

			PlaceShip(startPos.x, startPos.z, endPos.x, endPos.z, 1, gameScript.myname);
			currentSelection.Clear();

		}
	}

	public void PlaceShip (float startPosX, float startPosZ, float endPosX, float endPosZ, int local, string Player) {
		if (local == 1)
		{
			rpcScript.setShip(startPosX, startPosZ, endPosX, endPosZ, Player);
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
			else shipDir = GameScript.Direction.West;
			newZ += 0.5f;
		}
		Vector3 pos = new Vector3(newX, newY, newZ);
		// Create ship
		// TODO: Place new ship at correct orientation
		GameObject newShip = Instantiate(destroyer, pos, Quaternion.identity) as GameObject;
		// Add ship to list of ships in GameScript
		gameScript.ships.Add(newShip);
		ShipScript newShipScript = newShip.GetComponent<ShipScript>();
		newShipScript.Init();
		newShipScript.curDir = shipDir;
		newShipScript.SetRotation();
		newShipScript.player = Player;
		Debug.Log("Player: "+Player);
		newShipScript.shipID = Player + idSeedStack.Pop().ToString();
		Debug.Log("New Ship ID: "+ newShipScript.shipID);

		List<GameObject> shipCells = newShipScript.cells;
			
		int roundedStartPosX = (int)Mathf.RoundToInt(startPosX);
		int roundedStartPosZ = (int)Mathf.RoundToInt(startPosZ);

		switch(newShipScript.curDir) {
		case GameScript.Direction.East:
			for (int i = 0; i < newShipScript.shipSize; i++) {
				GameObject newCell = grid[roundedStartPosX + i, roundedStartPosZ];
				CellScript newCellScript = newCell.GetComponent<CellScript>();
				newCellScript.occupier = newShip;
				newCellScript.selected = false;
				newCellScript.available = false;
				newCellScript.curCellState = GameScript.CellState.Ship;
				newCellScript.DisplaySelection();

				shipCells.Add (newCell);
			}
			break;
		case GameScript.Direction.North:
			for (int i = 0; i < newShipScript.shipSize; i++) {
				GameObject newCell = grid[roundedStartPosX, roundedStartPosZ + i];
				CellScript newCellScript = newCell.GetComponent<CellScript>();
				newCellScript.occupier = newShip;
				newCellScript.selected = false;
				newCellScript.available = false;
				newCellScript.curCellState = GameScript.CellState.Ship;
				newCellScript.DisplaySelection();

				shipCells.Add (newCell);
			}
			break;
		case GameScript.Direction.South:
			for (int i = 0; i < newShipScript.shipSize; i++) {
				GameObject newCell = grid[roundedStartPosX, roundedStartPosZ - i];
				CellScript newCellScript = newCell.GetComponent<CellScript>();
				newCellScript.occupier = newShip;
				newCellScript.selected = false;
				newCellScript.available = false;
				newCellScript.curCellState = GameScript.CellState.Ship;
				newCellScript.DisplaySelection();
				shipCells.Add (newCell);
			}
			break;
		case GameScript.Direction.West:
			for (int i = 0; i < newShipScript.shipSize; i++) {
				GameObject newCell = grid[roundedStartPosX - i, roundedStartPosZ];
				CellScript newCellScript = newCell.GetComponent<CellScript>();
				newCellScript.occupier = newShip;
				newCellScript.selected = false;
				newCellScript.available = false;
				newCellScript.curCellState = GameScript.CellState.Ship;
				newCellScript.DisplaySelection();
				shipCells.Add (newCell);
			}
			break;
		}
		currentSelection.Clear();

		
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

	public void CustomSetupUpdate () {
		// Handle update function for grid during update phase

	}

	/** HELPER METHODS **/

	// Initialize grid of specified size
	private void CreateGrid () {
		// Create cell objects to from grid
		grid = new GameObject[gridSize,gridSize];
		for (int i = 0; i < gridSize; i++) {
			for (int j = 0; j < gridSize; j++) {
				Vector3 cellPos = new Vector3(i * cellSize[0], 0, j * cellSize[1]);
				GameObject newCell = Instantiate(gridCell, cellPos, Quaternion.identity) as GameObject;
				newCell.GetComponent<CellScript>().Init();
				newCell.GetComponent<CellScript>().SetGridPosition(i, j);
				newCell.transform.localScale = new Vector3(cellSize[0], 0.5f, cellSize[1]);
				grid[i,j] = newCell;
			}
		}

		// Create bases for each player
		// Create base on grid for Player 1
		for (int i = 0; i < 10; i++) {
			grid[0, 10 + i].GetComponent<CellScript>().SetBase(Color.magenta);
		}
		// Create base on grid for Player 2
		for (int i = 0; i < 10; i++) {
			grid[29, 10 + i].GetComponent<CellScript>().SetBase(Color.cyan);
		}
		Random.seed = reefSeed;
		// Create reefs on grid
		for (int i = 0; i < 24; i++) {
			int xVal = Random.Range(10, 20);
			int yVal = Random.Range(3, 27);
			while(grid[xVal, yVal].GetComponent<CellScript>().curCellState == GameScript.CellState.Reef) {
				xVal = Random.Range(10, 20);
				yVal = Random.Range(3, 27);
			}
			grid[xVal, yVal].GetComponent<CellScript>().SetReef();
		}
	}

	// Returns the neighbours of the given cell in the grid
	public List<GameObject> GetCellNeighbours (GameObject cell) {
		List<GameObject> neighbours = new List<GameObject>();
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
	public GameObject[,] GetCellsInRange (int positionX, int positionY, int dx, int dy) {
		GameObject[,] cells = new GameObject[dx, dy];
		for (int x = 0; x < dx; x++) {
			for (int y = 0; y < dy; y++) {
				cells[x, y] = grid[positionX+x, positionY+y];
			}
		}
		return cells;
	}

	/*
	 * Returns the valid destination cell within the given path
	 */
	public CellScript VerifyCellPath (int positionX, int positionY, int dist, GameScript.Direction dir, CellScript destCell) {
		bool obstacleEncountered = false;
		CellScript encounteredObstacle = destCell;
		switch (dir) {
		case GameScript.Direction.East:
			for (int x = 1; x <= dist; x++) {
				GameObject cell = grid[positionX + x, positionY];
				CellScript curCellScript = cell.GetComponent<CellScript>();
				if (curCellScript.available != true) {
					obstacleEncountered = true;
					encounteredObstacle = grid[positionX + (x-1), positionY].GetComponent<CellScript>();
					break;
				}
			}
			break;
		case GameScript.Direction.West:
			for (int x = 1; x <= dist; x++) {
				GameObject cell = grid[positionX - x, positionY];
				CellScript curCellScript = cell.GetComponent<CellScript>();
				if (curCellScript.available != true) {
					obstacleEncountered = true;
					encounteredObstacle = cell.GetComponent<CellScript>();
					break;
				}
			}
			break;
		case GameScript.Direction.North:
			for (int y = 1; y <= dist; y++) {
				GameObject cell = grid[positionX, positionY + y];
				CellScript curCellScript = cell.GetComponent<CellScript>();
				if (curCellScript.available != true) {
					obstacleEncountered = true;
					encounteredObstacle = cell.GetComponent<CellScript>();
					break;
				}
			}
			break;
		case GameScript.Direction.South:
			for (int y = 1; y <= dist; y++) {
				GameObject cell = grid[positionX, positionY - y];
				CellScript curCellScript = cell.GetComponent<CellScript>();
				if (curCellScript.available != true) {
					obstacleEncountered = true;
					encounteredObstacle = cell.GetComponent<CellScript>();
					break;
				}
			}
			break;
		}
		return encounteredObstacle;
	}

	// Adds given cell to current selection - returns FALSE if not a valid selection
	public bool AddToSelection (GameObject cell) {
		bool valid = false;
		if (currentSelection.Count > 0) {
			List<GameObject> neighbours = GetCellNeighbours(cell);
			foreach (GameObject o in currentSelection) {
				if (neighbours.Contains(o)) {
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

	public GameObject GetCell(int x, int y) {
		return grid [x, y];
	}

	// Removes given cell from current selection - if cell is not in selection, does nothing
	public void RemoveFromSelection (GameObject cell) {
		if (currentSelection.Contains(cell)) currentSelection.Remove(cell);
	}
}
