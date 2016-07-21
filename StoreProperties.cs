using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public class StoreProperties : MonoBehaviour {

	public static StoreProperties INSTANCE;

	void Awake() {
		INSTANCE = this;
	}
	
	public enum Environment {
		SANDBOX, PRODUCTION
	}
	
	public Environment payPalEndpoint;

	public string payPalEmailAddressOfSeller;

	public string currencyCode;

	public string gameTitle;

	[HideInInspector] // Hides var below
	public string playerID;


	public enum StoreTheme {
		BASIC,
		AQUA_PAPER,
		DARK_STONE,
		DIAMOND,
		BUBBLES,
		MARBLE,
		METAL,
		MOSS,
		PINSTRIPE,
		WEATHERED,
		WOOD
	}
	
	public StoreTheme storeTheme = StoreTheme.BASIC;
	
	[HideInInspector]
	public GameObject[] storeScreens;

	// Use this for initialization
	void Start () {

		//if basic is selected then don't change background
		if (storeTheme != StoreTheme.BASIC) {
	
			for (int i=0; i<storeScreens.Length; i++) {
				GameObject nextStoreScreen = storeScreens [i];
				nextStoreScreen.GetComponent<Image> ().sprite = Resources.Load <Sprite> ("StoreThemes/" + storeTheme.ToString ());
				nextStoreScreen.GetComponent<Image> ().color = Color.white;
			}
		}

		string gameCode = "";
		if (gameTitle.Length > 3) {
			gameCode = gameTitle.Substring(0,3);
		} else {
			gameCode = gameTitle;
		}

		//if playerID doesn't exist then create it
		if (!PlayerPrefs.HasKey ("PlayerID")) {
			PlayerPrefs.SetString ("PlayerID", gameCode + DateTime.Now.ToString ("yyyyMMddHHmmssffff"));
		}

		playerID = PlayerPrefs.GetString ("PlayerID");

		//Debug.Log ("PlayerID: " + playerID);

	}

	public string parseRawHTTPresponseString(string HTTPresponse) {
		
		return HTTPresponse.Split(new string[]{"***"}, System.StringSplitOptions.None)[1];;
		
	}
	
	public Dictionary<string,string> createHeader(WWWForm form) {
		
		Dictionary<string,string> header = new Dictionary<string,string>();
		
		header = form.headers;
		
		header.Add("Access-Control-Allow-Credentials", "true"); 
		header.Add("Access-Control-Allow-Headers", "Accept"); 
		header.Add("Access-Control-Allow-Methods", "POST"); 
		header.Add("Access-Control-Allow-Origin", "*");
		
		return header;
		
	}

}
