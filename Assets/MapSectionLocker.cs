using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MapSectionLocker : MonoBehaviour {

    public MapSectionLocker main;

    public GameObject sectionUnlockedPopUp;
    public Text counterText;
    public int starsRequired;
    public int sectionIndex;
    private int currentStars;
    private string key = "Section#";

    void Awake()
    {
        key = key + sectionIndex.ToString();
    }

	// Use this for initialization
	void Start () {
        main = this;
        key = key + sectionIndex;
        currentStars = PlayerPrefs.GetInt("StarCounter");
        if (currentStars < starsRequired)
            counterText.text = currentStars.ToString() + "/" + starsRequired.ToString();
        else
            this.gameObject.SetActive(false);
	}

    public void CheckForUnlocks()
    {
        Debug.Log("Checking for unlocks");
        currentStars = PlayerPrefs.GetInt("StarCounter");
        if (currentStars >= starsRequired)
        {
            PlayerPrefs.SetInt(key, 1);
            sectionUnlockedPopUp.GetComponent<CPanel>().SetActive(true);
            this.gameObject.SetActive(false);
        }
        else
        {
            counterText.text = currentStars.ToString() + "/" + starsRequired.ToString();
        }
    }
}
