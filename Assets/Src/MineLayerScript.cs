using UnityEngine;
using System.Collections;

public class MineLayerScript : ShipScript {

	// Use this for initialization
	void Awake () {
		this.shipSize = 2;
		this.heavyArmor = true;
		this.heavyCannon = false;
		this.maxSpeed = 6;
		
		this.radarRangeForward = 6;
		this.radarRangeSide = 5;
		this.radarRangeStart = -2;
		
		this.cannonRangeForward = 4;
		this.cannonRangeSide = 5;
		this.cannonRangeStart = -1;

		this.hasCannon = true;
		this.hasMine = true;
		this.canRotate = true;

		this.shipType = "minelayer";
	}

	void OnGUI () {
		Debug.Log ("Mine Layer GUI called");
		shipGUI();
		MineLayerGUI();
	}

	void MineLayerGUI() {
		if (selected) {
			if (GUI.Button(new Rect(Screen.width - 150, 210, 100, 30), "Drop Mine")) {
				DisplayMineRange(true);
				gameScript.curPlayAction = GameScript.PlayAction.DropMine;
			}
		}
	}
	public override void LayMine(CellScript cell) {
		cell.curCellState = GameScript.CellState.Mine;
		cell.available = false;
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
