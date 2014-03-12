using UnityEngine;
using System.Collections;

public class RPCScript : MonoBehaviour {

	// Use this for initialization



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

	public void MoveShip(string shipID, int x, int y)
	{
		networkView.RPC ("RPCMoveShip", RPCMode.All, shipID, x, y);
	}

	[RPC]
	void RPCHMoveShip(string shipID, int x, int y)
	{
		Debug.Log("Ship: "+shipID+" moved to " + x + " ," + y);
	}

	[RPC]
	void hitts()
	{
		Debug.Log("hit");
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
