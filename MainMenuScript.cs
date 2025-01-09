using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Lofelt.NiceVibrations;
using TMPro;

public class MainMenuScript : MonoBehaviour {
    [SerializeField] GameObject _fingerPlacementGuide;
    [SerializeField] Canvas _statsCanvas;
    [SerializeField] TMP_Text _highscoreText, _gamesPlayedText, _evadesText;

    // Start is called before the first frame update
    void Start() {
        Time.timeScale = 1f; //reset the timescale

        if (GlobalGameHandlerScript._adCounter == 2) GlobalGameHandlerScript.ShowAd(); //if the ad counter is 2, show ad

        _evadesText.text = PlayerPrefs.GetInt("evades", 0).ToString();
        _gamesPlayedText.text = PlayerPrefs.GetInt("gamesPlayed", 0).ToString();
        _highscoreText.text = PlayerPrefs.GetInt("highscore", 0).ToString();
    }

    void FixedUpdate() {
        if (_fingerPlacementGuide.transform.rotation.z < 0) _fingerPlacementGuide.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 360)); //reset the rotation if it falls below 0
        _fingerPlacementGuide.transform.Rotate(0, 0, -1); //rotate the finger placement guide 
    }

    // Update is called once per frame
    private void Update() {
        if (Input.touchCount == 1) {
            var _touch = Input.touches[0]; //save the touch to a varible
            if (_touch.phase == TouchPhase.Began) { //if the touch began
                Ray _touchposition = Camera.main.ScreenPointToRay(new Vector3(_touch.position.x, _touch.position.y, 0f)); //convert the touch position to an on screen location
                RaycastHit2D _hit = Physics2D.GetRayIntersection(_touchposition); //get the ray intersection of the touch and the object

                if (_hit && _hit.collider.CompareTag("fingerPlacementGuide")) { //if the fingerplacementguide was pressed
                    SceneManager.LoadScene(sceneName: "ingame");
                    PlayButtonFeedback();
                }
            }
        }
    }

    public void OnStatsButtonPressed() {
        _statsCanvas.enabled = true;
        PlayButtonFeedback();
    }

    public void OnCloseStatsButtonPressed() {
        _statsCanvas.enabled = false;
        PlayButtonFeedback();
    }
    void PlayButtonFeedback() {
        SoundManager.PlaySound("button");
        HapticPatterns.PlayPreset(HapticPatterns.PresetType.LightImpact); //play the light impact
    }
}
