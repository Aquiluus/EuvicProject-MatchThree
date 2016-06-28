using UnityEngine;
using UnityEngine.UI;
using Facebook.MiniJSON;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Parse;
using Soomla.Store;

namespace FBHandler
{
    [ThreadSafe]
    public class FBHolder : MonoBehaviour
    {
        public static bool inviteLoaded, leaderboardLoaded, giftLoaded;

        public GameObject FBIsLoggedIn;                         //Reference to a canvas handling FB logged in status
        public GameObject FBIsNotLoggedIn;                     //Reference to a canvas handling FB not logged in status
        public GameObject FacebookButton;                     //Reference to a button with which You enter the login screen      
        public GameObject ShareScoreCounter;                 //Reference to a gameObject that is dealing with score counting in order to share the score
        public GameObject ShareLevelCounter;                //Reference to a gameObject that is dealing with level counting in order to share the level
        public GameObject NoInternetConnectionScreen;       //Reference to a screen shown upon not having a internet connection
        public GameObject InternetConnectionScreen;         //Reference to a screen shown upon having a internet connection
        public GameObject InviteView;                       //Reference to a gameObject holding the scroll view with friends(see prefabs)
        public GameObject GiftView;                         //Reference to a gameObject holding the scroll view with gifts(see prefabs)
        public GameObject GiftSelectionPopUp;               //Reference to a gameObject holding the PopUp for selecting gifts(see prefabs)
        public GiftHandler GiftReceivedPopUp;               //Reference to a gameObject holding the PopUp for receiving gifts(see prefabs)
        public GameObject giftHolder;                       //Reference to a gameObject under which the PopUp messages should spawn
        public GameObject giftConfirmationPopUp;
        public GameObject giftAcknowledgmentPopUp;
        public GameObject loadingScreen;
        public GameObject sendGiftButton;

        public Sprite[] stateSprites;                       //Array of sprites for checking friends on invite screen(see Srites
    

        public ToggleState tglStateSlctAll = ToggleState.Unchecked;     
        //Reference to Invite Button and select All- toggler
        public GameObject btnInvite, btnSlctAll;
        // List of the invite and leaderboard list items

        // List containers that list Items - (Dynamically Increasing ListView <Custom>)
        public GameObject listInviteContainer, listLeaderboardContainer;
        //Prefabs that hold items that will be places in the containers.
        public ListItemInvite itemInvitePref;
        public ListItemLeaderboard itemLeaderPref;


        public GameObject leaderboardView;          //Reference to a gameObject holding the PopUp for showing the HighScores(See prefabs)
        public GameObject notLoggedInPopUp;         //Reference to a gameOject holding the PopUp for not logged in status
        public Text levelNumberText;                //Reference to a Text object in the leaderboard PopUp(in prefab for Leaderboard view)
        public Text levelCounterTextWinPopup;       //Reference to a Text objcet holding the level number shown on the WinPopup
        public Text levelCounterTextSelectLevel;    //Reference to a Text object holding the level number on the level selection PopUp

        public GameObject listGiftContainer;        //Reference to a gameobject holding the container for the gifts screen(See GiftView prefab)
        public ListItemGift itemGiftPref;           //Reference to a prefab showing the single gift selection item

        static List<ListItemGift> listGifts = new List<ListItemGift>();
        public static List<ListItemInvite> listInvites = new List<ListItemInvite>();
        static List<ListItemLeaderboard> listLeaderboard = new List<ListItemLeaderboard>();
        static List<LeaderboardListItem> leaderboardCheatList = new List<LeaderboardListItem>();
        static List<GiftListItem> queriedGiftsList = new List<GiftListItem>();

        private bool isConnectedToInternet;
        private float delay = 5;
        private float nextUsage;
        private bool isFBInitialized = false;
        private string playerName = "";
        private string playerPictureURL = "";
        private string playerID = "";
        private static bool parseScoreProcessFinished = false;
        private static bool parseGiftProcessFinished = false;
        private bool checkedForGifts;
        private bool postLoginProcessesFinished = false;

        private Gift[] giftsDatabase;

        private Dictionary<string, string> profile = null;

        private ParseObject currentGift;
        private TimeSpan oneDayTimeSpan = new TimeSpan(24, 0, 0);

        private string loadLeaderboardString = "app/scores", //loadInvitableFriendsString = "v2.0/me/invitable_friends";
            loadInvitableFriendsString = "v2.0/me/invitable_friends?limit=200";

