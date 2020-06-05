using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class imageSwitcher : MonoBehaviour {
    [HideInInspector] public PlayerPlanets planet;

    Image myImageComponent;
    public Sprite redHand; 
    public Sprite pinkHand;
    public Sprite blueHand;
    public Sprite yellowHand;
    public Sprite greenHand;
    public Sprite cyanHand;

    void Start() {
        myImageComponent = GetComponent<Image>();
    }
    
    public void SetHandRed() {
        if(myImageComponent == null) return;
        myImageComponent.sprite = redHand;
    }

    public void SetHandPink() {
        if(myImageComponent == null) return;
        myImageComponent.sprite = pinkHand;
    }

    public void SetHandBlue() {
        if(myImageComponent == null) return;
        myImageComponent.sprite = blueHand;
    }

    public void SetHandYellow() {
        if(myImageComponent == null) return;
        myImageComponent.sprite = yellowHand;
    }

    public void SetHandGreen() {
        if(myImageComponent == null) return;
        myImageComponent.sprite = greenHand;
    }

    public void SetHandCyan() {
        if(myImageComponent == null) return;
        myImageComponent.sprite = cyanHand;
    }
}
