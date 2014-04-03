using UnityEngine;
using System.Collections;

public class CruiserScript : ShipScript {

	// Use this for initialization
	void Start() {
		this.shipSize = 5;
		this.heavyArmor = true;
		this.heavyCannon = true;
		this.maxSpeed = 10;

		this.radarRangeForward = 10;
		this.radarRangeSide = 3;
		this.radarRangeStart = 1;

		this.cannonRangeForward = 15;
		this.cannonRangeSide = 11;
		this.cannonRangeStart = -5;

		this.shipType = "cruiser";
	}

	// Update is called once per frame
	void Update () {
		
	}
}
