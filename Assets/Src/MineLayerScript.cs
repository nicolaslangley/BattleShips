using UnityEngine;
using System.Collections;

public class MineLayerScript : ShipScript {

	int minesLeft;

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

		minesLeft = 5;
	}

	void OnGUI () {
		//Debug.Log ("Mine Layer GUI called");
		shipGUI();
		MineLayerGUI();
	}

	void MineLayerGUI() {
		if (selected && minesLeft > 0) {
			if (GUI.Button(new Rect(Screen.width - 150, 210, 100, 30), "Drop Mine")) {
				DisplayMineRange(true);
				gameScript.curPlayAction = GameScript.PlayAction.DropMine;
			}
		}
	}

}
