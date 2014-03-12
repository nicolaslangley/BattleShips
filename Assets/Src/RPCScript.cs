using UnityEngine;
using System.Collections;

public class RPCScript : MonoBehaviour {

	// Use this for initialization

	public GridScript gridScript;
	private GameObject system;


	void Awake()
	{
		system = GameObject.FindGameObjectWithTag("System");
		gridScript = system.GetComponent<GridScript>();
	}

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
	public void MoveShip(string shipID, int x, int y)
	{
		networkView.RPC ("RPCMoveShip", RPCMode.All, shipID, x, y);
	}

	[RPC]
	void RPCHMoveShip(string shipID, int x, int y)
	{
		Debug.Log("Ship: "+shipID+" moved to " + x + " ," + y);
	}

	public void setShip(float startPosX, float startPosZ, float endPosX, float endPosZ)
	{
		networkView.RPC ("RPCSetShip",RPCMode.Others, startPosX, startPosZ, endPosX, endPosZ);
	}

	[RPC]
	void RPCSetShip(float startPosX, float startPosZ, float endPosX, float endPosZ)
	{
		gridScript.PlaceShip(startPosX,startPosZ,endPosX,endPosZ);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
