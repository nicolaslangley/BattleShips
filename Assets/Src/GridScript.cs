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
	public GameObject[,] grid;
	public List<GameObject> currentSelection;

	private GameObject system;
	private GUIScript guiScript;

	/** UNITY METHODS **/

	// Use this for initialization
	void Start () {
		CreateGrid ();
		currentSelection = new List<GameObject>();
		system = GameObject.FindGameObjectWithTag("System");
		guiScript = system.GetComponent<GUIScript>();
	}

	void Update () {
	
		// Check if ship is to be added to grid
		if (guiScript.allowShipPlacement == true) {
			if (currentSelection.Count == guiScript.shipLength && currentSelection.Count != 0) {
				// Place ship over selected squares
				// Get position of first item in currentSelection
				Vector3 startPos = currentSelection[0].transform.position;
				Vector3 endPos = currentSelection[currentSelection.Count - 1].transform.position;
				float newX = ((endPos.x - startPos.x) / 2) + startPos.x;
				float newZ = ((endPos.z - startPos.z) / 2) + startPos.z;
				float newY = 0.5f;
				Vector3 pos = new Vector3(newX, newY, newZ);
				// Create ship
				GameObject newShip = Instantiate(ship1, pos, Quaternion.identity) as GameObject;

				// Reset selection of ship and cells
				guiScript.shipLength = 0;
				foreach (GameObject o in currentSelection) {
					CellScript cs = o.GetComponent<CellScript>();
					cs.selected = false;
					cs.DisplaySelection();
				}
				currentSelection.Clear();
			}
		}

	}

	/** HELPER METHODS **/

	// Initialize grid of specified size
	void CreateGrid () {
		grid = new GameObject[gridSize,gridSize];
		for (int i = 0; i < gridSize; i++) {
			for (int j = 0; j < gridSize; j++) {
				Vector3 cellPos = new Vector3(i * cellSize[0], 0, j * cellSize[1]);
				GameObject newCell = Instantiate(gridCell, cellPos, Quaternion.identity) as GameObject;
				newCell.transform.localScale = new Vector3(cellSize[0], 0.5f, cellSize[1]);
				grid[i,j] = newCell;
			}
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