        public enum ToggleState
        {
            Unchecked, Partial, Checked
        };

        public enum GiftType
        {
            Seeds, Gold, Moves, Life, Cut, Bombs, Switch, Shake, Magic
        };

        public struct LeaderboardListItem
        {
            public string id;
            public string playerName;
            public string score;
        }

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

        public struct GiftListItem
        {
            public string sender;
            public string giftType;
            public int giftId;
            public string giftStoreId;
        }

        void Awake()
        {
            DontDestroyOnLoad(gameObject.transform);
            NoInternetConnectionScreen.SetActive(true);
            InternetConnectionScreen.SetActive(false);
            isConnectedToInternet = false;
            SetupGifts();
            Time.timeScale = 1;
            CheckForInternetConnection();
            if(isConnectedToInternet)
            {
                FB.Init(SetInit, OnHideUnity);
                isFBInitialized = true;
            }
        }

        private void SetupGifts()               //Here a "giftDatabase" is created. Setup to ypur liking/needs. Also edit the enum for holding the gift type. 
                                                //First value is the enum type, second the amount, third the display name and lastly the shop id(for me it was soomla)
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

        void Start()
        {
            nextUsage = Time.time + delay;
            GiftView.SetActive(false);
            InviteView.SetActive(true);
        }

        void CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                using (var stream = client.OpenRead("http://www.google.com"))
                {
                    if(!isConnectedToInternet)
                    { 
                        NoInternetConnectionScreen.SetActive(false);
                        InternetConnectionScreen.SetActive(true);
                        isConnectedToInternet = true;
                        //FB.Logout();
                    }
                    
                }
            }
            catch
            {
                if (isConnectedToInternet)
                {
                    NoInternetConnectionScreen.SetActive(true);
                    InternetConnectionScreen.SetActive(false);
                    isConnectedToInternet = false;
                }
            }
        }

        void Update()
        {
            if (Time.time > nextUsage)
            {
                CheckForInternetConnection();
                nextUsage = Time.time + delay;
            }
            if (isConnectedToInternet)
            {
                if (isFBInitialized)
                {
                    if (FB.IsLoggedIn)
                    {
                        FBIsNotLoggedIn.SetActive(false);
                        FBIsLoggedIn.SetActive(true);
                        if (!checkedForGifts && postLoginProcessesFinished)
                        {
                            HandleGiftOperations();
                            checkedForGifts = true;
                        }

                    }
                    else
                    {
                        FBIsNotLoggedIn.SetActive(true);
                        FBIsLoggedIn.SetActive(false);
                    }
                }
                else
                {
                    isFBInitialized = true;
                    FB.Init(SetInit, OnHideUnity);
                }
            }
            else
            {
                StartCoroutine(netCheckCoroutine());

            }
        }

        IEnumerator netCheckCoroutine()
        {
            CheckForInternetConnection();
            yield return new WaitForSeconds(10f);
        }

        private void SetInit()
        {
            Debug.Log("FB init done.");

            if (FB.IsLoggedIn)
            {
                FB.API(Util.GetPictureURLB("me", 128, 128), Facebook.HttpMethod.GET, DealWithProfilePicture);
                FB.API("/me?fields=id,first_name,last_name", Facebook.HttpMethod.GET, DealWithUserName); 
                FBIsNotLoggedIn.SetActive(false);
                FBIsLoggedIn.SetActive(true);
                LoadInvitableFriends();
            }
        }

        private void OnHideUnity(bool isGameShown)
        {
            if (!isGameShown)
            {
                Time.timeScale = 0;
            }
            else
            {
                Time.timeScale = 1;
            }
        }

        public void OnButtonClick()
        {
            if (!FB.IsLoggedIn)
            {
                Debug.Log("Try Login");
                FB.Login("email,publish_actions,user_friends", AuthCallback);
            }
            else
            {
                FBIsNotLoggedIn.SetActive(false);
                FBIsLoggedIn.SetActive(true);

            }


        }

        void AuthCallback(FBResult result)
        {
            if (FB.IsLoggedIn)
            {

                Debug.Log("FB login worked.");
                FB.API(Util.GetPictureURLB("me", 128, 128), Facebook.HttpMethod.GET, DealWithProfilePicture);
                FB.API("/me?fields=id,first_name,last_name", Facebook.HttpMethod.GET, DealWithUserName);
                LoadInvitableFriends();
            }
            else
            {
                Debug.Log("FB login failed.");
            }
        }

        void DealWithProfilePicture(FBResult result)
        {
            if (result.Error != null)
            {
                Debug.Log("Problem with getting the profile picture");

                FB.API(Util.GetPictureURLB("me", 128, 128), Facebook.HttpMethod.GET, DealWithProfilePicture);
                return;
            }

            Sprite temporaryAvatar = Sprite.Create(result.Texture, new Rect(0, 0, 128, 128), new Vector2(0, 0));
            Texture2D popupIconTexture = new Texture2D((int)temporaryAvatar.rect.width, (int)temporaryAvatar.rect.height);

            var pixels = temporaryAvatar.texture.GetPixels((int)temporaryAvatar.textureRect.x,
                                                    (int)temporaryAvatar.textureRect.y,
                                                    (int)temporaryAvatar.textureRect.width,
                                                    (int)temporaryAvatar.textureRect.height);

            popupIconTexture.SetPixels(pixels);
            popupIconTexture.Apply();


            popupIconTexture = CalculateTexture(128, 128, 64, 64, 64, popupIconTexture);
            temporaryAvatar = Sprite.Create(popupIconTexture, new Rect(0, 0, 128, 128), new Vector2(0, 0));
            Image userAvatar = FacebookButton.GetComponent<Image>();
            userAvatar.sprite = temporaryAvatar;
            postLoginProcessesFinished = true;
        }

        void DealWithUserName(FBResult result)
        {
            if (result.Error != null)
            {
                Debug.Log("Problem with Username");
            }
            Debug.Log(result.Text);

            profile = Util.DeserializeJSONProfile(result.Text);
            playerName = profile["first_name"] + " " + profile["last_name"];
            //Debug.LogWarning(playerName);
            playerPictureURL = "https://graph.facebook.com/" + profile["id"] + "/picture";
            playerID = profile["id"];
            Debug.Log(playerPictureURL);
        }

        public void InviteFriends()
        {
            FB.AppRequest(
                message: "Join me in this great Berry Adventure!",
                title: "Invite Your friends to join You in game!"
                );
        }

        public void ShareWithFriends()
        {
            if (FB.IsLoggedIn)
            {
                string shareMessage = "I just scored " + ShareScoreCounter.GetComponent<Text>().text + " points on \n" + ShareLevelCounter.GetComponent<Text>().text + ". You think You can beat me?";

                FB.Feed(
                    linkCaption: "New Jewels Fruits High score!",
                    picture: "https://lh3.googleusercontent.com/AJM4lHx7XgTO1S9fpIhCaiF4emgGnU_KMBW3jvnwv-hX0UhlNLTsLxFgFg1JIcu9lF4=w300",
                    linkName: shareMessage,
                    link: "https://apps.facebook.com/" + FB.AppId + "/?challenge_brag=" + (FB.IsLoggedIn ? FB.UserId : "guest"));
            }
            else
            {
                notLoggedInPopUp.gameObject.GetComponent<CPanel>().SetActive(true);
            }
        }

        Texture2D CalculateTexture(
         int h, int w, float r, float cx, float cy, Texture2D sourceTex
         )
        {
            Color[] c = sourceTex.GetPixels(0, 0, sourceTex.width, sourceTex.height);
            Texture2D b = new Texture2D(h, w);
            for (int i = 0; i < (h * w); i++)
            {
                int y = Mathf.FloorToInt(((float)i) / ((float)w));
                int x = Mathf.FloorToInt(((float)i - ((float)(y * w))));
                if (r * r >= (x - cx) * (x - cx) + (y - cy) * (y - cy))
                {
                    b.SetPixel(x, y, c[i]);
                }
                else
                {
                    b.SetPixel(x, y, Color.clear);
                }
            }
            b.Apply();
            return b;
        }

        //Click Handler of Select ALl Button
        public void TglSelectAllClickHandler()
        {
            switch (tglStateSlctAll)
            {
                case ToggleState.Partial:
                case ToggleState.Unchecked:
                    foreach (var item in listInvites)
                    {
                        item.tglBtn.isOn = true;
                    }
                    tglStateSlctAll = ToggleState.Checked;
                    ChangeToggleState(ToggleState.Checked);
                    break;
                case ToggleState.Checked:
                    foreach (var item in listInvites)
                    {
                        item.tglBtn.isOn = false;
                    }
                    ChangeToggleState(ToggleState.Unchecked);
                    break;
            }
        }
        //Method to change Toggle State On the Fly
        public void ChangeToggleState(ToggleState state)
        {
            switch (state)
            {
                case ToggleState.Unchecked:
                    tglStateSlctAll = state;
                    btnSlctAll.GetComponent<Image>().sprite = stateSprites[0];
                    break;
                case ToggleState.Partial:
                    bool flagOn = false, flagOff = false;
                    foreach (var item in listInvites)
                    {
                        if (item.tglBtn.isOn)
                        {
                            flagOn = true;
                        }
                        else
                        {
                            flagOff = true;
                        }
                    }
                    if (flagOn && flagOff)
                    {
                        tglStateSlctAll = state;
                        btnSlctAll.GetComponent<Image>().sprite = stateSprites[1];
                        //Debug.Log("Partial");
                    }
                    else if (flagOn && !flagOff)
                    {
                        ChangeToggleState(ToggleState.Checked);
                        //Debug.Log("Checked");
                    }
                    else if (!flagOn && flagOff)
                    {
                        ChangeToggleState(ToggleState.Unchecked);
                        //Debug.Log("Unchecked");
                    }
                    break;
                case ToggleState.Checked:
                    tglStateSlctAll = state;
                    btnSlctAll.GetComponent<Image>().sprite = stateSprites[2];
                    break;
            }
        }

        delegate void LoadPictureCallback(Texture2D texture, int index);
        //Method to load leaderboard
        public void LoadLeaderboard()
        {
            if (!FB.IsLoggedIn || !FB.IsInitialized)
            {
                notLoggedInPopUp.gameObject.GetComponent<CPanel>().SetActive(true);
            }
            else
            {
                if (levelCounterTextSelectLevel.IsActive())
                {
                    leaderboardView.SetActive(true);
                    levelNumberText.text = levelCounterTextSelectLevel.text;
                    CallBackLoadLeaderboard();

                }
                else if(levelCounterTextWinPopup.IsActive())
                {
                    leaderboardView.SetActive(true);
                    levelNumberText.text = levelCounterTextWinPopup.text;
                    CallBackLoadLeaderboard();
                }
            }
        }

        public void CloseLeaderboard()
        {
            leaderboardView.SetActive(false);
            listLeaderboard = new List<ListItemLeaderboard>();
            for (int i = 0; i < listLeaderboardContainer.gameObject.transform.childCount; i++)
            {
                GameObject.Destroy(listLeaderboardContainer.transform.GetChild(0).gameObject);
            }
        }
        //callback of from Facebook API when the leaderboard data from the server is loaded.
        void CallBackLoadLeaderboard()
        {
            leaderboardLoaded = false;
            parseScoreProcessFinished = false;
            foreach(Transform child in leaderboardView.transform.GetChild(2).GetChild(0))
            {
                GameObject.Destroy(child.gameObject);
            }
            leaderboardCheatList = new List<LeaderboardListItem>();
            StartCoroutine(GetDataFromCloud());
            StartCoroutine(LoadLeaderboardItemsFromList());
            leaderboardLoaded = true;
            
        }

        IEnumerator GetDataFromCloud()
        {
            var query = ParseObject.GetQuery("GameScore").WhereEqualTo("levelNumber", levelNumberText.text);
            query.FindAsync().ContinueWith(t =>
            {
                if (!t.IsFaulted)
                {
                    IEnumerable<ParseObject> gameScores = t.Result;
                    foreach (ParseObject gameScore in gameScores)
                    {
                        LeaderboardListItem tmpItem = new LeaderboardListItem();
                        tmpItem.score = gameScore["scoreCount"].ToString();
                        tmpItem.playerName = gameScore["playerName"].ToString();
                        tmpItem.id = gameScore["playerID"].ToString();
                        leaderboardCheatList.Add(tmpItem);
                        //Debug.Log("Item: " + ", Id: " + tmpItem.id + ", Name: " + tmpItem.playerName + ", Score: " + tmpItem.score);
                    }
                    leaderboardCheatList.Sort(
                        delegate(LeaderboardListItem p1, LeaderboardListItem p2)
                        {
                            return p2.score.CompareTo(p1.score);
                        }
                        );
                    parseScoreProcessFinished = true;
                }
            });
            yield return new WaitForSeconds(0.5f);
        }

        IEnumerator LoadLeaderboardItemsFromList()
        {
            while (!parseScoreProcessFinished)
            {
                yield return new WaitForSeconds(0.5f);
            }
            if (parseScoreProcessFinished)
            {
                int i = 0;
                foreach (LeaderboardListItem item in leaderboardCheatList)
                {
                    CreateListItemLeaderboard(item.id, item.playerName, item.score);
                    LoadFriendsAvatar(i);
                    i++;
                }
            }
        } 

        // Method to load Friends Profile Pictures
        void LoadFriendsAvatar(int index)
        {
            FB.API(Util.GetPictureURLA(listLeaderboard[index].fId), Facebook.HttpMethod.GET, result =>
            {
                if (result.Error != null)
                {
                    Util.LogError(result.Error);
                    return;
                }
                listLeaderboard[index].picUrl = Util.DeserializePictureURLString(result.Text);
                StartCoroutine(LoadFPicRoutine(listLeaderboard[index].picUrl, PicCallBackLeaderboard, index));
            });
        }
        // Method that Proceeds with the Invitable Friends
        public void LoadInvitableFriends()
        { 
            FB.API(loadInvitableFriendsString, Facebook.HttpMethod.GET, CallBackLoadInvitableFriends);
        }
        //Callback of Invitable Friends API Call
        void CallBackLoadInvitableFriends(FBResult result)
        {
            //Deserializing JSON returned from server
            Debug.Log("Data from JSON");
            Debug.Log(result.Text);
            Dictionary<string, object> JSON = Json.Deserialize(result.Text) as Dictionary<string, object>;
            Debug.Log(JSON["data"]);
            List<object> data = JSON["data"] as List<object>;
            StartCoroutine(ShowLoadingScreen());
            //Loop to traverse and process all the items returned from the server.
            for (int i = 0; i < data.Count; i++)
            {
                string id = System.Convert.ToString(((Dictionary<string, object>)data[i])["id"]);
                string name = System.Convert.ToString(((Dictionary<string, object>)data[i])["name"]);
                Dictionary<string, object> picInfo = ((Dictionary<string, object>)data[i])["picture"] as Dictionary<string, object>;
                string url = Util.DeserializePictureURLObject(picInfo);
                CreateListItemInvite(id, name, url);
                StartCoroutine(LoadFPicRoutine(url, PicCallBackInvitable, i));
            }
            RemoveDupilcates();
            btnInvite.SetActive(true);
            inviteLoaded = true;
        }

        void RemoveDupilcates()
        {
            int counter = 0;
            List<ListItemInvite> tempList = listInvites.Distinct().ToList();
            foreach(ListItemInvite invite in listInvites)
            {
                if(counter >= tempList.Count)
                {
                    GameObject.Destroy(invite.gameObject);
                }
                counter++;
            }
            listInvites = tempList;
        }

        IEnumerator ShowLoadingScreen()
        {
            float timePassed = 0.0f;
            loadingScreen.SetActive(true);
            while (timePassed < 7)
            {
                timePassed += 1.5f;
                yield return new WaitForSeconds(1.5f);
            }
            loadingScreen.SetActive(false);
            yield return new WaitForSeconds(0.5f);
        }

        //Method to add item to the custom invitable dynamically scrollable list
        void CreateListItemInvite(string id, string fName, string url = "")
        {
            ListItemInvite tempItem = Instantiate(itemInvitePref) as ListItemInvite;
            tempItem.fId = id;
            tempItem.picUrl = url;
            tempItem.txtName.text = fName;
            tempItem.transform.SetParent(listInviteContainer.transform, false);
            listInvites.Add(tempItem);
        }
        //Method to all items to the leaderboard dynamically scrollable list
        void CreateListItemLeaderboard(string id, string fName, string fScore = "")
        {
            ListItemLeaderboard tempItem = Instantiate(itemLeaderPref) as ListItemLeaderboard;
            tempItem.fId = id;
            tempItem.txtName.text = fName;
            tempItem.txtScore.text = fScore;
            tempItem.transform.SetParent(listLeaderboardContainer.transform, false);
            listLeaderboard.Add(tempItem);
        }
        //Coroutine to load Picture from the specified URL
        IEnumerator LoadFPicRoutine(string url, LoadPictureCallback Callback, int index)
        {
            WWW www = new WWW(url);
            yield return www;
            Callback(www.texture, index);
        }
        //Callback of Invitable Friend API call
        private void PicCallBackInvitable(Texture2D texture, int index)
        {
            if (texture == null)
            {
                StartCoroutine(LoadFPicRoutine(listInvites[index].picUrl, PicCallBackInvitable, index));
                return;
            }
            listInvites[index].imgPic.sprite = Sprite.Create(texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
                );
        }
        //Callback to Score API Call
        private void PicCallBackLeaderboard(Texture2D texture, int index)
        {
            if (texture == null)
            {
                StartCoroutine(LoadFPicRoutine(listLeaderboard[index].picUrl, PicCallBackLeaderboard, index));
                return;
            }
            listLeaderboard[index].imgPic.sprite = Sprite.Create(texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
                );
        }
        //ClickHandling Method that Sends Backend Facebook Native App request (Invitable)Calls
        public void SendInvites()
        {
            List<string> lstToSend = new List<string>();
            foreach (var item in listInvites)
            {
                if (item.tglBtn.isOn)
                {
                    lstToSend.Add(item.fId);
                }
            }
            int dialogCount = (int)Mathf.Ceil(lstToSend.Count / 50f);
            CallInvites(lstToSend, dialogCount);
        }
        //Helping method that will be recursive if you'll have to sent invites to more than 50 Friends.
        public string invMessage, invTitle;
        private void CallInvites(List<string> lstToSend, int dialogCount)
        {
            if (dialogCount > 0)
            {
                string[] invToSend = (lstToSend.Count >= 50) ? new string[50] : new string[lstToSend.Count];

                for (int i = 0; i < invToSend.Length; i++)
                {
                    try
                    {
                        if (lstToSend[i] != null)
                        {
                            invToSend[i] = lstToSend[i];
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e.Message);
                    }
                }
                lstToSend.RemoveRange(0, invToSend.Length);
                FB.AppRequest(
                    invMessage,
                    invToSend,
                    null,
                    null,
                    null,
                    "",
                    invTitle,
                    FBResult =>
                    {
                        if (--dialogCount > 0)
                        {
                            CallInvites(lstToSend, dialogCount);
                        }
                    }
                );
            }
        }
        public void LoadData()
        {
            for (int i = 0; i < listLeaderboard.Count; i++)
            {
                ListItemLeaderboard tmp = Instantiate(itemLeaderPref) as ListItemLeaderboard;
                tmp.AssignValues(listLeaderboard[i].fId, listLeaderboard[i].picUrl, listLeaderboard[i].txtName.text,
                listLeaderboard[i].txtScore.text, listLeaderboard[i].imgPic.sprite);
                tmp.transform.SetParent(listLeaderboardContainer.transform, false);
                listLeaderboard[i] = tmp;

            }
            for (int j = 0; j < listInvites.Count; j++)
            {
                ListItemInvite tmp = Instantiate(itemInvitePref) as ListItemInvite;
                tmp.AssignValues(listInvites[j].fId, listInvites[j].picUrl,
                    listInvites[j].txtName.text, listInvites[j].imgPic.sprite);
                tmp.transform.SetParent(listInviteContainer.transform, false);
                listInvites[j] = tmp;
            }
        }
        public void UploadResultToParse(string levelNumber, int scoreCount)
        {
            if (FB.IsLoggedIn && !playerName.Equals(""))
            {
                var query = ParseObject.GetQuery("GameScore").WhereEqualTo("playerName", playerName).WhereEqualTo("levelNumber", levelNumber);
                query.FirstAsync().ContinueWith(t =>
                    {
                        if (t.IsFaulted)
                        {
                            Debug.Log("Adding new score");
                            ParseObject gameScore = new ParseObject("GameScore");
                            gameScore["playerID"] = playerID;
                            gameScore["playerName"] = playerName;
                            gameScore["levelNumber"] = levelNumber;
                            gameScore["scoreCount"] = scoreCount;
                            gameScore["playerPictureURL"] = playerPictureURL;
                            gameScore.SaveAsync();
                        }
                        else
                        {
                            Debug.Log("Updating score");
                            ParseObject gameScore = t.Result;
                            if (int.Parse(gameScore["scoreCount"].ToString()) < scoreCount)
                            {
                                gameScore["scoreCount"] = scoreCount;
                                gameScore.SaveAsync();
                            }
                        }

                    }
                );
            }
        }

        public void CloseGiftView()
        {
            GiftView.SetActive(false);
            InviteView.SetActive(true);
            sendGiftButton.transform.GetChild(0).GetComponent<Text>().text = "Send gift";
        }

        public void OpenGiftView()
        {
            InviteView.SetActive(false);
            GiftView.SetActive(true);
            sendGiftButton.transform.GetChild(0).GetComponent<Text>().text = "Refresh list";
            foreach (Transform child in GiftView.transform.GetChild(0).GetChild(0))
            {
                GameObject.Destroy(child.gameObject);
            }
            listGifts = new List<ListItemGift>();
            FB.API(loadLeaderboardString, Facebook.HttpMethod.GET, SendGiftCallback);
        }

        public void SendGiftCallback(FBResult result)
        {
            //Deserializing JSON returned from server
            Dictionary<string, object> JSON = Json.Deserialize(result.Text) as Dictionary<string, object>;
            List<object> data = JSON["data"] as List<object>;

            if (result.Error != null)
            {
                FB.API(loadLeaderboardString, Facebook.HttpMethod.GET, SendGiftCallback);
                return;
            }
            //Loop to traverse and process all the items returned from the server.
            for (int i = 0; i < data.Count; i++)
            {
                Dictionary<string, object> UserInfo = ((Dictionary<string, object>)data[i])["user"] as Dictionary<string, object>;
                string name = System.Convert.ToString(((Dictionary<string, object>)UserInfo)["name"]);
                string id = System.Convert.ToString(((Dictionary<string, object>)UserInfo)["id"]);
                if (!name.Equals(playerName))
                {
                    CreateListItemGift(id, name);
                    LoadGiftAvatar(i);
                }
            }
            giftLoaded = true;
        }

        public void CreateListItemGift(string id, string fName)
        {
            int timeLeft = 0;
            ListItemGift tempItem = Instantiate(itemGiftPref) as ListItemGift;
            tempItem.fId = id;
            tempItem.txtName.text = fName;
            if (!CheckIfGiftCanBeSend(fName, ref timeLeft))
            {
                Debug.Log(timeLeft);
                tempItem.SetButton(false);
                tempItem.giftImage.enabled = false;
                tempItem.buttonText.text = timeLeft.ToString() + "h";
                tempItem.buttonText.enabled = true;
            }
            else
            {
                tempItem.SetButton(true);
            }
            tempItem.transform.SetParent(listGiftContainer.transform, false);
            listGifts.Add(tempItem);
        }

        private void LoadGiftAvatar(int index)
        {
            FB.API(Util.GetPictureURLA(listGifts[index].fId), Facebook.HttpMethod.GET, result =>
            {
                if (result.Error != null)
                {
                    Util.LogError(result.Error);
                    return;
                }
                listGifts[index].picUrl = Util.DeserializePictureURLString(result.Text);
                StartCoroutine(LoadFPicRoutine(listGifts[index].picUrl, PicCallBackGift, index));
            });
        }

        private void PicCallBackGift(Texture2D texture, int index)
        {
            if (texture == null)
            {
                StartCoroutine(LoadFPicRoutine(listGifts[index].picUrl, PicCallBackGift, index));
                return;
            }
            listGifts[index].imgPic.sprite = Sprite.Create(texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
                );
        }

        bool CheckIfGiftCanBeSend(string playerName, ref int timeLeft)
        {
            if (PlayerPrefs.HasKey(playerName))
            {
                DateTime oldDate = DateTime.Parse(PlayerPrefs.GetString(playerName));
                Debug.Log(oldDate.ToString());
                TimeSpan difference = DateTime.Now.Subtract(oldDate);
                if (difference > oneDayTimeSpan)
                {
                    PlayerPrefs.DeleteKey(playerName);
                    return true;
                }
                else
                {
                    int temp = (int)(Math.Ceiling(oneDayTimeSpan.Subtract(difference).TotalHours));
                    timeLeft = temp;
                    return false;
                
                }
            }
            else
            {
                return true;
            }
        }

        public void SendGift(string targetPlayer)
        {
            Debug.Log("Current gift target player is: " + targetPlayer);
            currentGift = new ParseObject("Gift");
            currentGift["giftId"] = -1;
            currentGift["Sender"] = playerName;
            currentGift["Receiver"] = targetPlayer;
            GiftSelectionPopUp.GetComponent<CPanel>().SetActive(true);
        }

        public void SelectGift(int giftId)
        {
            if (GiftSelectionPopUp.GetComponentInChildren<Text>().text.Equals(giftsDatabase[giftId].giftName.ToString()))
            {
                GiftSelectionPopUp.GetComponentInChildren<Text>().text = "Select Gift";
                currentGift["giftId"] = -1;

            }
            else
            {
                GiftSelectionPopUp.GetComponentInChildren<Text>().text = giftsDatabase[giftId].giftName.ToString();
                currentGift["giftId"] = giftId;
                currentGift["GiftType"] = giftsDatabase[giftId].giftName.ToString();
                currentGift["giftStoreId"] = giftsDatabase[giftId].giftStoreId;
                Debug.Log("Current Gift Type: " + giftsDatabase[giftId].giftType.ToString() + " Amount: " + giftsDatabase[giftId].amount);
            }
        }

        public void CallGifts()
        {
            if (int.Parse(currentGift["giftId"].ToString()) != -1)
            {
                giftConfirmationPopUp.GetComponent<GiftConfirmationHandler>().SetValues(currentGift["Receiver"].ToString(), currentGift["GiftType"].ToString(), (int)currentGift["giftId"]);
                giftConfirmationPopUp.GetComponent<CPanel>().SetActive(true);
            }
        }

        public void ConfirmGiftSelection()
        {
            if(int.Parse(currentGift["giftId"].ToString()) != -1)
            {
                currentGift.SaveAsync();
                PlayerPrefs.SetString(currentGift["Receiver"].ToString(), System.DateTime.Now.ToString());
                CloseGiftSelectionPopup();
                OpenGiftView();
                giftAcknowledgmentPopUp.GetComponent<CPanel>().SetActive(true);
            }
        }

        public void CloseGiftSelectionPopup()
        {
            giftConfirmationPopUp.GetComponent<CPanel>().SetActive(false);
            GiftSelectionPopUp.GetComponent<CPanel>().SetActive(false);
        }

        public void HandleGiftOperations()
        {
            Debug.Log("Starting gift checking");
            parseGiftProcessFinished = false;
            queriedGiftsList = new List<GiftListItem>();
            StartCoroutine(CheckForGifts());
            StartCoroutine(ShowGifts());
        }

        IEnumerator CheckForGifts()
        {
            if (FB.IsLoggedIn && !playerName.Equals(""))
            {
                Debug.Log(playerName);
                var query = ParseObject.GetQuery("Gift").WhereEqualTo("Receiver",playerName);
                query.FindAsync().ContinueWith(t =>
                    {
                        if(!t.IsFaulted)
                        {
                            foreach (ParseObject gift in t.Result)
                            {
                                Debug.Log("Processing gits");
                                GiftListItem item = new GiftListItem();
                                item.sender = gift["Sender"].ToString();
                                item.giftType = gift["GiftType"].ToString();
                                item.giftId = int.Parse(gift["giftId"].ToString());
                                item.giftStoreId = gift["giftStoreId"].ToString();
                                Debug.Log("Sender: " + item.sender + " Gift Type: " + item.giftType);
                                queriedGiftsList.Add(item);
                                gift.DeleteAsync();

                            }
                            parseGiftProcessFinished = true;
                        }
                    }
                    );
                yield return new WaitForSeconds(0.1f);
            }
        }

        IEnumerator ShowGifts()
        {
            while(!parseGiftProcessFinished)
            {
                yield return new WaitForSeconds(0.0f);
            }
            if (parseGiftProcessFinished)
            {
                Debug.Log("Preparing to deal with gifts");
                foreach (GiftListItem gift in queriedGiftsList)
                {
                    Debug.Log("Showing gifts etc.");
                    Debug.Log("Sender: " + gift.sender + " GiftType: " + gift.giftType);
                    GiftHandler giftHandler = Instantiate(GiftReceivedPopUp) as GiftHandler;
                    giftHandler.SetValues(gift.sender, gift.giftType, gift.giftId);
                    giftHandler.gameObject.SetActive(false);
                    giftHandler.transform.SetParent(giftHolder.transform,false);
                    giftHandler.GetComponent<CPanel>().SetActive(true);
                }
            }
        }

        public void AcceptGift(int giftId)
        {
            if(giftsDatabase[giftId].giftType == GiftType.Life)
            {
                SessionAssistant.main.GiveLife();     //Obsolete(revise SessionAssistant, StoreInventoryReference)
            }
            else
            {
                Debug.Log("Granting gift");
                StoreInventory.GiveItem(giftsDatabase[giftId].giftStoreId,giftsDatabase[giftId].amount);
            }
                
        }
    }
}