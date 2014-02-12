using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class GameScript : MonoBehaviour {

	/** Enums **/

	public enum GameState {Setup, Play, End};
	public enum Direction {North, South, East, West};
	public enum CellState {Available, Mine, Reef, Ship};

	/** Properties **/

	public List<GameObject> ships;
	public GridScript gridScript;
	public GUIScript guiScript;
	public GameState curGameState = GameState.Setup;

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
			break;
		case (GameState.End):
			break;
		}
	}
}
