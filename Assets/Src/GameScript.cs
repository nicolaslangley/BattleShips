using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameScript : MonoBehaviour {

	/** Enums **/
	public enum GameState {Setup,SetupWaiting, Play, Wait, End};
	public enum Direction {North, East, South, West};
	public enum CellState {Available, Mine, Reef, Ship, Base};
	public enum PlayAction {Move, Cannon, Torpedo, DropMine, PickupMine, Repair, None};

	public enum PlayerType {Player1 =1, Player2=2}
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
		curGameState = GameState.Setup;
		// Run game initialization
		gridScript.Init();

		string playerName = PlayerPrefs.GetString("playerName");
		myname = playerName;
		rpcScript.sendPlayerName(myname);

		setPlayerType();
		player1SetupDone = false;
		player2SetupDone = false;
	}
	
	// Update is called once per frame
	void Update () {
		switch (curGameState) {
		case (GameState.Setup):
			// Perform update to objects based on setup state
			gridScript.CustomSetupUpdate();
			break;

		case (GameState.SetupWaiting):
			if (player1SetupDone && player2SetupDone) {
				curGameState = GameState.Play;
			}
			break;
		case (GameState.Play):
			// Perform update to objects based on play state
			gridScript.ResetVisibility();
			foreach (ShipScript s in ships) {
				s.CustomPlayUpdate();
			}
			break;
		case (GameState.Wait):
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
			bases[(int)myPlayerType - 1].DisplayDockingRegion(true);
			// GUI to be displayed during setup phase
			if (GUI.Button(new Rect(10, 10, 100, 30), "Play Game")) {
				rpcScript.SetupDone((int)myPlayerType);
				curGameState = GameState.SetupWaiting;
				Debug.Log ("Moving to SetupWait state");
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
			if (GUI.Button (new Rect(10, 50, 100, 30), "Place Ship")) {
				gridScript.setShip();
				Debug.Log ("Placing a Ship on selected square");
				rpcScript.SignalPlayer();
			}

			if (GUI.Button (new Rect(10, 80, 100, 30), "Default Config")) {
				gridScript.PlaceShip(5,10,7,10,1,myname);
				gridScript.PlaceShip(5,20,7,20,1,myname);
				gridScript.PlaceShip(5,15,7,15,1,myname);

				gridScript.PlaceShip(24,10,22,10,1,opponentname);
				gridScript.PlaceShip(24,20,22,20,1,opponentname);
				gridScript.PlaceShip(24,15,22,15,1,opponentname);
				
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

}
