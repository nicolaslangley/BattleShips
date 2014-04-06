using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

[XmlRoot("SavedGame")]
public class GameSaverScript {
	[XmlArray("Ships")]
	[XmlArrayItem("Ship")]
	public List<ShipSaverScript> ships;

	public GridSaverScript grid;

	public GameSaverScript() {}

	public GameSaverScript(GameScript gameScript)  {
		grid = new GridSaverScript (gameScript.gridScript);
		ships = new List<ShipSaverScript> ();
		foreach (ShipScript ship in gameScript.ships) {
			ships.Add (new ShipSaverScript(ship));
		}
	}
 
	public void Save(string path) {
		//Copy all values into the saver scripts
		var serializer = new XmlSerializer(typeof(GameSaverScript));
		using(var stream = new FileStream(path, FileMode.Create))
		{
			serializer.Serialize(stream, this);
		}
	}
	
	public static void Load(string path, GameScript gameScript) {
		var serializer = new XmlSerializer (typeof(GameSaverScript));
		GameSaverScript loader;
		using (var stream = new FileStream(path, FileMode.Open)) {
			loader = serializer.Deserialize (stream) as GameSaverScript;
		}
		//Set values from saver scripts

		loader.grid.Restore (gameScript.gridScript);

		foreach (ShipSaverScript shipSaver in loader.ships) {
			ShipScript ship;
			//I'm assuming this is how PlaceShip works.
			int startX = shipSaver.cells[0].gridPositionX;
			int startY = shipSaver.cells[0].gridPositionY;
			int length = shipSaver.cells.Count;
			int endX = shipSaver.cells[length-1].gridPositionX;
			int endY = shipSaver.cells[length-1].gridPositionY;

			float startPosX = gameScript.gridScript.grid[startX, startY].transform.position.x;
			float startPosZ = gameScript.gridScript.grid[startX, startY].transform.position.z;
			float endPosX = gameScript.gridScript.grid[endX, endY].transform.position.x;
			float endPosZ = gameScript.gridScript.grid[endX, endY].transform.position.z;
			int local = 1; //Not sure what to do about this...
			ship = gameScript.gridScript.PlaceShip(startPosX, startPosZ, endPosX, endPosZ, local, shipSaver.player, GameScript.ShipTypes.Mine, GameScript.PlayerType.Player1);

			shipSaver.Restore(ship, gameScript);
		}
	}
}
