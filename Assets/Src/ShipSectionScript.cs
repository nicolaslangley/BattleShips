using UnityEngine;
using System.Collections;

public class ShipSectionScript : MonoBehaviour {
	
	public ShipScript parent;

	void Start () {
		parent = transform.parent.gameObject.GetComponent<ShipScript>();
		if (this.name == "Bow") gameObject.renderer.material.color = Color.black;
		if (this.name == "Stern") gameObject.renderer.material.color = Color.white;
	}

	// Handle clicking on object
	void OnMouseDown () {
		GameScript gameScript = parent.gameScript;
		// Don't act on mouse click if in wait state
		if (gameScript.curGameState == GameScript.GameState.Wait) return;

		// Handle selection if moving ship
		if (gameScript.curPlayAction == GameScript.PlayAction.Cannon) {
			gameObject.renderer.material.color = Color.red;
			parent.HandleHit(this.gameObject);
		} else {
			parent.selected = !parent.selected;
			if (parent.selected == true) {
				//gameObject.renderer.material.color = Color.cyan;
				foreach (GameObject o in parent.cells) {
					CellScript cs = o.GetComponent<CellScript>();
					cs.selected = true;
					//cs.DisplaySelection();
				}
				parent.gameScript.selectedShip = parent;
			}
		}
	}
}
