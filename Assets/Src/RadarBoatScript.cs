using UnityEngine;
using System.Collections;

public class RadarBoatScript : ShipScript {

	// Use this for initialization
	void Start () {
		this.shipSize = 3;
		this.heavyArmor = false;
		this.heavyCannon = false;
		this.maxSpeed = 3;
		
		this.radarRangeForward = 6; //By default at start
		this.radarRangeSide = 3;
		this.radarRangeStart = 1;
		
		this.cannonRangeForward = 5;
		this.cannonRangeSide = 3;
		this.cannonRangeStart = -1;
	}

	void ToggleLongRange() {
		if (this.radarRangeForward == 6) { 
			this.radarRangeForward = 12;
			this.immobile = true;
		} else {
			this.radarRangeForward = 6;
			this.immobile = false;
		}
	}

	void OnGui() {
		this.OnGui ();
		if (GUI.Button(new Rect(Screen.width - 170, 90, 100, 30), "Toggle Long Range")) {
			ToggleLongRange();
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
