using UnityEngine;
using System.Collections;

public class ObservationCameraScript : MonoBehaviour {
	
	
	public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
	public RotationAxes axes = RotationAxes.MouseXAndY;
	public float sensitivityX = 15F;
	public float sensitivityY = 15F;

	public float minimumX = -360F;
	public float maximumX = 360F;

	public float minimumY = -360F;
	public float maximumY = 360F;
	
	float rotationX;
	float rotationY = 0F;
	
	
	public float mainSpeed = 100.0f;
	public float shiftAdd = 250.0f;
	public float maxShift = 1000.0f;
	public float camSens = 0.25f;
	
	private float totalRun = 1.0f;
	
	// Update is called once per frame
	void Update () {
		if (axes == RotationAxes.MouseXAndY)
		{
			rotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * sensitivityX;
			
			rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
			rotationY = Mathf.Clamp (rotationY, minimumY, maximumY);
			
			transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0);
		}
		else if (axes == RotationAxes.MouseX)
		{
			transform.Rotate(0, Input.GetAxis("Mouse X") * sensitivityX, 0);
		}
		else
		{
			rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
			rotationY = Mathf.Clamp (rotationY, minimumY, maximumY);
			
			transform.localEulerAngles = new Vector3(-rotationY, transform.localEulerAngles.y, 0);
		}
		
		Vector3 p = GetBaseInput();
		
		if (Input.GetKey (KeyCode.LeftShift)) {
			totalRun += Time.deltaTime;
			p = p * totalRun * shiftAdd;
			p.x = Mathf.Clamp(p.x, -maxShift, maxShift);
			p.y = Mathf.Clamp(p.y, -maxShift, maxShift);
			p.z = Mathf.Clamp(p.z, -maxShift, maxShift);
		} else {
			totalRun = Mathf.Clamp(totalRun * 0.5f, 1, 1000);
			p = p * mainSpeed;
		}
		
		p = p * Time.deltaTime;
		Vector3 newPosition = transform.position;
        if (Input.GetKey(KeyCode.Space)){ //If player wants to move on X and Z axis only
            transform.Translate(p);
            newPosition.x = transform.position.x;
            newPosition.z = transform.position.z;
            transform.position = newPosition;
        } else {
            transform.Translate(p);
        }
	}
	
	
	private Vector3 GetBaseInput() {
		Vector3 p_Velocity = new Vector3();
		if (Input.GetKey(KeyCode.W)) {
			p_Velocity += new Vector3(0, 0, 1);
		}
		if (Input.GetKey(KeyCode.S)) {
			p_Velocity += new Vector3(0, 0, -1);
		}
		if (Input.GetKey(KeyCode.A)) {
			p_Velocity += new Vector3(-1, 0, 0);
		}
		if (Input.GetKey(KeyCode.D)) {
			p_Velocity += new Vector3(1, 0, 0);
		}
		return p_Velocity;
	}
	
	void OnGUI () {
		GUI.Box (new Rect (0,Screen.height - 50,100,50), "Allied Ships");
		GUI.Box (new Rect (Screen.width-100, Screen.height-50,100, 50), "Enemies Left"); 
	}
}
