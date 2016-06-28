using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent (typeof (Text))]
public class BestScoreCounter : MonoBehaviour {

	Text text;

	void Awake () {
		text = GetComponent<Text> ();	
	}
	
	void OnEnable () {
		if (LevelProfile.main != null) {
			int bestScore = PlayerPrefs.GetInt ("Best_Score_" + LevelProfile.main.level.ToString());
			text.text = bestScore.ToString ();
            string levelString = "Level "+ LevelProfile.main.level.ToString();
            GameObject.FindGameObjectWithTag("FBHolder").GetComponent<FBHandler.FBHolder>().UploadResultToParse(levelString, bestScore);
		} else
			text.text = "";
	}
}
