using UnityEngine;
using System.Collections;

public class ResetCamera : MonoBehaviour {
	public GameObject Cam;

	public void Reset () {
		Cam.transform.position = new Vector3 (1, 7, -20);
	}
	

}
