using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class RocketScript : MonoBehaviour
{
    IngameScript _iGS;
    float _speed, _homingIntensity, _deadzoneRadius;
    bool _shouldRocketSeek;
    PostProcessVolume _pPV;
    private Vignette _v;
    float _vignetteDefaultValue;
    bool _isrocketDead; //this bool is switched when the rocket leaves the screen
    // Start is called before the first frame update
    void Start()
    {
        _isrocketDead = false; //mark the rocket as still being alive

        _pPV = Camera.main.GetComponent<PostProcessVolume>(); //get the post processing volume reference
        _pPV.profile.TryGetSettings(out _v); //attempt to get the reference to the vignette

        _iGS = GameObject.FindGameObjectWithTag("ingameHandler").GetComponent<IngameScript>(); //get the reference of the ingamescript

        LockOn(); //lock onto the player

        _speed = 10f; //set the speed of the rocket
        _homingIntensity = 0.3f; //the intensity at which to home onto the player with
        _deadzoneRadius = 3f; //the radius around the player in which the rocket will stop seeking once inside
        _shouldRocketSeek = true; //the boolean that determines whether or not the rocket should be actively trying to seek the player
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.Translate(Vector2.right*_speed*Time.deltaTime); //move the rocket constantly in the facing direction

        if (Vector2.Distance(transform.position, _iGS.GetPlayerPosition()) < _deadzoneRadius) _shouldRocketSeek = false; //if rocket has made it inside the player deadzone, stop seeking 

        if (_shouldRocketSeek) Seek(_homingIntensity); //if the rocket is able to seek, make it seek

        //if the player is in play mode
        if (_iGS.GetState() == IngameScript.GameState.Playing) {

            //if the rocket is near the player
            if (Vector2.Distance(transform.position, _iGS.GetPlayerPosition()) < 1f) {
                Time.timeScale = 0.25f; //slow down time when near the player
                if (_v.intensity.value < (0.45f - (0.05f * Vector2.Distance(transform.position, _iGS.GetPlayerPosition())))) _v.intensity.value += 0.04f;
            }

            //if the rocket isn't near the player and the rocket isn't dead
            else if (!_isrocketDead) {
                Time.timeScale = 1f; //set the time to default
                if (_v.intensity.value > 0.3f) _v.intensity.value -= 0.01f; //if the vignette is greater than the default value, casually reduce it
            }
        }
    }

    //this method locks the rocket's target to the player's touch direction
    void LockOn() {
        Vector3 targ = _iGS.GetPlayerPosition();
        targ.z = 0f;

        Vector3 objectPos = transform.position;
        targ.x = targ.x - objectPos.x;
        targ.y = targ.y - objectPos.y;

        float angle = Mathf.Atan2(targ.y, targ.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }

    //this method seeks the target with the given intensity
    void Seek(float intensity) {
        Vector3 targ = _iGS.GetPlayerPosition();
        targ.z = 0f;

        Vector3 objectPos = transform.position;
        targ.x = targ.x - objectPos.x;
        targ.y = targ.y - objectPos.y;

        float angle = Mathf.Atan2(targ.y, targ.x) * Mathf.Rad2Deg;
        //transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(new Vector3(0, 0, angle)), intensity);
    }

    //set the stats of the rocket
    public void SetStats(float speed, float homingIntensity, float deadzoneRadius) {
        _speed += speed; //set the speed to the passed parameter
        _homingIntensity += homingIntensity; //set the homing intensity to the passed parameter
        _deadzoneRadius += deadzoneRadius; //set the deadzone radius to the passed parameter
    }

    //this method starts the self destruct coroutine
    public void StartSelfDestruct() {
        StartCoroutine("SelfDestruct"); //start the self destruct coroutine
    }

    //this coroutine
    IEnumerator SelfDestruct() {
        yield return new WaitForSeconds(1f); //wait for 3 seconds
        transform.GetChild(0).GetComponent<ParticleScript>().Detatch(); //detatch the particle script gameobject
        gameObject.SetActive(false); //destroy self
    }

    private void InstantSelfDestruct() {
        SoundManager.PlaySound("explosion");
        transform.GetChild(1).GetComponent<ParticleScript>().Detatch(); //detatch the particle script gameobject
        gameObject.SetActive(false); //destroy self
    }

    //when the rocket leaves screen
    private void OnBecameInvisible() {
        //if the player is playing
        if (_iGS.GetState() == IngameScript.GameState.Playing) {
            SoundManager.PlaySound("explosion");
            _isrocketDead = true; //mark the rocket as being dead
            transform.GetChild(1).GetComponent<ParticleScript>().Detatch(); //detatch the particle script gameobject
            _iGS.UpdateScore(); //update the score
        }
    }

    //when the missle collides
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player") && _iGS.GetState() != IngameScript.GameState.GameOver) {
            _iGS.EndGame(); //call the endgame function if the game hasn't ended already
            InstantSelfDestruct();
        }
    }

    //when the missle collides
    private void OnTriggerStay2D(Collider2D other) {
        if (other.CompareTag("Player") && _iGS.GetState() != IngameScript.GameState.GameOver) {
            _iGS.EndGame(); //call the endgame function if the game hasn't ended already
            InstantSelfDestruct();
        }
    }
}
