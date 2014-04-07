using UnityEngine;
using System.Collections;

public class ShipSectionScript : MonoBehaviour {
	
	public ShipScript parent;
	private RPCScript rpcScript;
	private GameObject system;


	void Start () {
		parent = transform.parent.gameObject.GetComponent<ShipScript>();
		system = GameObject.FindGameObjectWithTag("System");
		rpcScript = system.GetComponent<RPCScript>();
		if (this.name == "Bow") gameObject.renderer.material.color = Color.black;
		if (this.name == "Stern") gameObject.renderer.material.color = Color.white;
		if (parent.player != parent.gameScript.myname) {
			Debug.Log ("parent name " + parent.player + " gamescript name " + parent.gameScript.myname);
			Debug.Log ("not my ship so disable renderer");
			gameObject.renderer.enabled = false;
			Debug.Log ("Disabling visibility on cell");
			parent.GetCellForSection(this.gameObject).SetVisible(false);
		} 
	}

	// Handle clicking on object
	void OnMouseDown () {
		GameScript gameScript = parent.gameScript;
		// Don't act on mouse click if in wait state
		if (gameScript.curGameState == GameScript.GameState.Wait) return;

		// Handle selection if cannon ship
		if (gameScript.curPlayAction == GameScript.PlayAction.Cannon) {
			CellScript curCell = parent.GetCellForSection(this);
			if (curCell.availableForShoot) {
				gameObject.renderer.material.color = Color.red;
				if (gameScript.selectedShip.heavyCannon) {
					//parent.HandleHit(this.gameObject, 1, 2);
					parent.HandleCannon(this.gameObject,1,2);

				} else {
					//parent.HandleHit(this.gameObject,1, 1);
					parent.HandleCannon(this.gameObject,1,1);
				}
				gameScript.selectedShip.DisplayCannonRange(false);
				gameScript.curPlayAction = GameScript.PlayAction.None;
			}
		} else if (parent.player != gameScript.myname) {
			// Only allow for repair or selection if the ship is mine.
			return;
		} else if (gameScript.curPlayAction == GameScript.PlayAction.Repair) {
			parent.HandleRepair(this.gameObject,1);
			gameScript.curPlayAction = GameScript.PlayAction.None;

		}  else {
			parent.selected = !parent.selected;
			if (parent.selected == true) {
				//gameObject.renderer.material.color = Color.cyan;
				foreach (CellScript oCellScript in parent.cells) {
					oCellScript.selected = true;
					//cs.DisplaySelection();
				}
				parent.gameScript.selectedShip = parent;
			}
		}
	}
	
}
