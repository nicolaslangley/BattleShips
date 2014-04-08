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

	void OnGUI () {
		if (selected) {
			shipGUI();
			KamikazeGUI();
		}
	}

	void KamikazeGUI() {
		if (GUI.Button(new Rect(Screen.width - 170, 90, 120, 30), "Detonate")) {
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


	IEnumerator MoveKamikazeBoat (Vector3 destPos) {
		destPos.y += 0.5f;
		Vector3 start = transform.position;
		// Time.time contains current frame time, so remember starting point
		float startTime=Time.time;
		// Perform the following until 1 second has passed
		while(Time.time-startTime <= 1) {
			// lerp from A to B in one second
			transform.position = Vector3.Lerp(start,destPos,Time.time-startTime); 
			// Wait for next frame
			yield return 1; 
		}
	}

	CellScript checkValidMove(CellScript destCell, int startX, int startY) {
		CellScript validDestCell = destCell;
		int destX = destCell.gridPositionX;
		int destY = destCell.gridPositionY;
		if (destY == startY) {
			if (destX < startX) {
				validDestCell = gridScript.VerifyCellPath(startX, startY, 2, GameScript.Direction.West, destCell, "Move", false);
			} else {
				// destX > startX
				validDestCell = gridScript.VerifyCellPath(startX, startY, 2, GameScript.Direction.East, destCell, "Move", false);
			}
		} else if (destX == startX) {
			if (destY < startY) {
				validDestCell = gridScript.VerifyCellPath(startX, startY, 2, GameScript.Direction.South, destCell, "Move", false);
			} else {
				// destY > startY
				validDestCell = gridScript.VerifyCellPath(startX, startY, 2, GameScript.Direction.North, destCell, "Move", false);
			}
		} else {
			// Check diagonals
			if (destX < startX && destY > startY) {
				// Upper left diagonal
				while (destX < startX && destY > startY) {
					destX += 1;
					destY -= 1;
					if (!gridScript.VerifyCell(destX, destY)) {
						validDestCell = gridScript.grid[destX, destY];
					}
				}
			} 
			if (destX > startX && destY > startY) {
				// Upper right diagonal
				while (destX > startX && destY > startY) {
					destX -= 1;
					destY -= 1;
					if (!gridScript.VerifyCell(destX, destY)) {
						validDestCell = gridScript.grid[destX, destY];
					}
				}
			}
			if (destX < startX && destY < startY) {
				// Bottom left diagonal
				while (destX < startX && destY < startY) {
					destX += 1;
					destY += 1;
					if (!gridScript.VerifyCell(destX, destY)) {
						validDestCell = gridScript.grid[destX, destY];
					}
				}
			}
			if (destX > startX && destY < startY) {
				// Bottom right diagonal
				while (destX > startX && destY < startY) {
					destX -= 1;
					destY += 1;
					if (!gridScript.VerifyCell(destX, destY)) {
						validDestCell = gridScript.grid[destX, destY];
					}
				}
			}
		}
		return validDestCell;
	}


	public override void MoveShip (CellScript destCell, int local) {
		if (local == 1) {
			rpcScript.NetworkMoveShip(shipID, destCell.gridPositionX, destCell.gridPositionY);
			return;
		}

		// TODO: Check path for movement is valid
		Debug.Log ("Called MoveShip within Kamikaze Boat for " + player);

		// De-select current cell for ship
		CellScript curCellScript = cells[0];
		curCellScript.occupier = null;
		curCellScript.selected = false;
		curCellScript.available = true;
		curCellScript.curCellState = GameScript.CellState.Available;
		cells.Clear();

		// Select current cell for ship
		cells.Add(destCell);
		destCell.occupier = this.gameObject;
		destCell.available = false;
		destCell.curCellState = GameScript.CellState.Ship;
		destCell.selected = true;

		// Un-display move range and end turn
		// TODO: range is not being un-displayed properly
		DisplayMoveRange(false);
		// Move ship to target cell
		StartCoroutine(MoveKamikazeBoat(destCell.transform.position));
		gameScript.EndTurn();
	}

	public override void DisplayMoveRange(bool status) {
		CellScript backCellScript = cells[0];
		int startX = backCellScript.gridPositionX;
		int startY = backCellScript.gridPositionY;
		for (int x = -2; x <= 2; x++) {
			for (int y = -2; y <= 2; y++) {
				gridScript.DisplayCellForMove(status, startX + x, startY + y);
			}
		}
	}

}
