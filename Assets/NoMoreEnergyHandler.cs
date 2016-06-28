using UnityEngine;
using System.Collections;

public class NoMoreEnergyHandler : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}

    public void RefillEnergy()
    {
        SessionAssistant.main.RefillEnergy();
    }
}
