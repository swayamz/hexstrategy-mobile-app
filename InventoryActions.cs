using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public class InventoryActions : MonoBehaviour {

	// SINGLETON USAGE

	public static InventoryActions INSTANCE; //this was static

	void Awake() {
		INSTANCE = this;
	}

	//PRIVATE VARS

	private IList<InventoryItem> items = new List<InventoryItem>();

	private string getItemsResultString = "";

	private int selectedSlotIndex = 0;

	private bool requestUse = false;

	private bool requestDelete = false;

	private bool actionComplete = false;

	//PUBLIC VARS

	public GameObject[] itemSlots;

	public GameObject waitingPanel;

	public GameObject itemContentArea;

	public Text waitingText;

	public Button useItemButton;

	public Button deleteItemButton;

	public Button noButton;

	public Button yesButton;

	public Button okButton;

	public GameObject inventoryScreen;

	public GameObject mainStoreScreen;

	public RectTransform borderImage;

	public Text currentDisplayedItemNameText;

	public Image currentDisplayedItemImage;

	public Text currentDisplayedItemDesc;

	public Text currentDisplayedItemTS;

	public Text currentDispalyedItemStatus;

	public Text currentDisplayedItemCost;




	public void refreshItems() {

		items.Clear ();

		waitingPanel.SetActive (true);
		itemContentArea.SetActive (false);
		yesButton.gameObject.SetActive (false);
		noButton.gameObject.SetActive (false);
		okButton.gameObject.SetActive (false);
		waitingText.text = "Retrieving items, Please wait...";

		StartCoroutine (GetPurchasedItems ());
		InvokeRepeating ("CheckForReturnedItems", 1f, 1f);

	}
	
	IEnumerator GetPurchasedItems() {
		
		
		WWWForm postData = new WWWForm();
		
		postData.AddField("PlayerID", StoreProperties.INSTANCE.playerID);
		postData.AddField("GameTitle", StoreProperties.INSTANCE.gameTitle);
		
		string url = "http://unityingameitemmanager.com/GetItems2.php";


		WWW www = new WWW(url, postData.data, StoreProperties.INSTANCE.createHeader(postData));		
		
		yield return www;
		
		//if ok response
		if (www.error == null) {
			Debug.Log("WWW Ok! Full Text: " + www.text);
			
			string resultString = StoreProperties.INSTANCE.parseRawHTTPresponseString(www.text);

			if (resultString == "0 results") {
				getItemsResultString = "NONE";
			} else {
				getItemsResultString = resultString.Trim(',');
			}

			
		} else {
			Debug.Log("WWW Error: "+ www.error);
		}    
	}


	void CheckForReturnedItems() {
		
		if (getItemsResultString != "") {
			
			if (getItemsResultString == "NONE") {
				waitingText.text = "No Items To Display.";
				okButton.gameObject.SetActive(true);
			} else {
				waitingText.text = "";
				waitingPanel.SetActive(false);
				itemContentArea.SetActive(true);
				selectedSlotIndex = 0;
				borderImage.SetParent(itemSlots[0].GetComponentInChildren<RectTransform>());
				borderImage.localPosition = new Vector2(-1f,1f);
				PopulateRecievedItemsList();
				refreshItemSlots();
				refreshCurrentDisplayedItem();
			}
			
			CancelInvoke("CheckForReturnedItems");
			getItemsResultString = "";
		}
		
	}

	void PopulateRecievedItemsList() {

		
		string[] rawItemData = getItemsResultString.Split (',');
		
		foreach (string rawItem in rawItemData) {
			
			string[] itemProperties = rawItem.Split ('|');
			
			InventoryItem nextInventoryItem = new InventoryItem();
			
			nextInventoryItem.itemName = itemProperties[0];
			nextInventoryItem.purchaseTS = itemProperties[1];
			nextInventoryItem.cost = itemProperties[2];
			nextInventoryItem.currencyCode = itemProperties[3];
			nextInventoryItem.isSandboxItem = itemProperties[4].Equals("Y");
			nextInventoryItem.isUsed = itemProperties[5].Equals("Y");
			nextInventoryItem.purchaseID = itemProperties[6];

			items.Add(nextInventoryItem);
			
		}
		
	}

	public void refreshItemSlots() {
		
		for (int i=0; i<itemSlots.Length; i++) {
			
			//disable slot if no item exists to fill that slot
			if (i > items.Count-1) {
				itemSlots[i].SetActive(false);
				continue;
			} else {
				itemSlots[i].SetActive(true);
			}
			
			Sprite currentSlotSprite = itemSlots[i].GetComponentInChildren<Image>().sprite;
			
			//try load sprite
			currentSlotSprite = Resources.Load <Sprite> ("ItemSprites/"+ items[i].itemName);

			//load default image if no image able to be loaded
			if (currentSlotSprite == null) {
				currentSlotSprite = Resources.Load <Sprite> ("ItemSprites/DefaultImage");
			}

			itemSlots[i].GetComponentInChildren<Image>().sprite = currentSlotSprite;

			if (items [i].isUsed) {
				itemSlots[i].GetComponentInChildren<Image>().color = new Color32(255,255,255,100);
			} else {
				itemSlots[i].GetComponentInChildren<Image>().color = new Color32(255,255,255,255);
			}

		}
		
	}

	// set values for current dispalyed item
	public void refreshCurrentDisplayedItem() {

		currentDisplayedItemNameText.text = items [selectedSlotIndex].itemName;

		Sprite currentDisplayedItemSprite = Resources.Load <Sprite> ("ItemSprites/"+ items[selectedSlotIndex].itemName);
		
		//load default image if no image able to be loaded
		if (currentDisplayedItemSprite == null) {
			currentDisplayedItemSprite = Resources.Load <Sprite> ("ItemSprites/DefaultImage");
		}

		currentDisplayedItemImage.sprite = currentDisplayedItemSprite;

		currentDisplayedItemDesc.text = StoreContentManager.INSTANCE.GetDescription (items [selectedSlotIndex].itemName);

		currentDisplayedItemTS.text = calculateLocalTS(items[selectedSlotIndex].purchaseTS);

		currentDisplayedItemCost.text = CurrencyCodeMapper.GetCurrencySymbol(items [selectedSlotIndex].currencyCode) + items [selectedSlotIndex].cost + " " + items [selectedSlotIndex].currencyCode;

		currentDispalyedItemStatus.text = "Purchased";

		if (items [selectedSlotIndex].isUsed) {
			currentDispalyedItemStatus.text += " and Used";
			useItemButton.interactable = false;
		} else {
			useItemButton.interactable = true;
		}

		if (items [selectedSlotIndex].isSandboxItem) {
			currentDispalyedItemStatus.text += " [SANDBOX]";
		}




	}

	private string calculateLocalTS(string serverTS) {

		string tsFormatString = "yyyy-MM-dd HH:mm:ss";

		DateTime serverPurchaseTime = DateTime.ParseExact (serverTS, tsFormatString, null);
		DateTime serverPurchaseTimeUTC = serverPurchaseTime.AddHours (5);
		DateTime localPurchaseTime = serverPurchaseTimeUTC.Add (DateTimeOffset.Now.Offset);

		return localPurchaseTime.ToString(tsFormatString);

	}

	public void returnToStore() {
		mainStoreScreen.SetActive (true);
		inventoryScreen.SetActive (false);
	}

	public void setSelectedSlotIndex(int newIndex) {
		selectedSlotIndex = newIndex;
	}

	public void clickDeleteItem() {

		requestDelete = true;
		waitingText.text = "Are you sure you want to delete this item?";

		waitingPanel.SetActive (true);
		yesButton.gameObject.SetActive (true);
		noButton.gameObject.SetActive (true);
		itemContentArea.SetActive (false);

	}

	public void clickUseItem() {
		
		requestUse = true;
		waitingText.text = "Are you sure you want to use this item?";
		
		waitingPanel.SetActive (true);
		yesButton.gameObject.SetActive (true);
		noButton.gameObject.SetActive (true);
		itemContentArea.SetActive (false);
		
	}

	public void clickNo() {
		waitingPanel.SetActive (false);
		itemContentArea.SetActive (true);
	}

	public void clickOK() {

		okButton.gameObject.SetActive (false);

		if (waitingText.text == "No Items To Display.") {
			returnToStore ();
		} else {
			refreshItems ();
		}


	}

	public void clickYes() {

		yesButton.gameObject.SetActive (false);
		noButton.gameObject.SetActive (false);

		if (requestUse) {
			waitingText.text = "Using item, please wait...";
			StartCoroutine (UseOrDeleteItem (true));
		} else if (requestDelete) {
			waitingText.text = "Deleting item, please wait...";
			StartCoroutine (UseOrDeleteItem (false));
		}

		InvokeRepeating ("CheckActionComplete", 1f, 1f);
	}
	
	IEnumerator UseOrDeleteItem(bool isUsing) {
		
		WWWForm postData = new WWWForm();
		
		postData.AddField("ItemID", items[selectedSlotIndex].purchaseID);

		Debug.Log ("ID to action: " + items [selectedSlotIndex].purchaseID);

		string url = "";

		if (isUsing) {
			url = "http://unityingameitemmanager.com/UseItem.php";
		} else {
			url = "http://unityingameitemmanager.com/DeleteItem.php";
		}


		WWW www = new WWW(url, postData.data, StoreProperties.INSTANCE.createHeader(postData));		
		
		yield return www;
		
		//if ok response
		if (www.error == null) {
			Debug.Log("WWW Ok! Full Text: " + www.text);
			
			string resultString = StoreProperties.INSTANCE.parseRawHTTPresponseString(www.text);
			
			if (resultString == "DONE") {
				actionComplete = true;
				//use item

				if (isUsing) {
					UseItem.Use(items[selectedSlotIndex].itemName);
				}
			}
			
		} else {
			Debug.Log("WWW Error: "+ www.error);
		}    
	}
	
	void CheckActionComplete() {
		
		if (actionComplete) {
			
			CancelInvoke("CheckActionComplete");
			actionComplete = false;

			if (requestUse) {
				waitingText.text = "Item successfully used!";
				requestUse = false;
			} else {
				waitingText.text = "Item successfully deleted!";
				requestDelete = false;
			}

			okButton.gameObject.SetActive(true);			
		}
		
	}

}
