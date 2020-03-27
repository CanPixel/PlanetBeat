using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextureSwitcher : MonoBehaviour {
    public TexturePack[] texturePacks;

    [HideInInspector] public int pack = 0;

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
        public Color tint;
    }
    [Space(20)]
    public Image sunReference, sunGlowReference;
    private GameObject asteroidReference;
    private PlanetGlow[] planetsReference;
    [Range(1, 10)]
    public int typeOfPlanets = 4;

    private static TextureSwitcher instance;

    public static TexturePack GetCurrentTexturePack() {
        return instance.texturePacks[instance.pack];
    }

    private Dropdown dropdown;

    void Start() {
        instance = this;
    }

    void OnEnable() {
        dropdown = GetComponent<Dropdown>();
        dropdown.value = PlayerPrefs.GetInt("TexturePack");
        UpdateTexturePack(PlayerPrefs.GetInt("TexturePack"));
    }

    public void UpdateTexturePack(int change) {
        pack = change;
        if(planetsReference == null) planetsReference = GameObject.FindGameObjectWithTag("PLANETS").GetComponentsInChildren<PlanetGlow>();
        if(asteroidReference == null) asteroidReference = GameObject.FindGameObjectWithTag("ASTEROIDBELT");

        var textPack = texturePacks[change];
        for(int i = 0; i < planetsReference.Length; i++) planetsReference[i].SetTexture(textPack.planets[i % typeOfPlanets]);
        if(textPack.blackHole.glow != null) {
            sunGlowReference.sprite = textPack.blackHole.glow;
            sunGlowReference.enabled = true;
        } else sunGlowReference.enabled = false;
        sunReference.sprite = textPack.blackHole.src;
        sunReference.SetNativeSize();
        sunGlowReference.SetNativeSize();   

        var asts = asteroidReference.GetComponentsInChildren<Asteroid>();
        foreach(var i in asts) i.SetTexture(textPack);

        PlayerPrefs.SetInt("TexturePack", change);
    }
}
