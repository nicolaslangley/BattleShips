using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class GridScript : MonoBehaviour {

	/** PROPERTIES **/

	//Prefabs
	public GameObject gridCell;
	public GameObject ship1;
	
	public int gridSize;
	public Vector2 cellSize;
	public CellScript[,] grid;
	public List<GameObject> currentSelection;

	private GameObject system;
	private GameScript gameScript;

	/** GAMELOOP METHODS **/

	// Use this for initialization
	public void Init () {
		// Create grid of cells
		CreateGrid ();

		CellScript[,] testCells = range (3, 4, 5, 2);
		for (int x = 0; x < 5; x ++) {
			for (int y = 0; y < 2; y++) {
				testCells[x, y].selected = true;
			}
		}

		// Initialize references to system and current selection
		currentSelection = new List<GameObject>();
		system = GameObject.FindGameObjectWithTag("System");
		gameScript = system.GetComponent<GameScript>();
	}

	public void PlaceShip () {
		if (currentSelection.Count == 2 && currentSelection.Count != 0) {
			// Place ship over selected squares
			// Get position of first item in currentSelection
			Vector3 startPos = currentSelection[0].transform.position;
			Vector3 endPos = currentSelection[currentSelection.Count - 1].transform.position;
			float newX = ((endPos.x - startPos.x) / 2) + startPos.x - 0.5f;
			float newZ = ((endPos.z - startPos.z) / 2) + startPos.z - 0.5f;
			float newY = 0.5f;
			// Update newZ and newX based on orientation
			GameScript.Direction shipDir = GameScript.Direction.East;
			// Ship is facing either North or South
			if (endPos.x - startPos.x == 0) {
				if (endPos.z > startPos.z) shipDir = GameScript.Direction.North;
				else shipDir = GameScript.Direction.South;

				newX += 0.5f;
			}
			// Ship is facing either East or West
			if (endPos.z - startPos.z == 0) { 
				if (endPos.x > startPos.x) shipDir = GameScript.Direction.East;
				else shipDir = GameScript.Direction.West;
				newZ += 0.5f;
			}
			Vector3 pos = new Vector3(newX, newY, newZ);
			// Create ship
			// TODO: Place new ship at correct orientation
			GameObject newShip = Instantiate(ship1, pos, Quaternion.identity) as GameObject;
			// Add ship to list of ships in GameScript
			gameScript.ships.Add(newShip);
			ShipScript newShipScript = newShip.GetComponent<ShipScript>();
			newShipScript.Init();
			newShipScript.curDir = shipDir;
			newShipScript.SetRotation();
			List<GameObject> shipCells = newShip.GetComponent<ShipScript>().cells;
			
			// Reset selection of ship and cells
			foreach (GameObject o in currentSelection) {
				CellScript cs = o.GetComponent<CellScript>();
				cs.selected = false;
				cs.DisplaySelection();
				shipCells.Add(o);
			}
			currentSelection.Clear();
		}
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
	public CellScript[,] range (int positionX, int positionY, int dx, int dy) {
		CellScript[,] cells = new CellScript[dx, dy];
		for (int x = 0; x < dx; x++) {
			for (int y = 0; y < dy; y++) {
				cells[x, y] = grid[positionX+x, positionY+y];
			}
		}
		return cells;
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

	// Removes given cell from current selection - if cell is not in selection, does nothing
	public void RemoveFromSelection (GameObject cell) {
		if (currentSelection.Contains(cell)) currentSelection.Remove(cell);
	}
}
