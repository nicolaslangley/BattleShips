
using UnityEngine;
using System.Collections;

public class GameLobbyScript : MonoBehaviour
{

    private bool launchingGame = false;
    private bool showMenu = false;
	private int launchCount = 0;

    private ArrayList playerList = new ArrayList();


    public class PlayerInfo
    {
        public string username;
        public NetworkPlayer player;
        public int UID;
		public int played;
		public int won;
    }

    private int serverMaxPlayers = 4;
    private string serverTitle = "Loading..";
    private bool serverPasswordProtected = false;

    private string playerName = "";
    private int playerUID = 0;
	private int playerPlayed = 0;
	private int playerWon = 0;

    private MainMenuScript mainMenuScript;
    void Awake()
    {
        showMenu = false;
    }
    void Start()
    {
        mainMenuScript = MainMenuScript.SP;
    }
    public void EnableLobby()
    {
        playerName = PlayerPrefs.GetString("playerName");
        playerUID = PlayerPrefs.GetInt("UID");
		playerPlayed = PlayerPrefs.GetInt ("Played");
		playerWon = PlayerPrefs.GetInt("Won");

        lastRegTime = Time.time - 3600;

        launchingGame = false;
        showMenu = true;

        LobbyChatScript chat = GetComponent<LobbyChatScript>() as LobbyChatScript;
        chat.ShowChatWindow();
    }
    void OnGUI()
    {
        if (!showMenu)
        {
            return;
        }


        //Back to main menu
        if (GUI.Button(new Rect(40, 10, 150, 20), "Back to main menu"))
        {
            StartCoroutine(leaveLobby());
        }

        if (launchingGame && launchCount == 0)
        {	
			launchCount = 1;
    		launchingGameGUI();

        }
        else if (!Network.isServer && !Network.isClient)
        {
            //First set player count, server name and password option			
            hostSettings();

        }
        else
        {
            //Show the lobby		
            showLobby();
        }
    }
    IEnumerator leaveLobby()
    {
        //Disconnect fdrom host, or shotduwn host
        if (Network.isServer || Network.isClient)
        {
            if (Network.isServer)
            {
                MultiplayerFunctionScript.SP.UnregisterHost();
            }
            Network.Disconnect();
            yield return new WaitForSeconds(0.3f);
        }

        LobbyChatScript chat = GetComponent<LobbyChatScript>() as LobbyChatScript;
        chat.CloseChatWindow();

        mainMenuScript.OpenMenu("multiplayer");

        showMenu = false;
    }
    private string hostSetting_title = "No server title";
    private int hostSetting_players = 2;
    private string hostSetting_password = "";
    void hostSettings()
    {

        GUI.BeginGroup(new Rect(Screen.width / 2 - 175, Screen.height / 2 - 75 - 50, 350, 150));
        GUI.Box(new Rect(0, 0, 350, 150), "Server options");

        GUI.Label(new Rect(10, 20, 150, 20), "Server title");
        hostSetting_title = GUI.TextField(new Rect(175, 20, 160, 20), hostSetting_title);

        GUI.Label(new Rect(10, 40, 150, 20), "Max. players (2-4)");
        hostSetting_players = int.Parse(GUI.TextField(new Rect(175, 40, 160, 20), hostSetting_players + ""));

        GUI.Label(new Rect(10, 60, 150, 50), "Password\n");
        hostSetting_password = (GUI.TextField(new Rect(175, 60, 160, 20), hostSetting_password));


        if (GUI.Button(new Rect(100, 115, 150, 20), "Go to lobby"))
        {
            StartHost(hostSetting_password, int.Parse(hostSetting_players + ""), hostSetting_title);
        }
        GUI.EndGroup();
    }
    void StartHost(string password, int players, string serverName)
    {
        if (players < 1)
        {
            players = 1;
        }
        if (players >= 32)
        {
            players = 32;
        }
        serverTitle = serverName;
		MultiplayerFunctionScript.SP.StartServer(password, MultiplayerFunctionScript.SP.defaultServerPort, (players - 1), true);
    }
    void showLobby()
    {
        string players = "";
        int currentPlayerCount = 0;
        foreach (var playerInstance in playerList)
        {
			PlayerInfo p = (playerInstance as PlayerInfo);
            players = p.username + " ("+p.won+"/" + p.played+") \n" + players;
            currentPlayerCount++;
        }

        GUI.BeginGroup(new Rect(Screen.width / 2 - 200, Screen.height / 2 - 200, 400, 180));
        GUI.Box(new Rect(0, 0, 400, 200), "Game lobby");


        string pProtected = "no";
        if (serverPasswordProtected)
        {
            pProtected = "yes";
        }
        GUI.Label(new Rect(10, 20, 150, 20), "Password protected");
        GUI.Label(new Rect(150, 20, 100, 100), pProtected);

        GUI.Label(new Rect(10, 40, 150, 20), "Server title");
        GUI.Label(new Rect(150, 40, 100, 100), serverTitle);

        GUI.Label(new Rect(10, 60, 150, 20), "Players");
        GUI.Label(new Rect(150, 60, 100, 100), currentPlayerCount + "/" + serverMaxPlayers);

        GUI.Label(new Rect(10, 80, 150, 20), "Current players");
        GUI.Label(new Rect(150, 80, 100, 100), players);


        if (Network.isServer)
        {
			if (currentPlayerCount == 2) {
				if (GUI.Button(new Rect(25, 140, 150, 20), "Start the game"))
				{
					HostLaunchGame();
				}
			} else {
				GUI.Label(new Rect(25, 140, 200, 40), "Waiting for 2 Players");

			}
        }
        else
        {
            GUI.Label(new Rect(25, 140, 200, 40), "Waiting for the server to start the game..");
        }

        GUI.EndGroup();
    }
    void OnConnectedToServer()
    {
        //Called on client
        //Send everyone this clients data
        playerList = new ArrayList();


        playerName = PlayerPrefs.GetString("playerName");
		playerUID = PlayerPrefs.GetInt("UID");
		playerPlayed = PlayerPrefs.GetInt ("Played");
		playerWon = PlayerPrefs.GetInt("Won");
	
        networkView.RPC("addPlayer", RPCMode.AllBuffered, Network.player, playerName, playerWon, playerPlayed);
    }
    void OnServerInitialized()
    {
        //Called on host
        //Add hosts own data to the playerlist	
        playerList = new ArrayList();

		networkView.RPC("addPlayer", RPCMode.AllBuffered, Network.player, playerName, playerWon, playerPlayed);

        bool pProtected = false;
        if (Network.incomingPassword != null && Network.incomingPassword != "")
        {
            pProtected = true;
        }
        int maxPlayers = Network.maxConnections + 1;

        networkView.RPC("setServerSettings", RPCMode.AllBuffered, pProtected, maxPlayers, hostSetting_title);

    }
    float lastRegTime = -60;
    void Update()
    {
        if (Network.isServer && lastRegTime < Time.time - 60)
        {
            lastRegTime = Time.time;
			MultiplayerFunctionScript.SP.RegisterHost(hostSetting_title, "No description");
        }
    }
    [RPC]
    void setServerSettings(bool password, int maxPlayers, string newSrverTitle)
    {
        serverMaxPlayers = maxPlayers;
        serverTitle = newSrverTitle;
        serverPasswordProtected = password;
    }
    void OnPlayerDisconnected(NetworkPlayer player)
    {
        //Called on host
        //Remove player information from playerlist
        networkView.RPC("playerLeft", RPCMode.All, player);

        LobbyChatScript chat = GetComponent<LobbyChatScript>() as LobbyChatScript;
        chat.addGameChatMessage("A player left the lobby");
    }
    [RPC]
    void addPlayer(NetworkPlayer player, string username, int won, int played)
    {

		NetworkViewID newViewID = Network.AllocateViewID();

        PlayerInfo playerInstance = new PlayerInfo();
        playerInstance.player = player;
        playerInstance.username = username;
		playerInstance.played = played;
		playerInstance.won = won;
        playerList.Add(playerInstance);
    }
    [RPC]
    void playerLeft(NetworkPlayer player)
    {

        PlayerInfo deletePlayer = null;

        foreach (PlayerInfo playerInstance in playerList)
        {
            if (player == playerInstance.player)
            {
                deletePlayer = playerInstance;
            }
        }
        playerList.Remove(deletePlayer);
        Network.RemoveRPCs(player);
        Network.DestroyPlayerObjects(player);
    }

