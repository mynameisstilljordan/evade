using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalGameHandlerScript : MonoBehaviour {
    public static GameObject _instance; //the instance var
    public static int _adCounter;
    string _npaValue;
    private void Awake() {
        DontDestroyOnLoad(gameObject); //dont destroy the gameobject on load
        if (_instance == null) _instance = gameObject; //if there isnt an existing instance of the gameobject
        else Destroy(gameObject); //destroy the gameobject
    }

    private void Start() {
        _adCounter = 0;

        Advertisements.Instance.Initialize(); //initialize ads

        //if (PlayerPrefs.GetInt("vibration", 1) == 0) HapticController.hapticsEnabled = false; //disable haptics
        //else HapticController.hapticsEnabled = true; //enable haptics
    }

    public static void IncrementAdCounter() {
        _adCounter++; //increment the ad counter
    }

    public static int GetAdCounter() {
        return _adCounter;
    }

    public static void ShowAd() {
        _adCounter = 0; //reset the adCounter
        Advertisements.Instance.ShowInterstitial(); //show the ad
    }
}
