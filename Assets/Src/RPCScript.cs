using UnityEngine;
using System.Collections;

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

	public void EndTurn()
	{
		networkView.RPC ("RPCEndTurn",RPCMode.All);
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

		networkView.RPC ("RPCMoveShip", RPCMode.All,shipdID, x, y);
		Debug.Log ("Sent move");
	}

	[RPC]
	void RPCMoveShip(string shipID, int x, int y, NetworkMessageInfo info)
	{
		Debug.Log("Ship: "+shipID+" moved to " + x + " ," + y);


		foreach(GameObject obj in gameScript.ships)
		{
			ShipScript shipscript = obj.GetComponent<ShipScript>();
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

	public void NetworkRotateShip(string shipID, bool clockwise)
	{
		networkView.RPC ("RPCRotateShip",RPCMode.AllBuffered,shipID,clockwise);
	}

	[RPC]
	void RPCRotateShip(string shipID, bool clockwise)
	{
		Debug.Log("Ship: "+shipID+" Rotated: " + clockwise);
	
		foreach(GameObject obj in gameScript.ships)
		{
			ShipScript shipscript = obj.GetComponent<ShipScript>();
			if (shipscript.shipID == shipID)
			{
				Debug.Log ("Found correct ship");
				shipscript.RotateShip(clockwise,0);
				break;
			}
		}
	}

	public void setShip(float startPosX, float startPosZ, float endPosX, float endPosZ, string playerName)
	{
		networkView.RPC ("RPCSetShip",RPCMode.OthersBuffered, startPosX, startPosZ, endPosX, endPosZ, playerName);
	}

	[RPC]
	void RPCSetShip(float startPosX, float startPosZ, float endPosX, float endPosZ, string playerName)
	{
		gridScript.PlaceShip(startPosX,startPosZ,endPosX,endPosZ,0, playerName);
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

		if (hitCellScript.curCellState != GameScript.CellState.Available)
		{
			Debug.Log ("Hit Something at " +x + ", " + y);
			gameScript.messages = "Something hit at "+x+", "+y;
		} else {
			Debug.Log ("HIt Nothing");
			gameScript.messages = "Hit Nothing";
		}

//		if (hitShipScript != null)
//		{
//			ShipScript hitShipScript = hitCellScript.occupier.GetComponent<ShipScript>();
//			//int index = hitShipScript.cells.IndexOf(hitCell);
//			//Debug.Log("Index of hit ship: "+ index);
//		} else {
//			Debug.Log("HIt nothin");
//		}
	}

	public void fireCannonShip(string shipID, int section, int damage)
	{
		networkView.RPC ("RPCFireCannonShip",RPCMode.AllBuffered,shipID, section, damage);
	}

	[RPC]
	void RPCFireCannonShip(string shipID, int section, int damage)
	{
		foreach(GameObject obj in gameScript.ships)
		{
			ShipScript shipscript = obj.GetComponent<ShipScript>();
			if (shipscript.shipID == shipID)
			{
				Debug.Log ("Found correct ship");
				shipscript.HandleHit(shipscript.GetSection(section),0, damage);
				gameScript.messages = "Hit ship with ID: " + shipID;

				break;
			}
		}
	}

	// Update is called once per frame
	void Update () {
	
	}
}
