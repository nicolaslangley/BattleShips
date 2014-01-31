using UnityEngine;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;

public class ShipScript {
	[XmlAttribute("player")]
	public string player;

	public ShipScript() {
		player = "Horatio";
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
