using UnityEngine;
using System.Collections;

public class BaseSaverScript {
	public int[] health;
	public string player;
	public GameScript.PlayerType myPlayerType;

	public BaseSaverScript(BaseScript playerBase) {
		health = playerBase.health;
		player = playerBase.player;
		myPlayerType = playerBase.myPlayerType;
	}

	public void Restore(BaseScript playerBase) {
		playerBase.health = health;
		playerBase.player = player;
		playerBase.myPlayerType = myPlayerType;
	}

}
