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
			DisplayCannonRange(true);
			gameScript.curPlayAction = GameScript.PlayAction.Detonate;
		}
	}

	public override void Detonate (CellScript targetCell, int local) {
		DisplayCannonRange(false);
		gridScript.Explode(targetCell.gridPositionX, targetCell.gridPositionY, GridScript.ExplodeType.Kamikaze);
		// Destroy kamikaze ship
		gameScript.ships.Remove(this);
		foreach (CellScript c in cells) 
		{
			c.available = true;
			c.occupier = null;
			c.isVisible = false;
			c.curCellState = GameScript.CellState.Available;
		}
		Destroy(gameObject);
	}
	
	
	IEnumerator MoveKamikazeBoat (Vector3 destPos) {
		destPos.y += 0.5f;
		Vector3 start = transform.position;
		// Time.time contains current frame time, so remember starting point
		float startTime=Time.time;
		// Perform the following until 1 second has passed
		while(Time.time-startTime <= 2) {
			// lerp from A to B in one second
			transform.position = Vector3.Lerp(start,dest,moveTime); 
			moveTime += Time.deltaTime/1;  
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
		DisplayMoveRange(false);
		CellScript validDestCell = checkValidMove(destCell, cells[0].gridPositionX, cells[0].gridPositionY);
		if (validDestCell != destCell) {
			Debug.Log ("Invalid path, moving up until collision");
			// De-select current cell for ship
			destCell = validDestCell;
		}
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
