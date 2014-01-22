using UnityEngine;
using System.Collections;

public class CameraScript : MonoBehaviour {

	// Center camera on center of grid
	// TODO: This only works if the size of the grid is an odd number
	void Update () {
		GameObject[] gridCells = GameObject.FindGameObjectsWithTag("Grid");
		Transform center = gridCells[(int)gridCells.Length/2].transform;
		transform.LookAt(center);
	}

}
