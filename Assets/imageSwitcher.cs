using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class imageSwitcher : MonoBehaviour
{
    [HideInInspector] public PlayerPlanets planet;

    Image myImageComponent;
    public Sprite redHand; 
    public Sprite pinkHand;
    public Sprite blueHand;
    public Sprite yellowHand;
    public Sprite greenHand;
    public Sprite cyanHand;

    void Start()
    {
        myImageComponent = GetComponent<Image>(); //Our image component is the one attached to this gameObject.
    }
    
    public void SetHandRed(){
        myImageComponent.sprite = redHand;
    }

    public void SetHandPink(){
        myImageComponent.sprite = pinkHand;
    }

    public void SetHandBlue(){
        myImageComponent.sprite = blueHand;
    }

    public void SetHandYellow(){
        myImageComponent.sprite = yellowHand;
    }

    public void SetHandGreen(){
        myImageComponent.sprite = greenHand;
    }

    public void SetHandCyan(){
        myImageComponent.sprite = cyanHand;
    }

}
