using UnityEngine;
using System.Collections;

public class TorpedoBoatScript : ShipScript {

	// Use this for initialization
	void Start () {
		this.shipSize = 3;
		this.heavyArmor = false;
		this.heavyCannon = false;
		this.maxSpeed = 9;
		
		this.radarRangeForward = 6;
		this.radarRangeSide = 3;
		this.radarRangeStart = 3;
		
		this.cannonRangeForward = 5;
		this.cannonRangeSide = 0;
		this.cannonRangeStart = 5;

		this.rotSteps = 2;

		this.shipType = "torpedoboat";
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
