using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class ShipSaverScript {
	[XmlAttribute("shipID")]
	public string shipID;
	[XmlAttribute("player")]
	public string player;
	[XmlArray("cells_occupied")]
	[XmlArrayItem("cell")]
	public List<CellSaverScript> cells;
	public GameScript.Direction curDir;
	[XmlAttribute("shipType")]
	public string shipType;
	public int[] health;
	public CellSaverScript baseCell;
	[XmlAttribute("speed")]
	public int speed; 

	public ShipSaverScript() {}

	public ShipSaverScript(ShipScript ship) {
		shipID = ship.shipID;
		player = ship.player;

		cells = new List<CellSaverScript> ();
		foreach (CellScript cell in ship.cells) {
			cells.Add(new CellSaverScript(cell));
		}

		curDir = ship.curDir;
		health = ship.health;
		if (baseCell != null) baseCell = new CellSaverScript (ship.baseCell);
		speed = ship.speed;
		shipType = ship.shipType;
	}

	public void Restore(ShipScript ship, GameScript gameScript) {
		ship.shipID = shipID;
		ship.player = player;

		ship.cells = new List<CellScript> ();
		foreach (CellSaverScript cell in cells) {
			ship.cells.Add(gameScript.gridScript.grid[cell.gridPositionX,cell.gridPositionY]);
		}

		ship.curDir = curDir;
		ship.health = health;
		if (baseCell != null) ship.baseCell = gameScript.gridScript.grid [baseCell.gridPositionX, baseCell.gridPositionY];
		ship.speed = speed;
		shipType = ship.shipType;
	}
}
