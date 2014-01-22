using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CellScript : MonoBehaviour {

	private bool selected = false;
	GameObject system;
	GUIScript guiScript;
	GridScript gridScript;
	List<GameObject> neighbours;

	/** UNITY METHODS **/
	
	// Use this for initialization
	void Start () {
		gameObject.renderer.material.color = Color.blue;
		system = GameObject.FindGameObjectWithTag("System");
		guiScript = system.GetComponent<GUIScript>();
		gridScript = system.GetComponent<GridScript>();
	}

	// Change selection status of cell and display it and it's neighbours if allowShipPlacement button has been pressed;
	void OnMouseDown () {
		// Only allow for selection if "Place Ship" button has been pressed
		if (guiScript.allowShipPlacement == true) {
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

	/** HELPER METHODS **/

	// This method can be called on a cell to display it as selected and display it's neighbours
	public void DisplaySelection () {
		neighbours = gridScript.GetCellNeighbours(gameObject);
		if (selected == true) {
			gameObject.renderer.material.color = Color.red;
			foreach (GameObject o in neighbours) {
				if (o.renderer.material.color != Color.red) o.renderer.material.color = Color.green;
			}
		} else {
			gameObject.renderer.material.color = Color.blue;
			foreach (GameObject o in neighbours) {
				o.renderer.material.color = Color.blue;
				if (o.GetComponent<CellScript>().selected == true) {
					o.GetComponent<CellScript>().DisplaySelection();
				}
			}
		}
	}
}
