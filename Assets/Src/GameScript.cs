using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class GameScript : MonoBehaviour {

	/** Enums **/

	public enum GameState {Setup, Play, End};
	public enum Direction {North, South, East, West};
	public enum CellState {Available, Mine, Reef, Ship, Base};
	public enum PlayAction {Move, Cannon, Torpedo, DropMine, PickupMine, Repair, None};

	/** Properties **/

	public List<GameObject> ships;
	public GridScript gridScript;
	public GUIScript guiScript;
	public GameObject selectedShip;

	/** Current state of game **/
	public GameState curGameState = GameState.Setup;
	public PlayAction curPlayAction = PlayAction.None;

	/** UNITY METHODS **/

	// Use this for initialization
	void Start () {
		gridScript = gameObject.GetComponent<GridScript>();
		guiScript = gameObject.GetComponent<GUIScript>();

		// Run game initialization
		gridScript.Init();

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
			Debug.Log ("In Play state");
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
			}
			break;
		case (GameState.Play):
			// GUI to be displayed during playing phase
			break;
		}
	}

}
