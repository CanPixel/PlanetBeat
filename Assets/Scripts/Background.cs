using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Background : MonoBehaviour {
    private Image src;

    public bool parallax = false;

    public float parallaxSpeed = 1f;

    void Start() {
        src = GetComponent<Image>();
        if(!parallax) SetTexture(TextureSwitcher.GetCurrentTexturePack());
    }

    void Update() {
        if(!parallax) transform.Rotate(Vector3.forward * 1 * Time.deltaTime);
        else transform.Rotate(Vector3.forward * parallaxSpeed * Time.deltaTime);
    }


    public void SetTexture(TextureSwitcher.TexturePack elm) {
        if(src == null) src = GetComponent<Image>();
        if(src == null) return;
        src.sprite = elm.Background.src;
    }
}