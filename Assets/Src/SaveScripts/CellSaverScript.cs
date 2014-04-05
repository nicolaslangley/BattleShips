using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class CellSaverScript {
	//TODO: This won't save all the cells, so reefs won't be saved. Fix this later.

	public GameScript.CellState curCellState;
	[XmlAttribute("gridPositionX")]
	public int gridPositionX;
	[XmlAttribute("gridPositionY")]
	public int gridPositionY;
	[XmlAttribute("instanceID")]
	public int instanceID;

	public CellSaverScript() {}

	public CellSaverScript(CellScript cell) {
		curCellState = cell.curCellState;
		gridPositionX = cell.gridPositionX;
		gridPositionY = cell.gridPositionY;
		instanceID = cell.instanceID;
	}

	public void Restore(CellScript cell) {
		cell.curCellState = curCellState;
		cell.gridPositionX = gridPositionX;
		cell.gridPositionY = gridPositionY;
		cell.instanceID = instanceID;
	}
}
