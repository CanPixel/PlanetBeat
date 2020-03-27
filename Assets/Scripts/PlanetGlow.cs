using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlanetGlow : MonoBehaviour {
    public Image glow, src; //Main Texture + Glow texture
    private float glowOffset;

    void Start() {
        glowOffset = 1f + Random.Range(0f, 1f);
    }

    void Update() {
        glow.color = Color.Lerp(glow.color, glow.color + new Color(0, 0, 0, Mathf.Sin(Time.time * 3f + glowOffset)), Time.deltaTime * 1f);
    }

    public void SetTexture(TextureSwitcher.TextureElement element) {
        src.sprite = element.src;
        if(element.glow == null) {
            glow.sprite = null;
            glow.color = new Color(0, 0, 0, 0);
            glow.enabled = false;
            return;
        }
        glow.enabled = true;
        glow.color = new Color(1, 1, 1, 1);
        glow.sprite = element.glow;
    }
}
