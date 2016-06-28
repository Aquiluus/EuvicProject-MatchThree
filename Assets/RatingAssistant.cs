using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Soomla.Store;
using System;
using UnityEngine.EventSystems;

public class RatingAssistant : MonoBehaviour {

    public static RatingAssistant main;

    public Text rateText;
    public GameObject doYouLikePopUp;
    public Button yes;
    public Button no;
    public Button collect;
    public UniRate uniRate;
    public GameObject goldReceivedPopup;

    private DateTime firstPlayDate;
    private String firstPlayDateKey = "firstPlayDateKey";
    private TimeSpan twoDaysPlus;
    private TimeSpan threeDaysPlus;
    public bool wasRated = false;
    private string wasRatedKey = "wasRated";
    private bool didLike = false;

    void Awake()
    {
        main = this;
        twoDaysPlus = new TimeSpan(1,0,0,0);
        threeDaysPlus = new TimeSpan(2, 0, 0,0);
        if (PlayerPrefs.HasKey(wasRatedKey))
        {
            wasRated = bool.Parse(PlayerPrefs.GetString(wasRatedKey));
        }
        if(!PlayerPrefs.HasKey(firstPlayDateKey))
        {
            PlayerPrefs.SetString(firstPlayDateKey, DateTime.Now.ToString());
        }
    }

    public int CheckRatingDate()
    {
        if (PlayerPrefs.HasKey(firstPlayDateKey) && !wasRated)
        {
            DateTime now = DateTime.Now;
            firstPlayDate = DateTime.Parse(PlayerPrefs.GetString(firstPlayDateKey));
            if (now >= (firstPlayDate + twoDaysPlus) && now < (firstPlayDate + threeDaysPlus))
            {
                return 1;
            }
            else if (now >= (firstPlayDate + threeDaysPlus))
            {
                return 2;
            }
            else
            {
                return 0;
            }
        }
        return 0;
    }

    public void ShowLikePopUp(bool haveWon)
    {  
        if (!wasRated)
        {
            if (goldReceivedPopup.activeSelf)
            {
                goldReceivedPopup.SetActive(false);
            }
            doYouLikePopUp.GetComponent<CPanel>().SetActive(true);
            if (haveWon)
            {
                UnityEngine.Events.UnityAction acceptGiftAction = () => { UIServer.main.ShowPage("YouWin"); };
                collect.GetComponent<Button>().onClick.AddListener(acceptGiftAction);
            }
            else
            {
                UnityEngine.Events.UnityAction acceptGiftAction = () => { UIServer.main.ShowPage("YouLose"); };
                collect.GetComponent<Button>().onClick.AddListener(acceptGiftAction);
            }
        }
    }

    public void LikeYes()
    {
        setValues(true, "We are happy that you like our game :) Please rate us.");
        UnityEngine.Events.UnityAction acceptGiftAction = () => { uniRate.PromptIfNetworkAvailable(); };
        collect.GetComponent<Button>().onClick.AddListener(acceptGiftAction);
    }

    public void LikeNo()
    {
        setValues(false, "Send us email what can we do to make this game better: nonamed07@gmail.com. Have a gift for your time.");
        rateText.fontSize = 25;
    }

    private void setValues(bool like, string message)
    {
        didLike = like;
        yes.gameObject.SetActive(false);
        no.gameObject.SetActive(false);
        collect.gameObject.SetActive(true);
        rateText.text = message;
    }

    public void CollectGift()
    {
        if (didLike)
        {
            Debug.Log("Granting gold!");
            wasRated = true;
            StoreInventory.GiveItem("gold", 90);
        }
        else
        {
            wasRated = true;
            StoreInventory.GiveItem("gold", 30);
        }
        PlayerPrefs.SetString(wasRatedKey, wasRated.ToString());
    }
}
