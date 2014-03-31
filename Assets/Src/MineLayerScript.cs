﻿using UnityEngine;
using System.Collections;

public class MineLayerScript : ShipScript {

	// Use this for initialization
	void Start () {
		this.shipSize = 3;
		this.heavyArmor = true;
		this.heavyCannon = false;
		this.maxSpeed = 6;
		
		this.radarRangeForward = 6;
		this.radarRangeSide = 5;
		this.radarRangeStart = 1;
		
		this.cannonRangeForward = 4;
		this.cannonRangeSide = 5;
		this.cannonRangeStart = -1;
	}

	void OnGui() {
		this.OnGui ();
		if (GUI.Button(new Rect(Screen.width - 110, 1700, 100, 30), "Drop Mine")) {
			DisplayMineRange(true);
			gameScript.curPlayAction = GameScript.PlayAction.DropMine;
		}
	}
	public override void LayMine(CellScript cell) {
		cell.curCellState = GameScript.CellState.Mine;
		DisplayMineRange (false);
	}

	void DisplayMineRange(bool display) {
		foreach (CellScript cell in this.cells) {
			foreach (CellScript neighbour in gridScript.GetCellNeighbours(cell)) {
				if (neighbour.curCellState == GameScript.CellState.Available) 
					gridScript.DisplayCellForShoot(display, neighbour);
			}
		}
	}
}
