using UnityEngine;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

public class RPCScript : MonoBehaviour {

	// Use this for initialization

	private GridScript gridScript;
	private GameScript gameScript;
	private GameObject system;


	void Awake()
	{
		system = GameObject.FindGameObjectWithTag("System");
		gridScript = system.GetComponent<GridScript>();
		gameScript = system.GetComponent<GameScript>();
	}

	public void sendPlayerName(string playerName)
	{
		//Called on client
		//Send everyone this clients data
		networkView.RPC("RPCsendPlayerName", RPCMode.OthersBuffered, Network.player, playerName);
	}

	[RPC]
	void RPCsendPlayerName(NetworkPlayer player, string username)
	{
		gameScript.opponentname = username;
		Debug.Log("opponent name is: " + gameScript.opponentname);

	}

	public void SetGridCell(string serializedCellSaver) {
		networkView.RPC ("RPCSetGridCell", RPCMode.AllBuffered, serializedCellSaver);
	}

	[RPC]
	void RPCSetGridCell(string serializedCellSaver) {
		CellSaverScript cellSaver;

		XmlSerializer serializer = new XmlSerializer (typeof(CellSaverScript));
		XmlReaderSettings settings = new XmlReaderSettings();
		
		using(StringReader textReader = new StringReader(serializedCellSaver)) {
			using(XmlReader xmlReader = XmlReader.Create(textReader, settings)) {
				cellSaver = (CellSaverScript) serializer.Deserialize(xmlReader);
			}
		}
	}

	public void CreateShip(string serializedShipSaver) {
		networkView.RPC ("RPCCreateShip", RPCMode.AllBuffered, serializedShipSaver);
	}

	[RPC]
	void RPCCreateShip(string serializedShipSaver) {
		ShipSaverScript shipSaver;

		XmlSerializer serializer = new XmlSerializer (typeof(ShipSaverScript));
		XmlReaderSettings settings = new XmlReaderSettings();

		using(StringReader textReader = new StringReader(serializedShipSaver)) {
			using(XmlReader xmlReader = XmlReader.Create(textReader, settings)) {
				shipSaver = (ShipSaverScript) serializer.Deserialize(xmlReader);
			}
		}

		ShipScript ship;
		int startX = shipSaver.cells[0].gridPositionX;
		int startY = shipSaver.cells[0].gridPositionY;
		int length = shipSaver.cells.Count;
		int endX = shipSaver.cells[length-1].gridPositionX;
		int endY = shipSaver.cells[length-1].gridPositionY;
		
		float startPosX = gameScript.gridScript.grid[startX, startY].transform.position.x;
		float startPosZ = gameScript.gridScript.grid[startX, startY].transform.position.z;
		float endPosX = gameScript.gridScript.grid[endX, endY].transform.position.x;
		float endPosZ = gameScript.gridScript.grid[endX, endY].transform.position.z;
		string shipType = shipSaver.shipType;
		GameScript.ShipTypes shiptype = GameScript.ShipTypes.Mine;
		switch(shipType) {
		case("minelayer"):
			shiptype = GameScript.ShipTypes.Mine;
			break;
		case("torpedoboat"):
			shiptype = GameScript.ShipTypes.Torpedo;
			break;
		case("radarboat"):
			shiptype = GameScript.ShipTypes.Radar;
			break;
		case("kamikaze"):
			shiptype = GameScript.ShipTypes.Kamikaze;
			break;
		case("cruiser"):
			shiptype = GameScript.ShipTypes.Cruiser;
			break;
		case("destroyer"):
			shiptype = GameScript.ShipTypes.Destroyer;
			break;
		}

	

		int local = 0;
		ship = gameScript.gridScript.PlaceShip(startPosX, startPosZ, endPosX, endPosZ, local, shipSaver.player, shiptype, GameScript.PlayerType.Player1);
		shipSaver.Restore(ship, gameScript);
	}
	
	public void SignalPlayer()
	{
		string playerName = PlayerPrefs.GetString("playerName");
		networkView.RPC("RPCSignalPlayer", RPCMode.AllBuffered, playerName);
	}


	[RPC]
	void RPCSignalPlayer(string username)
	{
		Debug.Log("setup ship for " + username);
	}

	public void SetupDone(int playerType)
	{
		networkView.RPC("RPCSetupDone",RPCMode.AllBuffered,playerType);
	}

