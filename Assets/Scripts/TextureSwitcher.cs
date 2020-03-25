using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextureSwitcher : MonoBehaviour {
    public TexturePack[] texturePacks;

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
    }
    [Space(20)]
    public Image sunReference;
    public Image asteroidReference;
    public Image[] planetsReference;
    [Range(1, 10)]
    public int typeOfPlanets = 4;

    void Start() {
        UpdateTexturePack(0);
    }

    public void UpdateTexturePack(int change) {
        var textPack = texturePacks[change];

        for(int i = 0; i < planetsReference.Length; i++) planetsReference[i].sprite = textPack.planets[i % typeOfPlanets].src;
        sunReference.sprite = textPack.blackHole.src;
        asteroidReference.sprite = textPack.asteroid.src; ///////////////
    }
}
