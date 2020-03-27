using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class Sun : MonoBehaviour {
    [Header("REFERENCES")]
    public GameObject sun;
    public Image sunGlow;
    private UIFloat sunFloatAnim;
    public Text roundCountdownText;

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
    }

    void Update() {
        //Glow fluctuation black hole
        if(sunGlow.enabled) sunGlow.color = new Color(1, 1, 1, Mathf.Sin(Time.time * 7f) * 0.8f + 0.2f);

        DoomsdayEvent();   
    }

    protected void SwitchStarState() {
        timer = roundDuration;
        blackHole = !blackHole;
        transition = 1;
    }

    void DoomsdayEvent() {
        if (!roundHasEnded) timer -= Time.deltaTime;

        if(timer <= 0f && roundHasEnded == false) SwitchStarState();
        roundCountdownText.text = "Doomsday In:" + timer.ToString("F0");

        if (timer <= 10f) roundCountdownText.color = Color.red;
        else roundCountdownText.color = Color.green;

        //sun.SetActive(!blackHole);
        //blackhole.gameObject.SetActive(blackHole);

        transform.localScale = Vector2.Lerp(transform.localScale, baseScale * (1 - transition), Time.deltaTime * 7f);
        if (transition > 0) transition -= Time.deltaTime * 2f;

        sunFloatAnim.enabled = !blackHole;
        //blackhole.enabled = blackHole;
    }
}
