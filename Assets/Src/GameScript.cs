using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class GameScript : MonoBehaviour {

	public List<GameObject> ships;
	public GridScript gridScript;
	public GUIScript guiScript;

	public enum GameState {Setup, Play, End};
	public GameState curState = GameState.Setup;

	// Use this for initialization
	void Start () {
		gridScript = gameObject.GetComponent<GridScript>();
		guiScript = gameObject.GetComponent<GUIScript>();

		// Run game initialization
		gridScript.Init();
	}
	
	// Update is called once per frame
	void Update () {
		switch (curState) {
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
