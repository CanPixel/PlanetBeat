using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sun : MonoBehaviour {
    [Range(1, 10)]
    public float interval = 1;

    [Header("REFERENCES")]
    public GameObject sun;
    public Planet blackhole;
    private UIFloat sunFloatAnim;
    
    private float timer = 0;

    private bool blackHole = false;
    private float transition = 0;

    private Vector2 baseScale;

    void Start() {
        baseScale = transform.localScale;
        sunFloatAnim = sun.GetComponent<UIFloat>();
    }

    void Update() {
        timer += Time.deltaTime;

        if(timer > interval) SwitchStarState();
    
        sun.SetActive(!blackHole);
        blackhole.gameObject.SetActive(blackHole);

        transform.localScale = Vector2.Lerp(transform.localScale, baseScale * (1 - transition), Time.deltaTime * 7f);
        if(transition > 0) transition -= Time.deltaTime * 2f;

        sunFloatAnim.enabled = !blackHole;
        blackhole.enabled = blackHole;
    }

    protected void SwitchStarState() {
        timer = 0;
        blackHole = !blackHole;
        transition = 1;
    }
}
