using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Background : MonoBehaviour {
    private Image src;

    public bool parallax = false;//, TURN = true;

    public float parallaxSpeed = 1f;

    [Header("PLANET WIGGLES")]
    public float wiggleSpeed;
    public float wiggleRange;

    void Start() {
        src = GetComponent<Image>();
        if(!parallax) SetTexture(PlanetSwitcher.GetCurrentTexturePack());
    
        var playerplanets = transform.GetComponentsInChildren<PlayerPlanets>();
//        foreach(var i in playerplanets) {
//            i.wiggleSpeed = wiggleSpeed;
//            i.wiggleRange = wiggleRange;
//        }
    }

    void Update() {
        //if(!TURN) return;
        
        //if(!parallax) transform.Rotate(Vector3.forward * 1 * Time.deltaTime);
        //else transform.Rotate(Vector3.forward * parallaxSpeed * Time.deltaTime);
    }


    public void SetTexture(PlanetSwitcher.TexturePack elm) {
        if(src == null) src = GetComponent<Image>();
        if(src == null) return;
        src.sprite = elm.Background.src;
    }
}