using UnityEngine;
using System.Collections;

public class InventoryItemClickEvent : MonoBehaviour {

	public RectTransform borderImage;
	
	Vector2 centrePointOfItemIcon;
	Vector2 topLeftPointOfItemIcon;
	
	// Use this for initialization
	void Start () {
		
		Vector3 rawIconPos = GetComponent<RectTransform> ().position;
		
		topLeftPointOfItemIcon = new Vector2 (rawIconPos.x, rawIconPos.y);
		centrePointOfItemIcon = new Vector2 (topLeftPointOfItemIcon.x + GetComponent<RectTransform> ().sizeDelta.x / 2f, topLeftPointOfItemIcon.y - GetComponent<RectTransform> ().sizeDelta.y / 2f);
		
	}
	
	// Update is called once per frame
	void Update () {
		
		if (Input.GetMouseButtonDown (0)) {

			
			float distFromCentre = Vector2.Distance(new Vector2(Input.mousePosition.x, Input.mousePosition.y), centrePointOfItemIcon);
			
			if (distFromCentre < 20f) {
				
				borderImage.SetParent(GetComponent<RectTransform>());
				borderImage.localPosition = new Vector2(-1f,1f);
				
				for (int i=0; i<InventoryActions.INSTANCE.itemSlots.Length;i++) {
					
					if (transform.parent == InventoryActions.INSTANCE.itemSlots[i].transform) {
						InventoryActions.INSTANCE.setSelectedSlotIndex(i);
						InventoryActions.INSTANCE.refreshCurrentDisplayedItem();
					}
					
				}
			
			}
			
		}
		
		
	}
}
