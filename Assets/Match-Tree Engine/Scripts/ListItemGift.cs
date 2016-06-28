using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace FBHandler
{
    public class ListItemGift : MonoBehaviour
    {
        //Class that Holds items of our dynamic custom Invitable ListView
        public Button sendGiftButton; // toggle button to select item
        public string fId, picUrl; //holds id and pic URL returned from server.
        public Text txtName; //Used to Store and Display Name from the server
        public Image imgPic; // Image View to show image of the specified ID
        public Text buttonText;
        public Image giftImage;

        public void SetButton(bool egligible)
        {
            if (egligible)
            {
                UnityEngine.Events.UnityAction action = () => { GameObject.FindGameObjectWithTag("FBHolder").GetComponent<FBHolder>().SendGift(txtName.text); };
                sendGiftButton.GetComponent<Button>().onClick.AddListener(action);
            }
            else
            {
                sendGiftButton.GetComponent<Button>().onClick.RemoveAllListeners();
                sendGiftButton.GetComponent<Button>().enabled = false;
            }
        }
        

        public void AssignValues(string fId, string picUrl, string txtName, Sprite imgPic)
        {
            this.fId = fId;
            this.picUrl = picUrl;
            this.txtName.text = txtName;
            this.imgPic.sprite = imgPic;
        }
    }
}