using UnityEngine;
using System.Collections;
using UnityEngine.Advertisements;

public class LoadOnClickEXTRAS : MonoBehaviour {
	public static bool ExtraModes = false;


	public void LoadScene(int level)
	{
		if (ExtraModes == true) {
			Application.LoadLevel(level);

		}
		if (ExtraModes != true) {
			Application.LoadLevel(9);

		}

	}
}