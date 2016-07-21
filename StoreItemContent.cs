using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Globalization;

public class StoreItemContent : MonoBehaviour {

	/* expose these values to user for convinience 
	 * (NOTE: these values will override any values set on the store item fields in the inspector)
	 */
	public Sprite itemImage;
	public string itemName;
	public float itemCost;
	public string itemDesc;

	private Image itemImageField;
	private Text itemNameTextField;
	private Text itemCostTextField;
	private Text itemCurCodeTextField;
	private Text itemDescTextField;

	// Use this for initialization
	void Start () {

		itemImageField = transform.FindChild ("ItemImage").GetComponent<Image> ();
		itemNameTextField = transform.FindChild ("ItemName").GetComponent<Text> ();
		itemCostTextField = transform.FindChild ("ItemCost").GetComponent<Text> ();
		itemCurCodeTextField = transform.FindChild ("ItemCurCode").GetComponent<Text> ();
		itemDescTextField = transform.FindChild ("ItemDesc").GetComponent<Text> ();
			
		if (itemImage == null) {
			itemImage = Resources.Load <Sprite> ("ItemSprites/DefaultImage");
		}

		if (itemCost <= 0.01f) {
			itemCost = 0.01f;
		}

		if (itemCost >= 99.99f) {
			itemCost = 99.99f;
		}

		itemImageField.sprite = itemImage;

		if (itemName.Length > 100) {
			itemName = itemName.Substring(0,99);
		}

		itemNameTextField.text = itemName;


		itemCostTextField.text = string.Format("{0:N}", itemCost);
		itemCostTextField.text = CurrencyCodeMapper.GetCurrencySymbol (StoreProperties.INSTANCE.currencyCode) + itemCostTextField.text;

		itemCurCodeTextField.text = "(" + StoreProperties.INSTANCE.currencyCode + ")";

		itemDescTextField.text = itemDesc;

	}


	public void BuyItemAction() {
		Debug.Log ("Tried to buy a "  + itemName);
		StoreActions.INSTANCE.OpenPurchaseItemScreen (this);
	}
	
}
