using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Text;

[XmlRoot("SavedGame")]
public class GameSaverScript {
	[XmlArray("Ships")]
	[XmlArrayItem("Ship")]
	public List<ShipSaverScript> ships;

	public string myname;
	public string opponentname;
	public string turn;
	
	public GameScript.PlayerType myPlayerType;
	public GameScript.PlayerType winner;
	public GameScript.GameState curGameState;

	public GridSaverScript grid;

	public GameSaverScript() {}

	public GameSaverScript(GameScript gameScript)  {
		grid = new GridSaverScript (gameScript.gridScript);
		ships = new List<ShipSaverScript> ();
		foreach (ShipScript ship in gameScript.ships) {
			ships.Add (new ShipSaverScript(ship));
		}
		myname = gameScript.myname;
		opponentname = gameScript.opponentname;
		turn = gameScript.turn;
		curGameState = gameScript.curGameState;
		myPlayerType = gameScript.myPlayerType;
		winner = gameScript.winner;
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
		loader.grid.Restore (gameScript);
		gameScript.myname = loader.myname;
		gameScript.opponentname = loader.opponentname;
		gameScript.turn = loader.turn;
		gameScript.myPlayerType = loader.myPlayerType;
		gameScript.winner = loader.winner;
		gameScript.curGameState = loader.curGameState;

		if (loader.myname == gameScript.myname) {

		} else {

		}

		XmlSerializer serialize = new XmlSerializer(typeof(ShipSaverScript));
		XmlWriterSettings settings = new XmlWriterSettings();
		settings.Encoding = new UnicodeEncoding(false, false);
		settings.Indent = false;
		settings.OmitXmlDeclaration = false;

		foreach (ShipSaverScript shipSaver in loader.ships) {
			using(StringWriter textWriter = new StringWriter()) {
				using(XmlWriter xmlWriter = XmlWriter.Create(textWriter, settings)) {
					serialize.Serialize(xmlWriter, shipSaver);
				}
				gameScript.rpcScript.CreateShip(textWriter.ToString());
			}
		}
	}
}
