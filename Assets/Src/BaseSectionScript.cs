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
		GameScript gameScript = parent.gameScript;
		// Don't act on mouse click if in wait state
		if (gameScript.curGameState == GameScript.GameState.Wait) return;
		
		// Handle selection if cannon ship
		if (gameScript.curPlayAction == GameScript.PlayAction.Cannon) {
			gameObject.renderer.material.color = Color.red;
			if (gameScript.selectedShip.heavyCannon)
				parent.HandleHit(this.gameObject, 1, 2);
			else
				parent.HandleHit(this.gameObject,1, 1);
		} else {
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