	[RPC]
	void RPCSetupDone(int playerType) {
		if (playerType == 1) {
			Debug.Log("Set 1 to true");
			gameScript.player1SetupDone = true;
		}
		if (playerType == 2) {
			Debug.Log("Set 2 to true");

			gameScript.player2SetupDone = true;
		}
	}

	public void setupAcceptance(bool accepted, GameScript.PlayerType playerType)
	{
		networkView.RPC ("RPCSetupAcceptance",RPCMode.AllBuffered,accepted,(int)playerType);

	}

	[RPC]
	void RPCSetupAcceptance(bool accepted, int playerType)
	{
		if (accepted)
		{
			if (playerType == 1) {
				gameScript.player1SetupAcceptance = true;
			} else if (playerType == 2) {
				gameScript.player2SetupAcceptance = true;
			}
		} else {
			gameScript.player1SetupAcceptance = false;
			gameScript.player2SetupAcceptance = false;
			// redo reefs.
			gridScript.refreshReef();
		}
	}

	public void EndTurn()
	{
		networkView.RPC ("RPCEndTurn",RPCMode.AllBuffered);
	}

	[RPC]
	void RPCEndTurn()
	{
		Debug.Log ("Send End Turn");
		gameScript.EndTurn();
	}

	//GRID RPC

	public void shareReefSeed(int seed)
	{
		int dd = Random.Range(1,100);
		networkView.RPC ("RPCShareReefSeed",RPCMode.AllBuffered, dd);
	}

	[RPC]
	void RPCShareReefSeed(int seed)
	{
		gridScript.setReefSeed(seed);
		Debug.Log("Seed is " + seed);
	}

	//SHIP RPC
	public void NetworkMoveShip(string shipdID, int x, int y)
	{

		networkView.RPC ("RPCMoveShip", RPCMode.AllBuffered,shipdID, x, y);
		Debug.Log ("Sent move");
	}

	[RPC]
	void RPCMoveShip(string shipID, int x, int y, NetworkMessageInfo info)
	{
		Debug.Log("Ship: "+shipID+" moved to " + x + " ," + y);


		foreach(ShipScript shipscript in gameScript.ships)
		{
			if (shipscript.shipID == shipID)
			{
				Debug.Log ("Found correct ship");
				CellScript destCellScript = gridScript.grid[x,y];
				Debug.Log("DestCell Value: " + destCellScript.gridPositionX);
				shipscript.MoveShip(destCellScript,0);

				break;
			}
		}
	}

	public void NetworkRotateShip(string shipID, bool clockwise, int steps)
	{
		networkView.RPC ("RPCRotateShip",RPCMode.AllBuffered,shipID,clockwise, steps);
	}

	[RPC]
	void RPCRotateShip(string shipID, bool clockwise, int steps)
	{
		Debug.Log("Ship: "+shipID+" Rotated: " + clockwise);
	
		foreach(ShipScript shipscript in gameScript.ships)
		{
			if (shipscript.shipID == shipID)
			{
				Debug.Log ("Found correct ship");
				shipscript.RotateShip(clockwise,0, steps);
				break;
			}
		}
	}

	public void setShip(float startPosX, float startPosZ, float endPosX, float endPosZ, string playerName, GameScript.ShipTypes shiptype, GameScript.PlayerType myType)
	{
		networkView.RPC ("RPCSetShip",RPCMode.OthersBuffered, startPosX, startPosZ, endPosX, endPosZ, playerName, (int)shiptype, (int) myType);
	}

	[RPC]
	void RPCSetShip(float startPosX, float startPosZ, float endPosX, float endPosZ, string playerName, int shipType, int myType)
	{
		gridScript.PlaceShip(startPosX,startPosZ,endPosX,endPosZ,0, playerName, (GameScript.ShipTypes) shipType, (GameScript.PlayerType) myType);
	}
	
	public void fireCannonCell(string shipID, int x, int y)
	{
		string playerName = PlayerPrefs.GetString("playerName");
		networkView.RPC ("RPCFireCannonCell",RPCMode.AllBuffered, playerName,shipID, x,y);
	}

	[RPC]
	void RPCFireCannonCell(string playerName, string shipID, int x, int y)
	{
		Debug.Log("Player: "+ playerName+ " Ship: "+shipID+" fired cannon at " + x + " ," + y);
		CellScript hitCellScript = gridScript.GetCell(x,y);
		Debug.Log(hitCellScript.gameObject.name);

		foreach(ShipScript shipscript in gameScript.ships)
		{
			if (shipscript.shipID == shipID)
			{
				Debug.Log ("Found correct ship");
				//shipscript.HandleHit(shipscript.GetSection(section),0, damage);
				shipscript.FireCannon(hitCellScript,0);
				break;
			}
		}

	}

