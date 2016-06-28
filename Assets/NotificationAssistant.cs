using UnityEngine;
using System.Collections;
using System;

public class NotificationAssistant : MonoBehaviour {

    public const long WEEK_SECONDS = 604800;
    public const long DAY_SECONDS = 86400;

    public GiftHandler giftReceivedPopUp;
    public GameObject giftHolder;

    public string[] notifTextsRegular = {"Come back and play! There is a gift waiting for You!", 
                                     "Haven't seen You for a while, come back!",
                                     "There is a gift waiting for You. Come and claim it",
                                     "Got a time for some berry adventure? Come back and play!",
                                     "Why not play a bit? There is a gift if You do!"};

    public string[] notifTextsFriday = {"Why not embark on an adventure before weeknd?",
                                           "Come, play and relax before weekend"};

    public string[] notifTextsSaturday = {"Weekend! Sounds like a good time for an adventure.",
                                             "No better time than weekend for some berry popping!",
                                             "Don't abandon your quest on weekend!"};

    private string giftType = "notGiftType";
    private string giftDate = "giftDate";
    private string giftDueDate = "giftDueDate";

    private string firstDayPlayedKey = "firstDayPlayed";

    private TimeSpan oneDayTimeSpan;
    private DateTime firstDayPlayed;

    private int idModifier = 2048;

    private int[] dayTable = {1,2,3,5,8,11,16,23};
    private bool giftSet;

    public enum GiftType
    {
        Seeds, Gold, Moves, Life, Cut, Bombs, Switch, Shake, Magic
    };

    public struct Gift
    {
        public GiftType giftType;
        public int amount;
        public string giftName;
        public string giftStoreId;

        public Gift(GiftType giftType, int amount, string giftName, string giftStoreId)
        {
            this.giftType = giftType;
            this.amount = amount;
            this.giftName = giftName;
            this.giftStoreId = giftStoreId;
        }
    }

    private Gift[] giftsDatabase;


    void Start()
    {
        oneDayTimeSpan = new TimeSpan(0, 24, 0, 0, 0);
        SetupGifts();
        CheckForGift();
        DateTime now = DateTime.Now;
        if (!PlayerPrefs.HasKey(firstDayPlayedKey))
        {
            PlayerPrefs.SetString(firstDayPlayedKey, now.ToString());
            firstDayPlayed = now;
            SetNonRepeatingNotifications();
            SetRepeatingNotification();
            SetGift(now, DAY_SECONDS);
        }
        else
        {
            firstDayPlayed = DateTime.Parse(PlayerPrefs.GetString(firstDayPlayedKey));
            if (!PlayerPrefs.HasKey(giftType))
            {
                SetUpGiftRecurring(now);
            } 
        }

    }

    private void SetNonRepeatingNotifications()
    {
        SetNotification(1,DAY_SECONDS);
        SetNotification(2,2*DAY_SECONDS);
        SetNotification(3,3*DAY_SECONDS);
        SetNotification(5,5*DAY_SECONDS);
        SetNotification(8,8*DAY_SECONDS);
        SetNotification(11,11*DAY_SECONDS);
        SetNotification(16,16*DAY_SECONDS);

    }

    private void SetRepeatingNotification()
    {
        LocalNotification.SendRepeatingNotification(30 + idModifier, 23 * DAY_SECONDS, WEEK_SECONDS, "Jewels fruits",
                    "Hey! Come back and pop some berries!", new Color32(0xff, 0x44, 0x44, 255));
    }

