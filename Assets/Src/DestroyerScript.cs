﻿using UnityEngine;
using System.Collections;

public class DestroyerScript : ShipScript {

	// Use this for initialization
	void Awake () {
		this.shipSize = 4;
		this.heavyArmor = false;
		this.heavyCannon = false;
		this.maxSpeed = 8;
		
		this.radarRangeForward = 8;
		this.radarRangeSide = 3;
		this.radarRangeStart = 1;
		
		this.cannonRangeForward = 12;
		this.cannonRangeSide = 9;
		this.cannonRangeStart = -4;

		this.hasCannon = true;
		this.hasTorpedo = true;
		this.canRotate = true;

		this.shipType = "destroyer";
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