	public void fireCannonShip(string shipID, int section, int damage)
	{
		networkView.RPC ("RPCFireCannonShip",RPCMode.AllBuffered,shipID, section, damage);
	}

	[RPC]
	void RPCFireCannonShip(string shipID, int section, int damage)
	{
		foreach(ShipScript shipscript in gameScript.ships)
		{
			if (shipscript.shipID == shipID)
			{
				Debug.Log ("Found correct ship");
				//shipscript.HandleHit(shipscript.GetSection(section),0, damage);
				shipscript.HandleCannon(shipscript.GetSection(section),0,damage);
				CellScript cellHit = shipscript.cells[section];
				gameScript.messages = "Hit ship at ("+cellHit.gridPositionX + ","+cellHit.gridPositionY+")";
				break;
			}
		}
	}

	public void fireTorpedo(string shipID) 
	{
		networkView.RPC ("RPCFireTorpedo",RPCMode.AllBuffered,shipID);
	}

	[RPC]
	void RPCFireTorpedo(string shipID)
	{
		foreach(ShipScript shipscript in gameScript.ships)
		{
			if (shipscript.shipID == shipID)
			{
				shipscript.FireTorpedo(0);
				break;
			}
		}
	}

	public void fireCannonBase(GameScript.PlayerType type, int section, int damage)
	{
		networkView.RPC ("RPCFireCannonBase",RPCMode.AllBuffered,(int)type, section, damage);
	}
	
	[RPC]
	void RPCFireCannonBase(int player, int section, int damage)
	{

		BaseScript baseScript;
		baseScript = gameScript.bases[player-1];
		baseScript.HandleCannon(baseScript.GetSection(section),0,damage);
	}
	
	public void repairShipWithIndex(string shipid, int index) 
	{
		networkView.RPC ("RPCRepairShipWithIndex",RPCMode.AllBuffered,shipid,index);
	}

	[RPC]
	void RPCRepairShipWithIndex (string shipid, int index) 
	{
		foreach (ShipScript shipscript in gameScript.ships)
		{
			if (shipscript.shipID == shipid)
			{
				shipscript.repairSection(index,0);
				break;
			}
		}
	}


	public void HandleBaseDamage(GameScript.PlayerType player, int section, int damage)
	{
		networkView.RPC ("RPCHandleBaseDamage",RPCMode.AllBuffered,(int)player,section,damage);
	}

	[RPC]
	void RPCHandleBaseDamage(int player, int section, int damage)
	{
		BaseScript baseScript;
		baseScript = gameScript.bases[player-1];
		baseScript.HandleHit(baseScript.GetSection(section),0,damage);
	}

	public void Explosion(int x, int y, GridScript.ExplodeType type)
	{
		networkView.RPC ("RPCExplosion",RPCMode.AllBuffered,x,y,(int)type);
	}

	[RPC]
	void RPCExplosion(int x, int y, int type)
	{

		gridScript.Explode(x,y,(GridScript.ExplodeType)type);
	}

	public void DetonateKamikaze(int x, int y, string shipid)
	{
		networkView.RPC ("RPCDetonateKamikaze",RPCMode.AllBuffered,x,y,shipid);
	}

	[RPC]
	void RPCDetonateKamikaze(int x, int y, string shipid)
	{
		CellScript targetCell = gridScript.grid[x, y];
		foreach (ShipScript shipscript in gameScript.ships)
		{
			if (shipscript.shipID == shipid)
			{
				shipscript.Detonate(targetCell,0);
				break;
			}
		}
	}

	public void PlaceMine(int x, int y)
	{
		networkView.RPC ("RPCPlaceMine",RPCMode.AllBuffered,x,y);
	}

	[RPC]
	void RPCPlaceMine(int x, int y) {
		gridScript.PlaceMine(x, y);
	}

	public void RemoveMine(int x, int y)
	{
		networkView.RPC ("RPCRemoveMine",RPCMode.AllBuffered,x,y);
	}

	[RPC]
	void RPCRemoveMine(int x, int y) {
		gridScript.RemoveMine(x, y);
	}


	// Update is called once per frame
	void Update () {
	
	}
}
