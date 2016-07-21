using UnityEngine;
using System.Collections;

public class UseItem : MonoBehaviour {
	public bool ExtraGamemodes;


	public static void Use (string itemName) {

		Debug.Log ("Custom item usage executing for item: " + itemName);


		// * TODO implement your own custom code for what happens when an item is used after it's used
		 //* 
		// * EXAMPLE USAGE:
		 //* 
		if (itemName == "Extra Gamemodes") {
			LoadOnClickEXTRAS.ExtraModes = true;
		}

		//if (itemName == "Extra Life") {
		//	YourOwnScript.addLives(1);
		//}
	//

	}

	void Gamemodes(){
		ExtraGamemodes = true;
	}

}
