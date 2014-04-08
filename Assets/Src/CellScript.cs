using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class CellScript : MonoBehaviour {

	/** PROPERTIES **/

	//[XmlArray("neighbours")]
	public List<CellScript> neighbours;
	public bool selected = false;
	public bool available = false;
	public bool availableForMove = false;
	public bool availableForShoot = false;
	public bool availableForDock = false;
	public GameObject occupier = null;
	public bool isMineRadius = false;

	//[XmlAttribute("cell_state")]
	public GameScript.CellState curCellState = GameScript.CellState.Available;
	[XmlAttribute("gridPositionX")]
	public int gridPositionX;
	[XmlAttribute("gridPositionY")]
	public int gridPositionY;
	[XmlAttribute("instanceID")]
	public int instanceID;
	public bool isVisible = false;

	public CellScript mineParentCell;

	private GameObject system;
	private GridScript gridScript;
	private GameScript gameScript;
	private RPCScript rpcScript;



	/** UNITY METHODS **/

	void OnGUI () {
		if (gameScript.curGameState == GameScript.GameState.Wait) return;
//		if (selected == true) {
//			if (GUI.Button(new Rect(Screen.width - 110, 300, 100, 30), "Explode")) {
//				rpcScript.Explosion(gridPositionX,gridPositionY,GridScript.ExplodeType.Kamikaze);
//			}
//		}
	}
	

	// Change selection status of cell and display it and it's neighbours if allowShipPlacement button has been pressed;
	void OnMouseDown () {
		Debug.Log ("Mouse click occured");
		Debug.Log ("CurGameState is: " + gameScript.curGameState);
		Debug.Log ("CurGameAction is: " + gameScript.curPlayAction);
		// Don't act on mouse click if in wait state
		if (gameScript.curGameState == GameScript.GameState.Wait) return;
		if (gameScript.curGameState == GameScript.GameState.End) return;

		// Handle selection if moving ship
		if (gameScript.curPlayAction == GameScript.PlayAction.Move) {
			if (availableForMove) {
				gameScript.selectedShip.MoveShip(this,1);
				gameScript.curPlayAction = GameScript.PlayAction.None;
				gameScript.waitTurn = true;
			}
			return;
		} else if (gameScript.curPlayAction == GameScript.PlayAction.Cannon) {
			if (availableForShoot) {
				gameScript.selectedShip.FireCannon(this, 1);
				gameScript.curPlayAction = GameScript.PlayAction.None;

			}
		} 
		else if (gameScript.curPlayAction == GameScript.PlayAction.DropMine) {
			if (availableForShoot) {
				gameScript.selectedShip.LayMine(this, 1);
				gameScript.curPlayAction = GameScript.PlayAction.None;
			}
		} else {
			if (gameScript.curGameState == GameScript.GameState.Setup && availableForDock) {
				Debug.Log ("Selection changing");
				selected = !selected;
				if (selected == true) {
					// Verify that selection is valid otherwise return
					if (gridScript.AddToSelection(this) == false) {
						selected = false;
						return;
					}
					//selected = false;
				} else {
					gridScript.RemoveFromSelection(this);
				}
				DisplaySelection();
			}
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

	public void SetAvailable() {
		curCellState = GameScript.CellState.Available;
		available = true;
		gameObject.renderer.material.color = Color.blue;
	}

	public void handleCellDamage(int damage, GridScript.ExplodeType type, CellScript origin) {
		Debug.Log("Handing cell damage for " + gridPositionX + " " + gridPositionY);
		if (occupier != null) {
			Debug.Log ("Occupier isn't null");
			ShipScript ship = occupier.GetComponent<ShipScript>();
			//Do one for base as well
			BaseScript based = occupier.GetComponent<BaseScript>();
			if (ship != null )
			{
				Debug.Log ("Ship isn't null");
				if (type == GridScript.ExplodeType.Mine) {
					Debug.Log("Mine damage");

					ship.HandleDoubleHit(this,damage, origin);
				} else {
					ship.HandleHit(this,damage);
				}
			}
			if (based != null)
			{
				based.HandleHit(this,damage);
			}

			//Occupier handles explosion
		}
	}
}
