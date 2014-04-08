using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Text;

public class GridSaverScript {
	public int gridSize;
	public Vector2 cellSize;
	public List<List<CellSaverScript>> grid;

	public GridSaverScript(){}

	public GridSaverScript(GridScript gridScript) {
		gridSize = gridScript.gridSize;
		cellSize = gridScript.cellSize;
		grid = new List<List<CellSaverScript>> ();
		for (int x = 0; x < grid.Count; x++) {
			grid.Add (new List<CellSaverScript>());
			for (int y = 0; y < grid.Count; y++) {
				grid[x][y] = new CellSaverScript(gridScript.grid[x,y]);
			}
		}
	}

	public void Restore(GameScript gameScript) {
		XmlSerializer serialize = new XmlSerializer(typeof(CellSaverScript));
		XmlWriterSettings settings = new XmlWriterSettings();
		settings.Encoding = new UnicodeEncoding(false, false);
		settings.Indent = false;
		settings.OmitXmlDeclaration = false;
		//We assume that gridScript has already been instantiated to the correct size
		for (int x = 0; x < grid.Count; x++) {
			for (int y = 0; y < grid[x].Count; y++) {
				using(StringWriter textWriter = new StringWriter()) {
					using(XmlWriter xmlWriter = XmlWriter.Create(textWriter, settings)) {
						serialize.Serialize(xmlWriter, grid[x][y]);
					}
					gameScript.rpcScript.SetGridCell(textWriter.ToString());
				}
			}
		}
	}
}
