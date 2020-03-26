using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextureSwitcher : MonoBehaviour {
    public TexturePack[] texturePacks;

    public int pack = 0;

    [System.Serializable]
    public class TexturePack {
        public string packName;
        public TextureElement[] planets;
        public TextureElement asteroid;
        public TextureElement blackHole;
    }

    [System.Serializable]
    public class TextureElement {
        public Sprite src, glow;
        [Range(0, 5)]
        public float scale = 1;
    }
    [Space(20)]
    public Image sunReference, sunGlowReference;
    public Image asteroidReference;
    private Planet[] planetsReference;
    [Range(1, 10)]
    public int typeOfPlanets = 4;

    void Start() {
        planetsReference = GameObject.FindGameObjectWithTag("PLANETS").GetComponentsInChildren<Planet>();
        UpdateTexturePack(0);
    }

    public void UpdateTexturePack(int change) {
        var textPack = texturePacks[change];

        for(int i = 0; i < planetsReference.Length; i++) planetsReference[i].SetTexture(textPack.planets[i % typeOfPlanets]);
        if(textPack.blackHole.glow != null) {
            sunGlowReference.sprite = textPack.blackHole.glow;
            sunGlowReference.enabled = true;
        } else sunGlowReference.enabled = false;
        sunReference.sprite = textPack.blackHole.src;
        sunReference.SetNativeSize();
        sunGlowReference.SetNativeSize();

        //sunReference.transform.localScale = Vector3.one * textPack.blackHole.scale;
        asteroidReference.sprite = textPack.asteroid.src; ///////////////
    }
}