    private void SetUpGiftRecurring(DateTime now)
    {
        TimeSpan diff = now - firstDayPlayed;
        int diffDays = Convert.ToInt32(Math.Floor(diff.TotalDays));
        Debug.LogWarning("After conversion: " + diffDays);
        for(int i = 0; i <dayTable.Length-1;i++)
        {
            if (diffDays > dayTable[i] && diffDays < dayTable[i + 1]) 
            {
                DateTime temp = firstDayPlayed +  TimeSpan.FromTicks(oneDayTimeSpan.Ticks * dayTable[i + 1]);
                TimeSpan setSeconds = temp - now;
                SetGift(now, (long)Math.Floor(setSeconds.TotalSeconds));
                giftSet = true;
            }
        }
        if (!giftSet)
        {
            int dayFloor = 23;
            int dayCeiling = 30;
            while(!giftSet)
            {
                if (diffDays > dayFloor  && diffDays < dayCeiling )
                {
                    DateTime temp = firstDayPlayed + TimeSpan.FromTicks(oneDayTimeSpan.Ticks * dayCeiling);
                    TimeSpan setSeconds = temp - now;
                    SetGift(now, (long)Math.Floor(setSeconds.TotalSeconds));
                    giftSet = true;
                }
                dayFloor += 7;
                dayCeiling += 7;
            }
        }
    }

    private void SetupGifts()               
    {
        Gift[] gifts = { new Gift(GiftType.Seeds, 50,"Seeds pack","seed"), 
                               new Gift(GiftType.Gold, 10, "Gold pack","gold"), 
                               new Gift(GiftType.Moves, 1, "Moves pack","move"),
                               new Gift(GiftType.Life, 1, "Life pack","life"), 
                               new Gift(GiftType.Cut, 1, "Cut","cut"), 
                               new Gift(GiftType.Bombs, 1, "Bombs","freebombs"), 
                               new Gift(GiftType.Switch,1, "Switch","switch"), 
                               new Gift(GiftType.Shake, 1, "Shake","shake"), 
                               new Gift(GiftType.Magic, 1, "Magic Finger","finger") };

        giftsDatabase = gifts;
    }


    private void SetNotification(int id, long seconds)
    {
        //DebugPanel.Log("Setting notification", seconds);

        LocalNotification.SendNotification(id + idModifier, seconds, "Jewels Fruits", GetRandomNotification(),
           new Color32(0xff, 0x44, 0x44, 255), true, true, true, "app_icon");

    }

    private void SetGift(DateTime now, long seconds)
    {
        DebugPanel.Log("Setting Gift", now);
        DebugPanel.Log("Gift Due Date: ", seconds);
        PlayerPrefs.SetString(giftDate, now.ToString());
        int temp = UnityEngine.Random.Range(0, 8);
        DebugPanel.Log("GiftType: ", temp);
        PlayerPrefs.SetInt(giftType,temp);
        PlayerPrefs.SetString(giftDueDate, seconds.ToString());
    }

    private string GetRandomNotification()
    {
        System.Random random = new System.Random();
        int i = random.Next(0, notifTextsRegular.Length - 1);
        return notifTextsRegular[i];
    }

    private void CheckForGift()
    {
        if(PlayerPrefs.HasKey(giftType))
        {
            string giftDate = PlayerPrefs.GetString(this.giftDate);
            DateTime tempDate = DateTime.Parse(giftDate);
            TimeSpan timeSpan = DateTime.Now - tempDate;
            string giftDueDate = PlayerPrefs.GetString(this.giftDueDate);
            long tempSeconds = long.Parse(giftDueDate);
            //DebugPanel.Log("TempSeconds: ", tempSeconds);
            //DebugPanel.Log("TotalSeconds: ", timeSpan.TotalSeconds);
            //DebugPanel.Log("Warunek: ", timeSpan.TotalSeconds > tempSeconds);
            if (timeSpan.TotalSeconds > tempSeconds)
            {
                int giftTypeTemp = PlayerPrefs.GetInt(giftType);
                DebugPanel.Log("Receiving gift: ", giftsDatabase[giftTypeTemp].giftName);
                GiftHandler giftHandler = Instantiate(giftReceivedPopUp) as GiftHandler;
                giftHandler.SetValues("You have received a gift!",giftsDatabase[giftTypeTemp].giftName, giftTypeTemp);
                giftHandler.transform.SetParent(giftHolder.transform, false);
                giftHandler.gameObject.SetActive(false);
                giftHandler.GetComponent<CPanel>().SetActive(true);
                giftSet = false;
                PlayerPrefs.DeleteKey(giftDate);
                PlayerPrefs.DeleteKey(giftType);
                PlayerPrefs.DeleteKey(giftDueDate);
            }
        }
    }
}
