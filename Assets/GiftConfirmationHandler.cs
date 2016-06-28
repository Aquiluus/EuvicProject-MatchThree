using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GiftConfirmationHandler : MonoBehaviour {

    public GameObject[] giftIcons;
    public Text playerName;
    public Text giftType;
    private bool giftConfirmed;
    public Button yesButton;
    public Button noButton;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void SetValues(string playerName, string giftType, int giftId)
    {
        Debug.Log("GiftHandler: " + playerName + " " + giftType);
        this.playerName.text = playerName;
        this.giftType.text = giftType;
        giftIcons[giftId].gameObject.SetActive(true);
    }

   public void CancelGift()
   {
       foreach (GameObject it in giftIcons)
       {
           it.SetActive(false);
       }
       this.GetComponent<CPanel>().SetActive(false);
   }
}
