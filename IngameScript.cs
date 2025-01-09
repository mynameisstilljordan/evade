using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Lofelt.NiceVibrations;
using DG.Tweening;
using MoreMountains.Feedbacks;

public class IngameScript : MonoBehaviour
{
    [SerializeField] GameObject _player; //the player gameobject
    [SerializeField] GameObject _fingerPlacementGuide; //the finger placement guide gameobject
    [SerializeField] GameObject _rocket; //the rocket gameobject
    [SerializeField] GameObject _pausePanel; //the pause panel gameobject
    [SerializeField] GameObject[] _spawnIndicators; //the spawn indicator array
    [SerializeField] GameObject[] _edgeBarriers; //the edge barriers
    [SerializeField] Canvas _ingameCanvas, _pauseCanvas, _endgameCanvas; //the pause canvas
    [SerializeField] TMP_Text _ingameScoreText, _endgameScoreText, _highscoreText, _pauseMessage;
    [SerializeField] MMF_Player _effectsPlayer;
    bool _canThePlayerReturnToTheMenu;
    int _score;
    float _spawnDelay;
    Vector3 _playerPos;

    public enum GameState {
        Playing, Paused, GameOver
    }

    GameState gameState;

    // Start is called before the first frame update
    void Start()
    {
        GlobalGameHandlerScript.IncrementAdCounter(); //increment ad counter

        SetupBoundaries(); //setup the boundaries

        gameState = GameState.Playing; //set the game state to playing on default

        if (Input.touchCount == 1) _player.transform.position = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position); //set the player position to the touch position

        _spawnDelay = 1f; //set the spawn delay of the rocket spawning

        _score = 0; //initialize the score

        StartRocketSpawning(); //start the rocket spawning coroutine

        _ingameScoreText.text = _score.ToString(); //set score text to score 

