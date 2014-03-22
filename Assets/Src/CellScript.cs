using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CellScript : MonoBehaviour {

	/** PROPERTIES **/

	public List<CellScript> neighbours;
	public bool selected = false;
	public bool available = false;
	public GameObject occupier = null;
	public GameScript.CellState curCellState = GameScript.CellState.Available;
	public int gridPositionX;
	public int gridPositionY;
	public int instanceID;

	private GameObject system;
	private GridScript gridScript;
	private GameScript gameScript;
	private RPCScript rpcScript;
	private bool isVisible = true;

	/** UNITY METHODS **/

	// Change selection status of cell and display it and it's neighbours if allowShipPlacement button has been pressed;
	void OnMouseDown () {
		Debug.Log ("Mouse click occured");
		Debug.Log ("CurGameState is: " + gameScript.curGameState);
		Debug.Log ("CurGameAction is: " + gameScript.curPlayAction);
		// Don't act on mouse click if in wait state
		if (gameScript.curGameState == GameScript.GameState.Wait) return;

		// Handle selection if moving ship
		if (gameScript.curPlayAction == GameScript.PlayAction.Move) {
			gameScript.selectedShip.MoveShip(this,1);
			gameScript.curPlayAction = GameScript.PlayAction.None;
			gameScript.waitTurn = true;
			return;
		} else if (gameScript.curPlayAction == GameScript.PlayAction.Cannon) {
			gameScript.selectedShip.FireCannon(this);
		} else {
			Debug.Log ("Selection changing");
			selected = !selected;
			if (selected == true) {
				// Verify that selection is valid otherwise return
				if (gridScript.AddToSelection(gameObject) == false) {
					selected = false;
					return;
				}
				
			} else {
				gridScript.RemoveFromSelection(gameObject);
			}
			DisplaySelection();
		}
	}

	/** GAMELOOP METHODS **/
	
	// Use this for initialization
	public void Init () {
		gameObject.renderer.material.color = Color.blue;
		available = true;
		system = GameObject.FindGameObjectWithTag("System");
		gridScript = system.GetComponent<GridScript>();
		gameScript = system.GetComponent<GameScript>();
		rpcScript = system.GetComponent<RPCScript>();
		instanceID = gameObject.GetInstanceID();
	}

	/** HELPER METHODS **/

	// This method can be called on a cell to display it as selected and display it's neighbours
	public void DisplaySelection () {
		Debug.Log ("Display Selection running");
//		neighbours = gridScript.GetCellNeighbours(gameObject);
		if (selected == true) {
			//Debug.Log ("Changing Color");
			//Debug.Log (this.gameObject.GetInstanceID());
			gameObject.renderer.material.color = Color.red;
			//Debug.Log ("Color changed");
//			foreach (GameObject o in neighbours) {
//				if (o.renderer.material.color != Color.red) o.renderer.material.color = Color.green;
//			}
		} else {
			gameObject.renderer.material.color = Color.blue;
//			foreach (GameObject o in neighbours) {
//				o.renderer.material.color = Color.blue;
//				if (o.GetComponent<CellScript>().selected == true) {
//					o.GetComponent<CellScript>().DisplaySelection();
//				}
//			}
		}
	}

	/*
	 * Sets the visibility and changes the color of the cell to grey.
	 */
	public void SetVisible(bool visibility) {
		isVisible = visibility;
		if (isVisible) {
			Color tempColor = gameObject.renderer.material.color;
			tempColor.a = 1.0f;
			gameObject.renderer.material.color = tempColor;
		} else {
			Color tempColor = gameObject.renderer.material.color;
			tempColor.a = 0.3f;
			gameObject.renderer.material.color = tempColor;
		}
	}

	public void SetGridPosition (int x, int y) {
		gridPositionX = x;
		gridPositionY = y;
	}

	public void SetBase (Color c) {
		curCellState = GameScript.CellState.Base;
		gameObject.renderer.material.color = c;
	}

	public void SetReef () {
		curCellState = GameScript.CellState.Reef;
		available = false;
		gameObject.renderer.material.color = Color.black;
	}
}
