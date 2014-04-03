using UnityEngine;
using System.Collections;

public class GridSaverScript {
	public int gridSize;
	public Vector2 cellSize;
	public CellSaverScript[,] grid;

	public GridSaverScript(){}

	public GridSaverScript(GridScript gridScript) {
		gridSize = gridScript.gridSize;
		cellSize = gridScript.cellSize;
		grid = new CellSaverScript[gridScript.grid.GetLength(0),gridScript.grid.GetLength(1)];
		for (int x = 0; x < grid.GetLength(0); x++) {
			for (int y = 0; y < grid.GetLength(1); y++) {
				grid[x,y] = new CellSaverScript(gridScript.grid[x,y]);
			}
		}
	}

	public void Restore(GridScript gridScript) {
		//We assume that gridScript has already been instantiated to the correct size
		for (int x = 0; x < grid.GetLength(0); x++) {
			for (int y = 0; y < grid.GetLength(1); y++) {
				grid[x,y].Restore(gridScript.grid[x,y]);
			}
		}
	}
}
