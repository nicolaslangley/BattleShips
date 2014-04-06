using UnityEngine;
using System.Collections;

public class KamikazeBoatScript : ShipScript {

	// Use this for initialization
	void Awake () {
		this.shipSize = 1;
		this.heavyArmor = true;
		this.heavyCannon = false;
		this.maxSpeed = 2;
		
		this.radarRangeForward = 5;
		this.radarRangeSide = 5;
		this.radarRangeStart = -2;
		
		this.cannonRangeForward = 3;
		this.cannonRangeSide = 3;
		this.cannonRangeStart = -1;
		this.shipType = "kamikaze";
	}

	void OnGUI() {
		this.OnGUI ();
		if (GUI.Button(new Rect(Screen.width - 170, 90, 100, 30), "Detonate")) {
			int startX = this.cells[0].gridPositionX;
			int startY = this.cells[0].gridPositionY;

			for (int x = (startX > 0 ? startX-1 : startX); x < 3 && x < this.gridScript.grid.GetLength(0); x++) {
				for (int y = (startY > 0 ? startY-1 : startY); y < 3 && y < this.gridScript.grid.GetLength(1); y++) {
					CellScript temp = this.gridScript.grid[x, y];
					if (temp.curCellState == GameScript.CellState.Ship) {
						if (temp.occupier != null) {
							ShipSectionScript section = temp.occupier.GetComponent<ShipSectionScript>();
							ShipScript ship = section.parent;
							if (ship.player == gameScript.myname) {
								ship.HandleHit(temp.occupier, 1, 1);
							} else {
								this.rpcScript.fireCannonShip(ship.shipID, 0, 1);
							}
						}
					}
				}
			}
		}
	}
}
