using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BaseScript : MonoBehaviour {

	public string player;
	public int playerType;
	public GameScript gameScript;
	public GridScript gridScript;
	public bool selected;
	public List<CellScript> cells;

	private List<GameObject> baseSections;
	private GameObject system;
	protected RPCScript rpcScript;
	private int[] health;

	/** UNITY METHODS **/

	// Display movement options for selected ship
	void OnGUI () {
		if (gameScript.curGameState == GameScript.GameState.Wait) return;
		if (selected == true) {
			if (GUI.Button(new Rect(Screen.width - 110, 30, 100, 30), "Repair Ship")) {
				gameScript.curPlayAction = GameScript.PlayAction.Repair;
				DisplayDockingRegion(true);
				Debug.Log ("Entering repair mode");
			}
		}
	}

	/** GAMELOOP METHODS **/

	// Use this for initialization
	public void Init () {
		system = GameObject.FindGameObjectWithTag("System");
		gameScript = system.GetComponent<GameScript>();
		gridScript = system.GetComponent<GridScript>();
		rpcScript = system.GetComponent<RPCScript>();
		baseSections = new List<GameObject>();
		foreach (Transform child in transform) {
			baseSections.Add(child.gameObject);
		}
	}

	public void CustomPlayUpdate () {
		// Handle visibility update for all surrounding cells
		
		if (player == gameScript.myname)
		{
			Debug.Log ("My base update function");
		}
	}

	/** HELPER METHODS **/

	// Retrive object for section of ship
	public GameObject GetSection(int section) {
		return baseSections[section];
	}

	public void HandleHit(GameObject section, int local, int damage) 
	{
		int sectionIndex = baseSections.IndexOf(section);
		if (local == 1)
		{
			Debug.Log("Local");
			rpcScript.HandleBaseDamage(player,sectionIndex,damage);
			return;
		}
		Debug.Log ("Hit handled on section: " + sectionIndex);
		health [sectionIndex] -= damage;
		if (health [sectionIndex] < 0) { health[sectionIndex]=0;}
		int damageTotal = 0;
		for (int i = 0; i < 10; i++) {
			Debug.Log ("health" + health[i]);
			damageTotal += health[i];
		}
		Debug.Log ("Damage total is: " + damageTotal);
		if (damageTotal == 0) {
			Destroy(gameObject);
			//Take care of stats, etc.
		} else {
			section.renderer.material.color = Color.red;
		}
		
		//rpcScript.EndTurn();
		gameScript.EndTurn();
	}
		
	/*
	 * TODO: handle display for when part of base has been destroyed
	 */
	public void DisplayDockingRegion(bool status) {

		if (cells[0].gridPositionX == 0) {
			//Debug.Log ("Displaying docking region for base1");
			for (int i = 0; i < 10; i++) {
				if (health[i] > 0) { 
					gridScript.DisplayCellForDock(status, 1, 10+i);
				}
			}
		} else if (cells[0].gridPositionX == 29) {
			//Debug.Log ("Displaying docking region for base2");
			for (int i = 0; i < 10; i++) {
				if (health[i] > 0) {
					gridScript.DisplayCellForDock(status, 28, 10+i);
				}
			}
		} 

	}
	
}
