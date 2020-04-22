using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextureSwitcher : MonoBehaviour {
    public TexturePack[] texturePacks;

    [HideInInspector] public int pack = 0;

    [HideInInspector] public BlackHoleTextures sun;

    private Color[] playerColors;
    public static Color GetPlayerTint(int i) {
        return instance.playerColors[GetPlayerTintIndex(i)];
    }

    public static int GetPlayerTintIndex(int viewID) {
        if(instance == null) instance = GameObject.FindGameObjectWithTag("TEXTURESWITCHER").GetComponent<TextureSwitcher>();
        int iF = (viewID / 1000);
        int fin = iF % instance.typeOfPlanets;
        return fin;
    }

    public static TextureElement GetRandomPlanet() {
        return instance.texturePacks[instance.pack].planets[Random.Range(0, instance.typeOfPlanets)];
    }

    [System.Serializable]
    public class TexturePack {
        public string packName;
        public TextureElement[] planets;
        public TextureElement asteroid;
        public TextureElement blackHole;
        public TextureElement Background;
        public TextureElement[] Ship;
    }

    [System.Serializable]
    public class TextureElement {
        public Sprite src, glow;
        [Range(0, 5)]
        public float scale = 1;
        public Color tint = Color.white;
    }
    [Space(20)]
    private Image sunReference, sunGlowReference;
    private GameObject asteroidReference;
    private GameObject backgroundReference;
    private PlanetGlow[] planetsReference;
    [Range(1, 10)]
    public int typeOfPlanets = 4;

    private static TextureSwitcher instance;
    public static TexturePack GetCurrentTexturePack() {
        return instance.texturePacks[instance.pack];
    }

    private Dropdown dropdown;

    public static void Detach() {
        instance.transform.SetParent(null);
        DontDestroyOnLoad(instance.gameObject);
        instance.planetsReference = null;
        instance.asteroidReference = null;
        instance.sun = null;
        instance.backgroundReference = null;
        instance.sunGlowReference = null;
        instance.sunReference = null;
    }

    void OnEnable() {
        if(instance == null) instance = this;
        playerColors = new Color[typeOfPlanets];
        if(dropdown == null) dropdown = GetComponent<Dropdown>();
        instance.pack = PlayerPrefs.GetInt("TexturePack");
        if(dropdown != null) dropdown.value = PlayerPrefs.GetInt("TexturePack");
    }

    public static void ForceUpdateTextures() {
        if(instance == null) {
            var obj = GameObject.FindGameObjectWithTag("TEXTURESWITCHER");
            if(obj != null) instance = obj.GetComponent<TextureSwitcher>();
        }
        if(instance != null) instance.UpdateTexturePack(PlayerPrefs.GetInt("TexturePack"));
    }

    public void UpdateTexturePack(int change) {
        pack = change;
        if(planetsReference == null) planetsReference = GameObject.FindGameObjectWithTag("PLANETS").GetComponentsInChildren<PlanetGlow>();
        if(asteroidReference == null) asteroidReference = GameObject.FindGameObjectWithTag("ASTEROIDBELT");
        if(sunReference == null) {
            var sunObj = GameObject.FindGameObjectWithTag("SUN");
            if(sunObj != null) {
                sun = sunObj.GetComponent<BlackHoleTextures>();
                if(sun != null) {
                    sunReference = sun.src;
                    sunGlowReference = sun.glow;
                }
            }
        }
        if(backgroundReference == null) backgroundReference = GameObject.FindGameObjectWithTag("BACKGROUND");

        var textPack = texturePacks[change];
        if(planetsReference != null) for(int i = 0; i < planetsReference.Length; i++) planetsReference[i].SetTexture(textPack.planets[i % typeOfPlanets]);
        if(sunGlowReference != null) sunGlowReference.sprite = textPack.blackHole.glow;
        if(sunReference != null) sunReference.sprite = textPack.blackHole.src;

        var bgs = backgroundReference.GetComponent<Background>();
        bgs.SetTexture(textPack);

        if(sun != null) sun.UpdateSize();

        if(asteroidReference != null) {
            var asts = asteroidReference.GetComponentsInChildren<Asteroid>();
            foreach(var i in asts) i.SetTexture(textPack);
        }

        if(playerColors == null) playerColors = new Color[typeOfPlanets];
        for(int i = 0; i < GetCurrentTexturePack().planets.Length; i++) playerColors[i] = GetCurrentTexturePack().planets[i].tint;

        PlayerPrefs.SetInt("TexturePack", change);
    }
}