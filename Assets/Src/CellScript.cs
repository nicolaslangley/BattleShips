using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CellScript : MonoBehaviour {

	/** PROPERTIES **/

	public List<GameObject> neighbours;
	public bool selected = false;
	public bool available = false;

	private GameObject system;
	private GUIScript guiScript;
	private GridScript gridScript;


	/** UNITY METHODS **/
	
	// Use this for initialization
	public void Init () {
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
}
