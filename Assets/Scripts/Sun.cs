using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class Sun : MonoBehaviour {
    [Range(1, 10)]
    public float interval = 1;

    [Header("REFERENCES")]
    public GameObject sun;
    public Image sunGlow;
    public Planet blackhole;
    private UIFloat sunFloatAnim;
    public Text roundCountdownText;

    public GameObject scoreManager;
    PlayerPlanets _playerPlanets; 
    
    private float timer = 0;

    public float roundDuration = 30f; 

    public bool roundHasEnded;

    private bool blackHole = false;
    private float transition = 0;

    private Vector2 baseScale;

    void Start() {
        baseScale = transform.localScale;
        sunFloatAnim = sun.GetComponent<UIFloat>();

        timer = roundDuration; 

        roundHasEnded = false;

        //_playerPlanets = scoreManager.GetComponent<PlayerPlanets>(); 
    }

    void Update() {
        //Glow fluctuation black hole
        if(sunGlow.enabled) sunGlow.color = new Color(1, 1, 1, Mathf.Sin(Time.time * 26f) * 0.8f + 0.2f);

        DoomsdayEvent();   
    }

    protected void SwitchStarState() {
        timer = roundDuration;
        blackHole = !blackHole;
        transition = 1;
    }

    void DoomsdayEvent()
    {
     
        
        if (!roundHasEnded)  
            timer -= Time.deltaTime;

        if(timer <= 0f && roundHasEnded == false)
        {
            SwitchStarState();
            //roundHasEnded = true;
            CheckScores();
        }

        roundCountdownText.text = "Doomsday In:" + timer.ToString("F0");

        if (timer <= 10f)
            roundCountdownText.color = Color.red;
        else
            roundCountdownText.color = Color.green;






        sun.SetActive(!blackHole);
        blackhole.gameObject.SetActive(blackHole);

        //transform.localScale = Vector2.Lerp(transform.localScale, baseScale + new Vector2(0.1f, 0.1f) * (1 - transition), Time.deltaTime * 7f);
        if (transition > 0) transition -= Time.deltaTime * 2f;

        sunFloatAnim.enabled = !blackHole;
        blackhole.enabled = blackHole;
    }


    void CheckScores()
    {
        _playerPlanets.blackHole = true;
        Debug.Log(_playerPlanets.lowestValue); 
    
     
    }
}
