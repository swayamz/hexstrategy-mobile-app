using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;


public class StoreActions : MonoBehaviour {

	public static StoreActions INSTANCE;

	void Awake() {
		INSTANCE = this;
	}

	public GameObject MainStoreScreen;
	public GameObject PurchaseItemScreen;
	public GameObject PurchasedItemsScreen;

	public Image PurchaseItemImageField;
	public Text PurchaseItemNameField;
	public Text PurchaseItemCostField;
	public Text PurchaseItemCurrCodeField;
	public Text PurchaseItemDescField;

	public Text PurchaseStatusField;

	public Text StoreTitleField;
	
	private string generatedPayID = "";
	private bool isPaidFlag = false;
	

	// Use this for initialization
	void Start () {
		OpenStore ();
	}

	public void OpenStore() {

		MainStoreScreen.SetActive (true);
		StoreTitleField.text = StoreProperties.INSTANCE.gameTitle + " Store";

	}

	public void CloseStore() {

		MainStoreScreen.SetActive (false);
		Application.LoadLevel(0);


	}

	public void ViewPurchasedItems() {
		MainStoreScreen.SetActive (false);
		PurchasedItemsScreen.SetActive (true);

		InventoryActions.INSTANCE.refreshItems ();

	}

	public void OpenPurchaseItemScreen(StoreItemContent itemToPurchase) {
		MainStoreScreen.SetActive (false);
		PurchaseItemScreen.SetActive (true);

		PurchaseItemImageField.sprite = itemToPurchase.itemImage;
		PurchaseItemNameField.text = itemToPurchase.itemName;

		PurchaseItemCostField.text = string.Format("{0:N}", itemToPurchase.itemCost);
		PurchaseItemCostField.text = CurrencyCodeMapper.GetCurrencySymbol (StoreProperties.INSTANCE.currencyCode) + PurchaseItemCostField.text;

		PurchaseItemDescField.text = itemToPurchase.itemDesc;
		PurchaseItemCurrCodeField.text = StoreProperties.INSTANCE.currencyCode;

		StartCoroutine (SetupPurchase());

		InvokeRepeating("CheckForGeneratedPayID", 1f, 1f);

	}

	public void ClosePurchaseItemScreen() {
		MainStoreScreen.SetActive (true);
		PurchaseItemScreen.SetActive (false);

		if (IsInvoking ("AnimateWaitingText")) {
			CancelInvoke("AnimateWaitingText");
		}

		if (IsInvoking ("CheckForPaidPurchaseStatus")) {
			CancelInvoke("CheckForPaidPurchaseStatus");
		}

		if (IsInvoking ("PollInvoker")) {
			CancelInvoke("PollInvoker");
		}

		generatedPayID = "";
		isPaidFlag = false;
		PurchaseStatusField.text = "Waiting";
		PurchaseStatusField.color = Color.yellow;

	}

	public void PollServerForPurchaseStatusChange() {
		float pollInterval = 5f;
		InvokeRepeating ("PollInvoker", 0f, pollInterval);
		InvokeRepeating ("CheckForPaidPurchaseStatus", 2f, pollInterval);
		InvokeRepeating ("AnimateWaitingText", 1f, 1f);

	}

	//this is repeatedly called until a PayID is set within SetupPurchase()
	void CheckForGeneratedPayID() {

		if (generatedPayID != "") {

			CancelInvoke("CheckForGeneratedPayID");

			string checkoutURL = "";

			if (StoreProperties.INSTANCE.payPalEndpoint == StoreProperties.Environment.SANDBOX) {
				checkoutURL = "https://www.sandbox.paypal.com/cgi-bin/webscr?cmd=_ap-payment&paykey=";
			} else {
				checkoutURL = "https://www.paypal.com/cgi-bin/webscr?cmd=_ap-payment&paykey=";
			}

			checkoutURL += generatedPayID;

			//Differnt behaviour required for different platforms
			if (Application.isWebPlayer) {
				Application.ExternalEval("window.open('"+checkoutURL+"','_blank')");
			} else {
				Application.OpenURL(checkoutURL);
			}


			PollServerForPurchaseStatusChange ();

			Debug.Log("opened URL: " + checkoutURL);
		}

	}

	void CheckForPaidPurchaseStatus() {
		
		if (isPaidFlag) {
			CancelInvoke("PollInvoker");
			CancelInvoke("CheckForPaidPurchaseStatus");
			CancelInvoke("AnimateWaitingText");

			PurchaseStatusField.color = Color.green;
			PurchaseStatusField.text = "SUCCESS!";
		}
		
	}

	void PollInvoker() {
		StartCoroutine(GetPurchaseStatus());
	}

	void AnimateWaitingText() {
		Debug.Log ("animating text");
		string waitingText = PurchaseStatusField.text;

		waitingText += ".";

		if (waitingText.Length > ("Waiting".Length + 5)) {
			waitingText = "Waiting";
		}

		PurchaseStatusField.text = waitingText;
	}


	IEnumerator GetPurchaseStatus() {

		WWWForm postData = new WWWForm();
		
		postData.AddField("PayPalPaymentID", generatedPayID);
		
		string url = "http://unityingameitemmanager.com/GetPaymentStatus.php";
		
		WWW www = new WWW(url, postData.data, StoreProperties.INSTANCE.createHeader(postData));
		
		yield return www;
		
		//if ok response
		if (www.error == null) {
			Debug.Log("WWW Ok! Full Text: " + www.text);

			string resultString = StoreProperties.INSTANCE.parseRawHTTPresponseString(www.text);

			if (resultString.ToUpper() == "Y") {
				isPaidFlag = true;
			}
			
			// check for errors
		} else {
			Debug.Log("WWW Error: "+ www.error);
		}    
	}


	IEnumerator SetupPurchase() {

		WWWForm postData = new WWWForm();

		postData.AddField("SellerEmail", StoreProperties.INSTANCE.payPalEmailAddressOfSeller);
		postData.AddField("ItemName", PurchaseItemNameField.text);
		postData.AddField("ItemCost", PurchaseItemCostField.text.Substring(1));
		postData.AddField("ItemCurrencyCode", StoreProperties.INSTANCE.currencyCode);
		postData.AddField("PlayerID", StoreProperties.INSTANCE.playerID);
		postData.AddField("GameTitle", StoreProperties.INSTANCE.gameTitle);

		string sandboxFlag = (StoreProperties.INSTANCE.payPalEndpoint == StoreProperties.Environment.SANDBOX) ? "Y" : "N";
		postData.AddField("SandboxFlag", sandboxFlag);
		
		string setupPurchaseURL = "http://unityingameitemmanager.com/SetupPurchase.php";
		WWW www = new WWW(setupPurchaseURL, postData.data, StoreProperties.INSTANCE.createHeader(postData));		
		
		yield return www;
		
		//if ok response
		if (www.error == null) {
			Debug.Log("WWW Ok! Full Text: " + www.text);
			string resultString = StoreProperties.INSTANCE.parseRawHTTPresponseString(www.text);
			
			generatedPayID = resultString;
			
		} else {
			Debug.Log("WWW Error: "+ www.error);
		}    
	}




}
