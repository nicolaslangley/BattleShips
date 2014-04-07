using UnityEngine;
using System.Collections;

public class BaseSectionScript : MonoBehaviour {

	public BaseScript parent;
	private RPCScript rpcScript;
	private GameObject system;

	void Start () {
		parent = transform.parent.gameObject.GetComponent<BaseScript>();
		system = GameObject.FindGameObjectWithTag("System");
		rpcScript = system.GetComponent<RPCScript>();
	}
	
	// Handle clicking on object
	void OnMouseDown () {
		Debug.Log ("Click on base section");
		GameScript gameScript = parent.gameScript;
		// Don't act on mouse click if in wait state
		if (gameScript.curGameState == GameScript.GameState.Wait) return;
		
		// Handle selection if in cannon state
		if (gameScript.curPlayAction == GameScript.PlayAction.Cannon) {
			CellScript curCell = parent.GetCellForSection(this);
			if (curCell.availableForShoot) {
				gameObject.renderer.material.color = Color.red;
				if (gameScript.selectedShip.heavyCannon)
					parent.HandleHit(this.gameObject, 1, 2);
				else
					parent.HandleHit(this.gameObject,1, 1);
				gameScript.selectedShip.DisplayCannonRange(false);
				gameScript.curPlayAction = GameScript.PlayAction.None;
			}
		} else if ((int)parent.playerType != (int)(gameScript.myPlayerType)) {
			// Only allow for repair or selection if the base is mine.
			return; 
		} else {
			Debug.Log (parent.playerType + " " + gameScript.myPlayerType);
			Debug.Log("base selection changed");
			parent.selected = !parent.selected;
			if (parent.selected == true) {
				//gameObject.renderer.material.color = Color.cyan;
				foreach (CellScript oCellScript in parent.cells) {
					oCellScript.selected = true;
					//cs.DisplaySelection();
				}
			}
		}
	}
}
