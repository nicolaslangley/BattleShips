using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class GameScript : MonoBehaviour {

	/** Enums **/

	public enum GameState {Setup, Play, End};
	public enum Direction {North, East, South, West};
	public enum CellState {Available, Mine, Reef, Ship, Base};
	public enum PlayAction {Move, Cannon, Torpedo, DropMine, PickupMine, Repair, None};



	/** Properties **/

	public List<GameObject> ships;
	public GridScript gridScript;
	public ShipScript selectedShip;
	public RPCScript rpcScript;

	public string myname;
	public string opponentname;

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

		GUI.Label(new Rect(10, 20, 150, 20), myname);
		GUI.Label(new Rect(10, 20, 150, 20), opponentname);
	}
	
	// Update is called once per frame
	void Update () {
		switch (curGameState) {
		case (GameState.Setup):
			// Perform update to objects based on setup state
			gridScript.CustomSetupUpdate();
			break;
		case (GameState.Play):
			// Perform update to objects based on play state
			foreach (GameObject o in ships) {
				ShipScript s = o.GetComponent<ShipScript>();
				s.CustomPlayUpdate();
			}
			break;
		case (GameState.End):
			break;
		}
	}

	// Display GUI overlay for game
	void OnGUI () {
		switch(curGameState) {
		case (GameState.Setup):
			// GUI to be displayed during setup phase
			if (GUI.Button(new Rect(10, 10, 100, 30), "Play Game")) {
				curGameState = GameState.Play;
				Debug.Log ("Moving to Play state");
			}
			if (GUI.Button (new Rect(10, 50, 100, 30), "Place Ship")) {
				gridScript.PlaceShip();
				Debug.Log ("Placing a Ship on selected square");
				rpcScript.SignalPlayer();
			}
			break;
		case (GameState.Play):
			// GUI to be displayed during playing phase
			break;
		}
	}

}
