using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class imageSwitcher : MonoBehaviour
{
    Image myImageComponent;
    public Sprite redHand; 
    public Sprite pinkHand; 

    void Start()
    {
        myImageComponent = GetComponent<Image>(); //Our image component is the one attached to this gameObject.
    }

    public void SetImage1() //method to set our first image
    {
        myImageComponent.sprite = redHand;
    }

    public void SetImage2()
    {
        myImageComponent.sprite = pinkHand;
    }
}
