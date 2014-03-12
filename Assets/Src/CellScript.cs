using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CellScript : MonoBehaviour {

	/** PROPERTIES **/

	public List<GameObject> neighbours;
	public bool selected = false;
	public bool available = false;
	public GameObject occupier = null;
	public GameScript.CellState curCellState = GameScript.CellState.Available;
	public int gridPositionX;
	public int gridPositionY;

	private GameObject system;
	private GridScript gridScript;
	private GameScript gameScript;
	private bool isVisible = true;

	/** UNITY METHODS **/

	// Change selection status of cell and display it and it's neighbours if allowShipPlacement button has been pressed;
	void OnMouseDown () {
		// Handle selection if moving ship
		if (gameScript.curPlayAction == GameScript.PlayAction.Move) {
			gameScript.selectedShip.MoveShip(this);
			gameScript.curPlayAction = GameScript.PlayAction.None;
			return;
		} else if (gameScript.curPlayAction == GameScript.PlayAction.Cannon) {
			gameScript.selectedShip.FireCannon(this);
		} else {
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

	/*
	 * Sets the visibility and changes the color of the cell to grey.
	 */
	public void setVisible(bool visibility) {
		isVisible = visibility;
		gameObject.renderer.material.color = Color.grey;
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
		gameObject.renderer.material.color = Color.black;
	}
}
