using UnityEngine;
using System.Collections;

public class TorpedoBoatScript : ShipScript {

	// Use this for initialization
	void Awake () {
		this.shipSize = 3;
		this.heavyArmor = false;
		this.heavyCannon = false;
		this.maxSpeed = 9;
		
		this.radarRangeForward = 6;
		this.radarRangeSide = 3;
		this.radarRangeStart = 1;
		
		this.cannonRangeForward = 5;
		this.cannonRangeSide = 0;
		this.cannonRangeStart = 5;

		this.rotSteps = 1;
		this.hasCannon = true;
		this.hasTorpedo = true;
		this.canRotate = true;

		this.shipType = "torpedoboat";
	}
	
	/*
	 * Rotates the ship
	 */
	public override void RotateShip(bool clockwise, int local) {

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
		CellScript front = cells[cells.Count-1];
		CellScript back = cells[0];
		CellScript baseCellScript = cells[1];

		int frontX = front.gridPositionX;
		int frontY = front.gridPositionY;
		int x = baseCellScript.gridPositionX;
		int y = baseCellScript.gridPositionY;
		int backX = back.gridPositionX;
		int backY = back.gridPositionY;
		CellScript[,] grid = gridScript.grid;
		int sign = (clockwise ? 1 : -1);
		// Perform check based on the orientation of the ship
		if (curDir == GameScript.Direction.North || curDir == GameScript.Direction.South) {
			obstacle = obstacle || grid[frontX+sign, frontY].curCellState != GameScript.CellState.Available
				|| grid[backX-sign, backY].curCellState != GameScript.CellState.Available
					|| grid[x+1,y].curCellState != GameScript.CellState.Available || grid[x-1,y].curCellState != GameScript.CellState.Available;
		} else {
			obstacle = obstacle || grid[frontX, frontY+sign].curCellState != GameScript.CellState.Available
				|| grid[backX, backY-sign].curCellState != GameScript.CellState.Available
					|| grid[x, y+1].curCellState != GameScript.CellState.Available || grid[x, y-1].curCellState != GameScript.CellState.Available;
		}

		if (!obstacle) {
			curDir = (GameScript.Direction)newRot;
			// Reset all except rotation base cells to be unoccupied

			foreach (CellScript oCellScript in cells) {
				if (oCellScript == baseCellScript) continue;
				
				Debug.Log ("Resetting cell at position: " + oCellScript.gridPositionX + " " + oCellScript.gridPositionY);
				oCellScript.occupier = null;
				oCellScript.selected = false;
				oCellScript.available = true;
				oCellScript.curCellState = GameScript.CellState.Available;
			}
			cells.Clear();


			// Based on direction of ship set currently occupied cells
			switch(curDir) {
			case GameScript.Direction.East:
				cells.Add(grid[x-1, y]);
				cells.Add(grid[x,y]);
				cells.Add(grid[x+1, y]);
				break;
			case GameScript.Direction.North:
				cells.Add(grid[x, y-1]);
				cells.Add(grid[x,y]);
				cells.Add(grid[x, y+1]);
				break;
			case GameScript.Direction.South:
				cells.Add(grid[x, y+1]);
				cells.Add(grid[x,y]);
				cells.Add(grid[x, y-1]);
				break;
			case GameScript.Direction.West:
				cells.Add(grid[x+1, y]);
				cells.Add(grid[x,y]);
				cells.Add(grid[x-1,y]);
				break;
			}
			//TODO: 180
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
		transform.position = cells [0].transform.position + new Vector3(0,cells[0].renderer.bounds.size.y, 0);
		gameScript.EndTurn();
	}
}
