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

	public GameSaverScript() {}

	public GameSaverScript(GameScript gameScript)  {
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

		foreach (ShipSaverScript shipSaver in loader.ships) {
			ShipScript ship;
			//I'm assuming this is how PlaceShip works.
			float startPosX = shipSaver.cells[0].gridPositionX;
			float startPosZ = shipSaver.cells[0].gridPositionY;
			float endPosX = shipSaver.health.Length;
			float endPosZ = shipSaver.health.Length;
			int local = 1; //Not sure what to do about this...
			ship = gameScript.gridScript.PlaceShip(startPosX, startPosZ, endPosX, endPosZ, local, shipSaver.player);

			shipSaver.Restore(ship, gameScript);
		}
	}
}
