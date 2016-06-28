using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GiftHandler : MonoBehaviour {

    public GameObject[] giftIcons;
    public Text playerName;
    public Text giftType;
    private bool giftAccepted;
    public Button AcceptGiftButton;
    private int giftId;

	// Use this for initialization
	void Start () {
	    UnityEngine.Events.UnityAction acceptGiftAction = () => {this.AcceptGift();};
        AcceptGiftButton.GetComponent<Button>().onClick.AddListener(acceptGiftAction);
        if (this.name.Contains("GiftReceivedPopUpNot"))
        {
            UnityEngine.Events.UnityAction additionalAction = () => { UIServer.main.ShowPage("MainMenu"); };
            AcceptGiftButton.GetComponent<Button>().onClick.AddListener(additionalAction);
        }
        else
        {
            UnityEngine.Events.UnityAction additionalAction = () => { UIServer.main.ShowPage("FacebookIntegration"); };
            AcceptGiftButton.GetComponent<Button>().onClick.AddListener(additionalAction);
        }
        giftAccepted = false;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void SetValues(string playerName, string giftType, int giftId)
    {
        Debug.Log("GiftHandler: " + playerName + " " + giftType);
        this.playerName.text = playerName;
        this.giftType.text = giftType;
        this.giftId = giftId;
        giftIcons[giftId].gameObject.SetActive(true);
    }

    public void AcceptGift()
    {
        if (!giftAccepted)
        {
            gameObject.GetComponent<CPanel>().SetActive(false);
            GameObject.FindGameObjectWithTag("FBHolder").GetComponent<FBHandler.FBHolder>().AcceptGift(giftId);
            if (!this.name.Contains("GiftReceivedPopUpNot"))
            {
                GameObject.Destroy(this.gameObject);
            }
            giftAccepted = true;
        }
    }
}
