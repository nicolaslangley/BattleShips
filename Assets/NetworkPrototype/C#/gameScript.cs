using UnityEngine;
using System.Collections;

public class gameScript: MonoBehaviour
{
    void Awake()
    {
        //RE-enable the network messages now we've loaded the right level
        Network.isMessageQueueRunning = true;
		DontDestroyOnLoad(networkView);
    }
    void OnGUI()
    {

        if (Network.peerType == NetworkPeerType.Disconnected)
        {
            //We are currently disconnected: Not a client or host
            GUILayout.Label("Connection status: We've (been) disconnected");
            if (GUILayout.Button("Back to main menu"))
            {
                Application.LoadLevel((Application.loadedLevel - 1));
            }

        }
        else
        {
            //We've got a connection(s)!


            if (Network.peerType == NetworkPeerType.Connecting)
            {

                GUILayout.Label("Connection status: Connecting");

            }
            else if (Network.peerType == NetworkPeerType.Client)
            {

                GUILayout.Label("Connection status: Client!");
                GUILayout.Label("Ping to server: " + Network.GetAveragePing(Network.connections[0]));

            }
            else if (Network.peerType == NetworkPeerType.Server)
            {

                GUILayout.Label("Connection status: Server!");
                GUILayout.Label("Connections: " + Network.connections.Length);
                if (Network.connections.Length >= 1)
                {
                    GUILayout.Label("Ping to first player: " + Network.GetAveragePing(Network.connections[0]));
                }
            }

            if (GUILayout.Button("Disconnect"))
            {
                Network.Disconnect();
            }

			if (GUI.Button(new Rect(40, 10, 150, 20), "Signal"))
			{
				string playerName = PlayerPrefs.GetString("playerName");
				networkView.RPC("signalPlayer", RPCMode.AllBuffered, playerName);
			}
        }
        
    }

	[RPC]
	void signalPlayer(string myName)
	{
		Debug.Log("Hit" + myName);
	}

    //Client&Server
    void OnDisconnectedFromServer(NetworkDisconnection info)
    {
        if (Network.isServer)
        {
            Debug.Log("Local server connection disconnected");
        }
        else
        {
            if (info == NetworkDisconnection.LostConnection)
                Debug.Log("Lost connection to the server");
            else
                Debug.Log("Successfully diconnected from the server");
        }
    }
    //Server functions called by Unity
    void OnPlayerConnected(NetworkPlayer player)
    {
        Debug.Log("Player connected from: " + player.ipAddress + ":" + player.port);
    }

    void OnPlayerDisconnected(NetworkPlayer player)
    {
        Debug.Log("Player disconnected from: " + player.ipAddress + ":" + player.port);

    }
}