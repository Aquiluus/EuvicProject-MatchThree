using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

namespace FBHandler
{
    public class ListItemInvite : MonoBehaviour, IEquatable<ListItemInvite>
    {
        //Class that Holds items of our dynamic custom Invitable ListView
        public Toggle tglBtn; // toggle button to select item
        public string fId, picUrl; //holds id and pic URL returned from server.
        public Text txtName; //Used to Store and Display Name from the server
        public Image imgPic; // Image View to show image of the specified ID

        private GameObject FBHolderReference;

        void Start()
        {
            tglBtn.GetComponent<Toggle>().onValueChanged.AddListener(ToggleClicked);
            FBHolderReference = GameObject.FindGameObjectWithTag("FBHolder");
        }

        private void ToggleClicked(bool state)
        {
            FBHolderReference.GetComponent<FBHolder>().ChangeToggleState(FBHolder.ToggleState.Partial);
        }
        public void AssignValues(string fId, string picUrl, string txtName, Sprite imgPic)
        {
            this.fId = fId;
            this.picUrl = picUrl;
            this.txtName.text = txtName;
            this.imgPic.sprite = imgPic;
        }

        public bool Equals(ListItemInvite inviteObject)
        {
            return (this.txtName.text.Equals(inviteObject.txtName.text) && this.picUrl.Equals(inviteObject.picUrl));
        }

        public override int GetHashCode()
        {
            int hName = this.txtName.text != null ? this.txtName.text.GetHashCode() : 0;
            int hUrl = this.picUrl != null ? this.picUrl.GetHashCode() : 0;
            return hName ^ hUrl;
        }
    }
}