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
	[RPC]


	void OnConnectedToServer()
	{
		//Called on client
		//Send everyone this clients data
		string playerName = PlayerPrefs.GetString("playerName");
		networkView.RPC("addPlayer", RPCMode.AllBuffered, Network.player, playerName);
	}

	[RPC]
	public void addPlayer(NetworkPlayer player, string username)
	{
		Debug.Log("got addplayer" + username);
	}
	
	public void SignalPlayer()
	{
		string playerName = PlayerPrefs.GetString("playerName");
		networkView.RPC("RPCSignalPlayer", RPCMode.All, playerName);
	}

	[RPC]
	void RPCSignalPlayer(string username)
	{
		Debug.Log("setup ship for " + username);
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

		networkView.RPC ("RPCMoveShip", RPCMode.OthersBuffered,shipdID, x, y);
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
				GameObject destCellObject = gridScript.grid[x,y];
				CellScript destCell = destCellObject.GetComponent<CellScript>();
				Debug.Log("DestCell Value: " + destCell.gridPositionX);
				shipscript.MoveShip(destCell,0);

				break;
			}
		}


	}

	public void setShip(float startPosX, float startPosZ, float endPosX, float endPosZ)
	{
		networkView.RPC ("RPCSetShip",RPCMode.Others, startPosX, startPosZ, endPosX, endPosZ);
	}

	[RPC]
	void RPCSetShip(float startPosX, float startPosZ, float endPosX, float endPosZ)
	{
		gridScript.PlaceShip(startPosX,startPosZ,endPosX,endPosZ,0);
	}
	
	public void fireCannon(string shipID, int x, int y)
	{
		string playerName = PlayerPrefs.GetString("playerName");
		networkView.RPC ("RPCFireCannon",RPCMode.Others, playerName,shipID, x,y);
	}

	[RPC]
	void RPCFireCannon(string playerName, string shipID, int x, int y)
	{
		Debug.Log("Player: "+ playerName+ " Ship: "+shipID+" fired cannon at " + x + " ," + y);
	}

	// Update is called once per frame
	void Update () {
	
	}
}
