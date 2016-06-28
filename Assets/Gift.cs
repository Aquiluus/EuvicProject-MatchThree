using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Gift : MonoBehaviour {

    public GameObject giftConfirmationPopUp;

    [System.Serializable]
    public class spritePack
    {
        public Sprite buttonPressed;
        public Sprite buttonUnpressed;
    }

    public spritePack[] buttonSprites;
    public GameObject[] buttons;

    private int currentButtonId = -1;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void ButtonToggle(int buttonId)
    {
        if (currentButtonId == buttonId)
        {
            buttons[buttonId].GetComponent<Image>().sprite = buttonSprites[buttonId].buttonUnpressed;
            currentButtonId = -1;
        }
        else if (buttons[buttonId].GetComponent<Image>().sprite.name.Equals(buttonSprites[buttonId].buttonPressed.name))
        {
            buttons[buttonId].GetComponent<Image>().sprite = buttonSprites[buttonId].buttonUnpressed;
            currentButtonId = -1;
        }
        else
        {
            if(currentButtonId !=-1)
            buttons[currentButtonId].GetComponent<Image>().sprite = buttonSprites[currentButtonId].buttonUnpressed;
            buttons[buttonId].GetComponent<Image>().sprite = buttonSprites[buttonId].buttonPressed;
            currentButtonId = buttonId;
        }
    }
}
