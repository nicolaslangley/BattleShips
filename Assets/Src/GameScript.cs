using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameScript : MonoBehaviour {

	/** Enums **/
	public enum GameState {Connecting, Setup,SetupWaiting, Play, Wait, End};
	public enum Direction {North, East, South, West};
	public enum CellState {Available, Mine, Reef, Ship, Base};
	public enum PlayAction {Move, Cannon, Torpedo, DropMine, PickupMine, Repair, None};
	public enum ShipTypes {Cruiser, Destroyer, Torpedo, Mine, Radar, Kamikaze};


	public enum PlayerType {None = 0, Player1 =1, Player2=2}
	/** Properties **/
	public List<ShipScript> ships;
	public List<BaseScript> bases;
	public GridScript gridScript;
	public ShipScript selectedShip;
	public RPCScript rpcScript;
	
	public string myname;
	public string opponentname;
	public string turn;

	public PlayerType myPlayerType;

	public string messages;

	public bool player1SetupDone;
	public bool player2SetupDone;

	public bool waitTurn;

	private bool gridInited;

	private int myCruiserCount;
	private int myDestoryerCount;
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

		string playerName = PlayerPrefs.GetString("playerName");
		myname = playerName;
		rpcScript.sendPlayerName(myname);
		setPlayerType();

		player1SetupDone = false;
		player2SetupDone = false;

		myRadarCount = 1;
		myKamikazeCount = 1;
		myCruiserCount = 2;
		myDestoryerCount = 3;
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
			}
			gridScript.CustomSetupUpdate();
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
			break;
		}
	}

	// Display GUI overlay for game
	void OnGUI () {
		GUI.Label(new Rect(150, 50, 100, 100), "Player turn: "+turn);
		GUI.Label(new Rect(100, 250, 100, 100), messages);

		switch(curGameState) {
		case (GameState.Setup):
			Debug.Log ("-----------Player type: " + ((int)myPlayerType-1));
			bases[((int)myPlayerType - 1)].DisplayDockingRegion(true);
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

			if (myCruiserCount > 0) {
				GUI.Label (new Rect(120,90,100,30),"Remaining: " + myCruiserCount);
				if (GUI.Button (new Rect(10,90,100,30), "Place Cruiser." )) {
					gridScript.setShip(ShipTypes.Cruiser);
					rpcScript.SignalPlayer();
					myCruiserCount--;
				}
			}

			if (myDestoryerCount > 0) {
				GUI.Label (new Rect(120,130,100,30),"Remaining: " + myDestoryerCount);
				if (GUI.Button (new Rect(10,130,100,30), "Place Destoryer")) {
					gridScript.setShip(ShipTypes.Destroyer);
					rpcScript.SignalPlayer();
					myDestoryerCount--;
				}
			}

			if (myMineLayerCount > 0) {
				GUI.Label (new Rect(120,170,100,30),"Remaining: " + myMineLayerCount);

				if (GUI.Button (new Rect(10,170,100,30), "Place MineLayer")) {
					gridScript.setShip(ShipTypes.Mine);
					rpcScript.SignalPlayer();
					myMineLayerCount--;
				}
			}

			if (myTorpedoCount > 0) {
				GUI.Label (new Rect(120,210,100,30),"Remaining: " + myTorpedoCount);

				if (GUI.Button (new Rect(10,210,100,30), "Place Torpedo")) {
					gridScript.setShip(ShipTypes.Torpedo);
					rpcScript.SignalPlayer();
					myTorpedoCount--;
				}
			}

			if (myRadarCount > 0) {
				GUI.Label (new Rect(120,250,100,30),"Remaining: " + myRadarCount);

				if (GUI.Button (new Rect(10,250,100,30), "Place Radar")) {
					gridScript.setShip(ShipTypes.Destroyer);
					rpcScript.SignalPlayer();
					myRadarCount--;
				}
			}
	
			if (myKamikazeCount > 0) {
				GUI.Label (new Rect(120,290,100,30),"Remaining: " + myKamikazeCount);

				if (GUI.Button (new Rect(10,290,100,30), "Place Kamikaze")) {
					gridScript.setShip(ShipTypes.Destroyer);
					rpcScript.SignalPlayer();
					myKamikazeCount--;

				}
			}
	
			break;

		case (GameState.SetupWaiting):
			break;
		case (GameState.Play):
			// GUI to be displayed during playing phase
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
		// Perform one last update to visibility before ending turn
		gridScript.ResetVisibility();
		foreach (ShipScript s in ships) {
			s.CustomPlayUpdate();
		}
		foreach (ShipScript s in ships) {
			s.UpdateShipVisibility();
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

	public PlayerType checkWinner(string myName, PlayerType myType) {
		bool player1HasShips = false;
		bool player2HasShips = false;

		foreach (ShipScript s in ships) {
			if (s.myPlayerType == GameScript.PlayerType.Player1) {
				player1HasShips = true;
			} else if (s.myPlayerType == GameScript.PlayerType.Player2) {
				player2HasShips = true;
			}
		}
		if (player1HasShips || player2HasShips) return GameScript.PlayerType.None;
		if (!player2HasShips) return GameScript.PlayerType.Player1;
		if (!player1HasShips) return GameScript.PlayerType.Player2;



		return GameScript.PlayerType.None;
	}



}
