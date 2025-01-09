using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {
    public static AudioClip _explosion, _rocket, _button;
    static AudioSource audioSrc;

    // Start is called before the first frame update
    void Start() {
        _rocket = Resources.Load<AudioClip>("rocket");
        _explosion = Resources.Load<AudioClip>("explosion");
        _button = Resources.Load<AudioClip>("button");
        audioSrc = GetComponent<AudioSource>();
    }

    public static void PlaySound(string clip) {
        if (PlayerPrefs.GetInt("sound", 1) == 1) {
            switch (clip) {
                case "rocket":
                    audioSrc.PlayOneShot(_rocket);
                    break;
            }

            switch (clip) {
                case "explosion":
                    audioSrc.PlayOneShot(_explosion);
                    break;
            }

            switch (clip) {
                case "button":
                    audioSrc.PlayOneShot(_button);
                    break;
            }
        }
    }
}