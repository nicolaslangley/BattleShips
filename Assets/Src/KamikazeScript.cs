using UnityEngine;
using System.Collections;

public class KamikazeBoatScript : ShipScript {

	// Use this for initialization
	void Start () {
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
	}

	void OnGui() {
		this.OnGui ();
		if (GUI.Button(new Rect(Screen.width - 170, 90, 100, 30), "Detonate")) {
			Debug.Log ("Boom!! Not implemented.");
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
