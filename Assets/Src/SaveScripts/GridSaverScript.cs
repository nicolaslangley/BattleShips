using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridSaverScript {
	public int gridSize;
	public Vector2 cellSize;
	public List<List<CellSaverScript>> grid;

	public GridSaverScript(){}

	public GridSaverScript(GridScript gridScript) {
		gridSize = gridScript.gridSize;
		cellSize = gridScript.cellSize;
		grid = new List<List<CellSaverScript>> ();
		for (int x = 0; x < grid.Count; x++) {
			grid.Add (new List<CellSaverScript>());
			for (int y = 0; y < grid.Count; y++) {
				grid[x][y] = new CellSaverScript(gridScript.grid[x,y]);
			}
		}
	}

	public void Restore(GridScript gridScript) {
		//We assume that gridScript has already been instantiated to the correct size
		for (int x = 0; x < grid.Count; x++) {
			for (int y = 0; y < grid[x].Count; y++) {
				grid[x][y].Restore(gridScript.grid[x,y]);
			}
		}
	}
}
