using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent (typeof (Button))]
public class StarLevelButton : MonoBehaviour {

    public GameObject energyRefillPopUp;

	public void OnClick () {
		if (CPanel.uiAnimation > 0) return;
        if (SessionAssistant.main.GetAmountOfLives() > 0)
        {
            FieldAssistant.main.CreateField();
            SessionAssistant.main.StartSession(LevelProfile.main.target, LevelProfile.main.limitation);
            FindObjectOfType<FieldCamera>().ShowField();
            UIServer.main.ShowPage("Field");
        }
        else
        {
            UIServer.main.ShowPage("NoMoreEnergyPopup");
        }
	}
}