        _canThePlayerReturnToTheMenu = false; //make it so the player cannot return to the main menu yet
    }

    // Update is called once per frame
    void Update()
    {
        //if there is one finger on the screen and the player is in playing mode
        if (Input.touchCount == 1 && GetState() == GameState.Playing) { //if there is only 1 touch and the player is still playing
            var touch = Input.touches[0]; //save the touch
            if (touch.phase == TouchPhase.Moved) { //everytime the player's finger moves
                _playerPos = Camera.main.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, 10)); //save the finger position
                _player.transform.position = _playerPos; //move the player gameobject to the finger position
            }

            //if the player has lifted their finger
            if (touch.phase == TouchPhase.Ended) {
                if (gameState == GameState.Playing) { //if the player is still playing
                    gameState = GameState.Paused; //set the gamestate to paused
                    _pauseMessage.text = "PLACE YOUR FINGER\nBACK ON THE SCREEN"; //update the pause message
                    PauseGame(touch.position); //call pause game while passing the location
                }
            }
        }

        //if the player has more than 1 finger on the screen
        else if (Input.touchCount > 1 && GetState() == GameState.Playing) {
            var touch = Input.touches[0]; //save the first touch
            if (gameState == GameState.Playing) { //if the player is still playing
                gameState = GameState.Paused; //set the gamestate to paused
                _pauseMessage.text = "YOU MAY ONLY\nUSE ONE FINGER"; //update the pause message
                PauseGame(touch.position); //call pause game while passing the location
            }
        }

        //if there is one finger on the screen and the player is in pause mode
        else if (Input.touchCount == 1 && gameState == GameState.Paused) {
            var _touch = Input.touches[0]; //save the touch to a varible
            if (_touch.phase == TouchPhase.Began) { //if the touch began
                Ray _touchposition = Camera.main.ScreenPointToRay(new Vector3(_touch.position.x, _touch.position.y, 0f)); //convert the touch position to an on screen location
                RaycastHit2D _hit = Physics2D.GetRayIntersection(_touchposition); //get the ray intersection of the touch and the object
                if (_hit && _hit.collider.CompareTag("fingerPlacementGuide")) { //if the fingerplacementguide was pressed
                    UnpauseGame(); //unpause the game
                }
            }
        }

        //if there's atleast 1 finger on the screen and endgame is active
        else if (Input.touchCount == 1 && gameState == GameState.GameOver && _canThePlayerReturnToTheMenu && Input.touches[0].phase == TouchPhase.Began) {
            SceneManager.LoadScene(sceneName: "menu"); //return back to the menu
        }

        if (GetState() == GameState.GameOver && Time.timeScale > 0.1f) Time.timeScale -= 0.25f * Time.deltaTime; //if the game has ended and time isn't frozen, start slowing time
        else if (GetState() == GameState.GameOver && Time.timeScale < 0.1f) Time.timeScale = 0.1f;
    }

    //the ienumerator that starts the rocket spawning cycle
    void StartRocketSpawning() {
        //if the player is playing
        if (gameState != GameState.GameOver) {
            EnableIndicator(Random.Range(0, 4)); //enable the indicator
        }
    }

    //enable the flash
    void EnableIndicator(int side) {
        _spawnIndicators[side].GetComponent<SpriteRenderer>().enabled = true; //enable the indicator
        FlashIndicator(_spawnIndicators[side], side); //start fading the flash
    }

    //disable the flash
    void DisableIndicator(int side) {
        _spawnIndicators[side].GetComponent<SpriteRenderer>().enabled = false; //enable the indicator
    }

    void FlashIndicator(GameObject flashIndicator, int spawnSide) {
        var _fI = flashIndicator.GetComponent<SpriteRenderer>();
        _fI.DOFade(0f, _spawnDelay) //flash the ball without consent
            .OnComplete(() => { //when the flash is completed
                DisableIndicator(spawnSide); //disable the indicator
                _fI.DOFade(1f, 0f); //set the alpha back to 1
                StartRocketSpawning(); //start spawning another rocket again
                SpawnRocket(spawnSide); //spawn the rocket on the given side
            });
    }

    //this method spawns a rocket in a random location
    void SpawnRocket(int spawnSide) {
        
        float xSpawn = 0f, ySpawn = 0f; //the actual location for the rocket spawn
        float baseSpeed = 10; //the base speed for the rocket

        //switch case determining exact spawn coordinates based on the spawn side
        switch (spawnSide) {
            case 0: ySpawn = 0f; xSpawn = Random.Range(0f, 1f); break; //up
            case 1: ySpawn = 1f; xSpawn = Random.Range(0f, 1f); break; //down
            case 2: ySpawn = Random.Range(0f, 1f); xSpawn = 0; break; //left
            case 3: ySpawn = Random.Range(0f, 1f); xSpawn = 1; break; //right
        }

        Vector2 spawnLocation = Camera.main.ViewportToWorldPoint(new Vector3(xSpawn, ySpawn)); //convert spawn location to world dimensions

        var rocketInstance = ObjectPool.Instance.GetRocket(); //spawn the rocket
        rocketInstance.SetActive(true); //activate the rocket
        rocketInstance.transform.position = spawnLocation; //set the rocket position to spawn location
        rocketInstance.GetComponent<RocketScript>().SetStats(baseSpeed + (_score * .25f), _score * .05f, _score * .05f); //set the stats of the rocket
        if (_spawnDelay >= 0.5f) _spawnDelay -= 0.005f; //decrease the spawn delay
        SoundManager.PlaySound("rocket");
    }

    //this method is called when the game needs to be paused
    void PauseGame(Vector2 fingerLocation) {
        _ingameCanvas.enabled = false; //hide the ingame canvas
        _pauseCanvas.enabled = true; //show the pause canvas
        _pausePanel.SetActive(true); //enable the pause panel
        Instantiate(_fingerPlacementGuide, Camera.main.ScreenToWorldPoint(new Vector3 (fingerLocation.x, fingerLocation.y, 11f)), Quaternion.identity); //spawn the finger placement guide at the lifted position
        Time.timeScale = 0f; //stop time
    }

    //this method is called when the game needs to be unpaused
    void UnpauseGame() {
        _pauseCanvas.enabled = false; //hide the pause canvas
        _ingameCanvas.enabled = true; //show the ingame canvas
        _pausePanel.SetActive(false); //enable the pause panel
        Destroy(GameObject.FindGameObjectWithTag("fingerPlacementGuide")); //destroy the fingerplacementguide
        gameState = GameState.Playing; //switch back to the playing gamestate
        Time.timeScale = 1f; //regulate time again
    }

    //this method is called when the score needs to be updated
    public void UpdateScore() {
        _score++; //increment the score
        HapticPatterns.PlayPreset(HapticPatterns.PresetType.LightImpact); //light haptic effect
        _ingameScoreText.text = _score.ToString(); //set score text to score 
        VibrateScreen(); //this method vibrates the screen when called
    }

    //this method is called when the game is ended
    public void EndGame() {
        gameState = GameState.GameOver; //set the gamestate to game over
        HapticPatterns.PlayPreset(HapticPatterns.PresetType.HeavyImpact); //light haptic effect
        _ingameCanvas.enabled = false; //hide the ingame canvas
        _pauseCanvas.enabled = false; //hide the pause canvas
        _endgameCanvas.enabled = true; //show the endgame canvas

        _player.GetComponent<SpriteRenderer>().enabled = false; //disable the sprite renderer of the player
        _endgameScoreText.text = "SCORE: "+_score.ToString(); //update the endgame score

        UpdateHighScore(); //update the highscore
     
        StartCoroutine("EndgameCooldown"); //start the coroutine for the endgame cooldown
    }

    //this method sets a 1 second cooldown before letting the player continue back to the main menu
    IEnumerator EndgameCooldown() {
        yield return new WaitForSeconds(.1f); //wait for 1 second
        _canThePlayerReturnToTheMenu = true; //allow the player to return to the main menu
    }

    //this method returns the gamestate
    public GameState GetState() {
        return gameState; //return the gamestate
    }

    //this method returns the player position
    public Vector3 GetPlayerPosition() {
        return _playerPos; //return the player position
    }

    //update the highscore
    private void UpdateHighScore() {
        //if the score is greater than the current highscore
        if (_score > PlayerPrefs.GetInt("highscore", 0)) {
            PlayerPrefs.SetInt("highscore", _score); //update the highscore
            _highscoreText.text = "BEST: "+_score.ToString(); //show the new highscore
        }

        //if there's no highscore
        else _highscoreText.text = "BEST: "+PlayerPrefs.GetInt("highscore",0).ToString(); //show the highscore

        PlayerPrefs.SetInt("evades", PlayerPrefs.GetInt("evades", 0) + _score);
        PlayerPrefs.SetInt("gamesPlayed", PlayerPrefs.GetInt("gamesPlayed", 0) + 1); //update the gamesplayed playerpref
    }

    void SetupBoundaries() {
        Vector3 _point;

        //place topright
        _point = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.nearClipPlane));
        _edgeBarriers[1].transform.position = _point; //move the barrier to point

        //place bottomleft
        _point = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, Camera.main.nearClipPlane));
        _edgeBarriers[0].transform.position = _point; //move the barrier to point
    }

    void VibrateScreen() {
        _effectsPlayer.PlayFeedbacks(); //play the feedbacks
    }
}