    void HostLaunchGame()
    {
        if (!Network.isServer)
        {
            return;
        }

        // Don't allow any more players
        Network.maxConnections = -1;
		MultiplayerFunctionScript.SP.UnregisterHost();
        networkView.RPC("launchGame", RPCMode.All);
    }
    [RPC]
    void launchGame()
    {
        Network.isMessageQueueRunning = false;
        launchingGame = true;
    }
    void launchingGameGUI()
    {
        //Show loading progress, ADD LOADINGSCREEN?
        GUI.Box(new Rect(Screen.width / 4 + 180, Screen.height / 2 - 30, 280, 50), "");
		GUI.Label(new Rect(Screen.width / 4 + 200, Screen.height / 2 - 25, 285, 150), "Loaded, starting the game!");
		//Debug.Log ("Loading game GUI");

        if (Application.CanStreamedLevelBeLoaded("GridTest"))
        {
            GUI.Label(new Rect(Screen.width / 4 + 200, Screen.height / 2 - 25, 285, 150), "Loaded, starting the game!");
			Application.LoadLevel("GridTest");
        }
        else
        {
            GUI.Label(new Rect(Screen.width / 4 + 200, Screen.height / 2 - 25, 285, 150), "Starting..Loading the game: " + Mathf.Floor(Application.GetStreamProgressForLevel((Application.loadedLevel + 1)) * 100) + " %");
        }

    }
}