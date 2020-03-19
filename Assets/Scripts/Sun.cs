using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class Sun : MonoBehaviour {
    [Range(1, 10)]
    public float interval = 1;

    [Header("REFERENCES")]
    public GameObject sun;
    public Planet blackhole;
    private UIFloat sunFloatAnim;
    public Text roundCountdownText; 
    
    private float timer = 0;

    public float roundDuration = 30f; 

    private bool roundHasEnded;

    private bool blackHole = false;
    private float transition = 0;

    private Vector2 baseScale;

    void Start() {
        baseScale = transform.localScale;
        sunFloatAnim = sun.GetComponent<UIFloat>();

        timer = roundDuration; 

        roundHasEnded = false; 
    }

    void Update() {

        DoomsdayEvent();

       /* if (timer > interval) SwitchStarState();
    
        sun.SetActive(!blackHole);
        blackhole.gameObject.SetActive(blackHole);

        transform.localScale = Vector2.Lerp(transform.localScale, baseScale * (1 - transition), Time.deltaTime * 7f);
        if(transition > 0) transition -= Time.deltaTime * 2f;

        sunFloatAnim.enabled = !blackHole;
        blackhole.enabled = blackHole;



    */

      
    }

    protected void SwitchStarState() {
        timer = 0;
        blackHole = !blackHole;
        transition = 1;
    }

    void DoomsdayEvent()
    {
        roundCountdownText.text = "Doomsday In:" + timer.ToString("F0"); 

        if(timer <= 10f)
            roundCountdownText.color = Color.red;         
        else      
            roundCountdownText.color = Color.green; 
        
        if (!roundHasEnded)  
            timer -= Time.deltaTime;

        if(timer <= 0f && roundHasEnded == false)
        {
            SwitchStarState();
            roundHasEnded = true;
            CheckScores();
        }

        sun.SetActive(!blackHole);
        blackhole.gameObject.SetActive(blackHole);

        transform.localScale = Vector2.Lerp(transform.localScale, baseScale * (1 - transition), Time.deltaTime * 7f);
        if (transition > 0) transition -= Time.deltaTime * 2f;

        sunFloatAnim.enabled = !blackHole;
        blackhole.enabled = blackHole;
    }


    void CheckScores()
    {
        //Check plyer Scores script for the highest and lowest score
    }
}
