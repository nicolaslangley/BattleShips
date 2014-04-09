using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameScript : MonoBehaviour {

	/** Enums **/
	public enum GameState {Connecting, Setup,SetupWaiting, Play, Wait, End};
	public enum Direction {North, East, South, West};
	public enum CellState {Available, Mine, MineRadius, Reef, Ship, Base};
	public enum PlayAction {Move, Cannon, Torpedo, DropMine, PickupMine, Detonate, Repair, None};
	public enum ShipTypes {Cruiser, Destroyer, Torpedo, Mine, Radar, Kamikaze};


	public enum PlayerType {None = 0, Player1 =1, Player2=2}
	/** Properties **/
	public List<ShipScript> ships;
	public List<BaseScript> bases;
	public GridScript gridScript;
	public ShipScript selectedShip;
	public BaseScript selectedBase;
	public RPCScript rpcScript;
	public bool existSelection;

	public string myname;
	public string opponentname;
	public string turn;

	public PlayerType myPlayerType;
	public PlayerType winner;

	public string messages;

	public bool player1SetupDone;
	public bool player2SetupDone;

	public bool player1SetupAcceptance;
	public bool player2SetupAcceptance;

	private bool setupAccepted;
	private bool isLoadedGame;

	private string loadFilePath;
	

	public bool waitTurn;
	

	private bool gridInited;

	private int myCruiserCount;
	private int myDestroyerCount;
	private int myMineLayerCount;
	private int myTorpedoCount;
	private int myRadarCount;
	private int myKamikazeCount;


	/** Current state of game **/
	public GameState curGameState;
	public PlayAction curPlayAction;



	/** UNITY METHODS **/


	void Awake()
	{
		//RE-enable the network messages now we've loaded the right level
		Network.isMessageQueueRunning = true;
		DontDestroyOnLoad(networkView);
	}

	// Use this for initialization
	void Start () {
		gridScript = gameObject.GetComponent<GridScript>();
		rpcScript = gameObject.GetComponent<RPCScript>();

		// Initialize game state variables
		curPlayAction = PlayAction.None;
		curGameState = GameState.Connecting;
		// Run game initialization
		gridInited = false;
		existSelection = false;

		string playerName = PlayerPrefs.GetString("playerName");
		myname = playerName;
		rpcScript.sendPlayerName(myname);
		setPlayerType();

		int LoadedInt = PlayerPrefs.GetInt("LoadedGame");

		if (LoadedInt == 1) {
			//This is a loaded game
			//isLoadedGame = true;
			//Send RPC to say its a loaded game.

			//Change current game state.
			isLoadedGame = true;
			loadFilePath = PlayerPrefs.GetString("LoadedGamePath");
			Debug.Log(loadFilePath);
		}


		player1SetupDone = false;
		player2SetupDone = false;
		player1SetupAcceptance = false;
		player2SetupAcceptance = false;

		setupAccepted = false;

		myRadarCount = 1;
		myKamikazeCount = 1;
		myCruiserCount = 2;
		myDestroyerCount = 3;
		myTorpedoCount = 2;
		myMineLayerCount = 2;
	}
	
	// Update is called once per frame
	void Update () {
		switch (curGameState) {

		case (GameState.Connecting):
			if (!string.IsNullOrEmpty(opponentname) && !string.IsNullOrEmpty(myname)) {
				setPlayerType();
				curGameState = GameState.Setup;
			}
			break;
		case (GameState.Setup):
			// Perform update to objects based on setup state
			if (!gridInited) {
				gridInited = true;
				gridScript.Init();

				if (isLoadedGame) {
					//Load game
					//GameSaverScript.Load("String",this);
				}

			}

			if (player1SetupAcceptance && player2SetupAcceptance)
			{
				setupAccepted = true;
			}

			if (myCruiserCount == 0 
			    && myDestroyerCount == 0
			    && myMineLayerCount == 0
			    && myKamikazeCount == 0
			    && myRadarCount == 0
			    && myTorpedoCount == 0) 
			{
				setPlayerType();
				
				rpcScript.SetupDone((int)myPlayerType);
				curGameState = GameState.SetupWaiting;
				Debug.Log ("Moving to SetupWait state");

			}
			break;

		case (GameState.SetupWaiting):
			if (player1SetupDone && player2SetupDone) {
				Debug.Log("Both true");
				bases[((int)myPlayerType - 1)].DisplayDockingRegion(false);
				player1SetupDone = false;
				player2SetupDone = false;
				if (Network.peerType == NetworkPeerType.Server)
				{
					curGameState = GameState.Play;
					turn = myname;
				} else {
					curGameState = GameState.Wait;
					turn = opponentname;
				}
				Debug.Log ("It is now "+ turn);

			}
			break;
		case (GameState.Play):
			// Perform update to objects based on play state
			gridScript.ResetVisibility();
			foreach (ShipScript s in ships) {
				s.CustomPlayUpdate();
			}
			foreach (ShipScript s in ships) {
				s.UpdateShipVisibility();
			}
			break;
		case (GameState.Wait):
			gridScript.ResetVisibility();
			foreach (ShipScript s in ships) {
				s.CustomPlayUpdate();
			}
			break;
		case (GameState.End):
			Debug.Log("In end state");
			break;
		}
	}

	public void NotifyDetonation(string weapon, CellScript cell) {
		string message = "Hit nothing";
		if (cell != null) {
			switch (cell.curCellState) {
			case CellState.Available:
				message = "Hit nothing";
				break;
			case CellState.Base:
				message = "Hit player base";
				break;
			case CellState.Mine:
				message = "Mine Hit... this shouldn't happen";
				break;
			case CellState.Ship:
				message = "Hit Ship";
				break;
			case CellState.Reef:
				message = "Hit reef";
				break;
			}
			message += " by a " + weapon + " at location (" + cell.gridPositionX + "," + cell.gridPositionY + ")";
			Debug.Log (message);
		}
		else Debug.Log ("Notify Detonation was passed a null cell.");
		GlobalNotify (message);
	}

	public void GlobalNotify(string message) {
		this.messages = message;
	}

	// Display GUI overlay for game
	void OnGUI () {
		GUI.Label(new Rect(150, 50, 100, 100), "Player turn: "+turn);
		GUI.Label(new Rect(100, 250, 100, 100), messages);

		switch(curGameState) {
		case (GameState.Setup):
			//Debug.Log ("-----------Player type: " + ((int)myPlayerType-1));
			if (bases.Count > 0) bases[((int)myPlayerType - 1)].DisplayDockingRegion(true);
			// GUI to be displayed during setup phase
			if (GUI.Button(new Rect(10, 10, 100, 30), "Play Game")) {
				setPlayerType();

				rpcScript.SetupDone((int)myPlayerType);
				curGameState = GameState.SetupWaiting;
				Debug.Log ("Moving to SetupWait state");
//				if (Network.peerType == NetworkPeerType.Server)
//				{
//					curGameState = GameState.Play;
//					turn = myname;
//				} else {
//					curGameState = GameState.Wait;
//					turn = opponentname;
//				}
				Debug.Log ("It is now "+ turn);

			}
//			if (GUI.Button (new Rect(10, 50, 100, 30), "Place Ship")) {
//				gridScript.setShip();
//				Debug.Log ("Placing a Ship on selected square");
//				rpcScript.SignalPlayer();
//			}

			if (!player1SetupAcceptance || !player2SetupAcceptance) {
				if (GUI.Button (new Rect(10,90,100,30), "Accept Setup." )) {
					rpcScript.setupAcceptance(true,myPlayerType);
				}
				if (GUI.Button (new Rect(10,130,100,30), "Reject Setup")) {
					rpcScript.setupAcceptance(false,myPlayerType);
				}
			}
					
			if (myCruiserCount > 0 && setupAccepted) {
				GUI.Label (new Rect(120,90,100,30),"Remaining: " + myCruiserCount);
				if (GUI.Button (new Rect(10,90,100,30), "Place Cruiser." )) {
					if (gridScript.currentSelection.Count == 1) {
						gridScript.setShip(ShipTypes.Cruiser);
						rpcScript.SignalPlayer();
						myCruiserCount--;
					}
				}
			}

			if (myDestroyerCount > 0 && setupAccepted) {
				GUI.Label (new Rect(120,130,100,30),"Remaining: " + myDestroyerCount);
				if (GUI.Button (new Rect(10,130,100,30), "Place Destroyer")) {
					if (gridScript.currentSelection.Count == 1) {
						gridScript.setShip(ShipTypes.Destroyer);
						rpcScript.SignalPlayer();
						myDestroyerCount--;
					}

				}
			}

			if (myMineLayerCount > 0 && setupAccepted) {
				GUI.Label (new Rect(120,170,100,30),"Remaining: " + myMineLayerCount);

				if (GUI.Button (new Rect(10,170,100,30), "Place MineLayer")) {
					if (gridScript.currentSelection.Count == 1) {
						gridScript.setShip(ShipTypes.Mine);
						rpcScript.SignalPlayer();
						myMineLayerCount--;
					}
				}
			}

			if (myTorpedoCount > 0 && setupAccepted) {
				GUI.Label (new Rect(120,210,100,30),"Remaining: " + myTorpedoCount);

				if (GUI.Button (new Rect(10,210,100,30), "Place Torpedo")) {
					if (gridScript.currentSelection.Count == 1) {
						gridScript.setShip(ShipTypes.Torpedo);
						rpcScript.SignalPlayer();
						myTorpedoCount--;
					}
				
				}
			}

			if (myRadarCount > 0 && setupAccepted) {
				GUI.Label (new Rect(120,250,100,30),"Remaining: " + myRadarCount);

				if (GUI.Button (new Rect(10,250,100,30), "Place Radar")) {
					if (gridScript.currentSelection.Count == 1) {
						gridScript.setShip(ShipTypes.Radar);
						rpcScript.SignalPlayer();
						myRadarCount--;
					}

				}
			}
	
			if (myKamikazeCount > 0 && setupAccepted) {
				GUI.Label (new Rect(120,290,100,30),"Remaining: " + myKamikazeCount);

				if (GUI.Button (new Rect(10,290,100,30), "Place Kamikaze")) {
					if (gridScript.currentSelection.Count == 1) {
						gridScript.setShip(ShipTypes.Kamikaze);
						rpcScript.SignalPlayer();
						myKamikazeCount--;
					}
				

				}
			}
	
			break;

		case (GameState.SetupWaiting):
			break;
		case (GameState.Play):
			// GUI to be displayed during playing phase
			break;

		case (GameState.End):
			int didwin = 0;
			if (myPlayerType == winner) {
				GUI.Label(new Rect(200, 20, 100, 100), "WINNER IS: "+myname);

				didwin = 1;
			} else {
				GUI.Label(new Rect(200, 20, 100, 100), "WINNER IS: "+opponentname);
				didwin = 0;
			}


			if (GUI.Button(new Rect(Screen.width - 110, 300, 100, 30), "Leave")) {
				int uid = PlayerPrefs.GetInt("UID");

				int played = PlayerPrefs.GetInt("Played");
				int won = PlayerPrefs.GetInt("Won");

				PlayerPrefs.SetInt("Played",played+1);
				PlayerPrefs.SetInt("Won",won+didwin);

				WWWForm form = new WWWForm();
				form.AddField("UID", uid);
				form.AddField("didWin", didwin);
				WWW w = new WWW("http://battlefield361.dx.am/updateStat.php", form);
				StartCoroutine(updateStat(w));


			}
			
			break;
		}
	}
	
	public void setPlayerType(){
		//set alphabetically.
		Debug.Log(myname + "////"+ opponentname);
		if (string.Compare(myname,opponentname) < 0) {
			myPlayerType = PlayerType.Player1;
		} else {
			myPlayerType = PlayerType.Player2;
		}
	}

	public void EndTurn()
	{
		ResetTotalSelection();
		// Perform one last update to visibility before ending turn
		gridScript.ResetVisibility();
		foreach (ShipScript s in ships) {
			s.CustomPlayUpdate();
		}
		foreach (ShipScript s in ships) {
			s.UpdateShipVisibility();
		}


		winner = checkWinner();
		if (winner != PlayerType.None) {
			//Debug.Log("winner is +" winner);
			Debug.Log("Winner decided");
			
			curGameState = GameState.End;
		}


		if (curGameState == GameState.Wait)
		{
			curGameState = GameState.Play;
			turn = myname;

		} else if (curGameState == GameState.Play) {
			curGameState = GameState.Wait;
			turn = opponentname;
		}


		Debug.Log ("It is now "+ turn);
	}

	public void Reload(GameScript gameScript) {
		//Reset this gamescript from a previously saved one.
		//Do nothing for now.
	}

	public PlayerType checkWinner() {
		bool player1HasShips = false;
		bool player2HasShips = false;
		Debug.Log("Checking win conditions");

		foreach (ShipScript s in ships) {
			if (s.myPlayerType == GameScript.PlayerType.Player1) {
				player1HasShips = true;
			} else if (s.myPlayerType == GameScript.PlayerType.Player2) {
				player2HasShips = true;
			}
		}
		if (player1HasShips && player2HasShips) return GameScript.PlayerType.None;
		if (!player2HasShips) return GameScript.PlayerType.Player1;
		if (!player1HasShips) return GameScript.PlayerType.Player2;
		return GameScript.PlayerType.None;
	}

	public void ResetTotalSelection() {
		foreach (ShipScript s in ships) {
			s.selected = false;
		}
		foreach (BaseScript b in bases) {
			b.selected = false;
		}
		gridScript.ResetSelection();
		existSelection = false;
	}

	private void LoadGame() {

	}


	IEnumerator updateStat(WWW w)
	{
		yield return w;
		if (w.error == null)
		{
			if (w.text == "SuccUpdate") {
				Debug.Log("Success update");
				if (Application.CanStreamedLevelBeLoaded("lobby"))
				{
					GUI.Label(new Rect(Screen.width / 4 + 200, Screen.height / 2 - 25, 285, 150), "Going back to Main Menu.");
					Application.LoadLevel("lobby");
					Destroy(GameObject.Find("Persistent"));
					Destroy(gameObject);

				}
			} else {
				Debug.Log("Fail update");

			}

		}
		else
		{
			Debug.Log (w.error);
		}
	}
	
	
	
}
