﻿using UnityEngine;
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

	public void NetworkRotateShip(string shipID, bool clockwise)
	{
		networkView.RPC ("RPCRotateShip",RPCMode.OthersBuffered,shipID,clockwise);
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
		networkView.RPC ("RPCSetShip",RPCMode.Others, startPosX, startPosZ, endPosX, endPosZ, playerName);
	}

	[RPC]
	void RPCSetShip(float startPosX, float startPosZ, float endPosX, float endPosZ, string playerName)
	{
		gridScript.PlaceShip(startPosX,startPosZ,endPosX,endPosZ,0, playerName);
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
