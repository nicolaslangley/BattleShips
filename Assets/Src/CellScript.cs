using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CellScript : MonoBehaviour {

	/** PROPERTIES **/

	public List<GameObject> neighbours;
	public bool selected = false;
	public bool available = false;
	public bool isVisible = true;
	public GameObject occupier = null;
	public GameScript.CellState curCellState = GameScript.CellState.Available;
	public Vector2 gridPosition;

	private GameObject system;
	private GridScript gridScript;
	private GameScript gameScript;

	/** UNITY METHODS **/

	// Change selection status of cell and display it and it's neighbours if allowShipPlacement button has been pressed;
	void OnMouseDown () {
		// Handle selection if moving ship
		if (gameScript.curPlayAction == GameScript.PlayAction.Move) {
			gameScript.selectedShip.MoveShip(this);
			return;
		}

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

	/** GAMELOOP METHODS **/
	
	// Use this for initialization
	public void Init () {
		gameObject.renderer.material.color = Color.blue;
		system = GameObject.FindGameObjectWithTag("System");
		gridScript = system.GetComponent<GridScript>();
		gameScript = system.GetComponent<GameScript>();
	}

	/** HELPER METHODS **/

	// This method can be called on a cell to display it as selected and display it's neighbours
	public void DisplaySelection () {
		neighbours = gridScript.GetCellNeighbours(gameObject);
		if (selected == true) {
			gameObject.renderer.material.color = Color.red;
			foreach (GameObject o in neighbours) {
				if (o.renderer.material.color != Color.red) o.renderer.material.color = Color.green;
				o.GetComponent<CellScript>().available = true;
			}
		} else {
			gameObject.renderer.material.color = Color.blue;
			foreach (GameObject o in neighbours) {
				o.renderer.material.color = Color.blue;
				o.GetComponent<CellScript>().available = true;
				if (o.GetComponent<CellScript>().selected == true) {
					o.GetComponent<CellScript>().DisplaySelection();
				}
			}
		}
	}

	public void SetGridPosition (float x, float y) {
		gridPosition = new Vector2(x, y);
	}

	public void SetBase (Color c) {
		curCellState = GameScript.CellState.Base;
		gameObject.renderer.material.color = c;
	}

	public void SetReef () {
		curCellState = GameScript.CellState.Reef;
		gameObject.renderer.material.color = Color.black;
	}
}
