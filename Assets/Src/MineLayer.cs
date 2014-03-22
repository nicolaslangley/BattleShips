using UnityEngine;
using System.Collections;

public class MineLayer : ShipScript {

	// Use this for initialization
	void Start () {
		this.shipSize = 2;
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
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnGui() {
		this.OnGui ();
		if (GUI.Button(new Rect(Screen.width - 110, 1700, 100, 30), "Drop Mine")) {
			Debug.Log ("Drop mine not implemented.");
		}
	}
}
