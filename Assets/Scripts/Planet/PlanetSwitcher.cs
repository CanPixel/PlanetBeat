using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlanetSwitcher : MonoBehaviour {
    public TexturePack texturePack;

    [HideInInspector] public BlackHoleTextures sun;

    private Color[] playerColors;
    public static Color GetPlayerTint(int i) {
        return instance.playerColors[GetPlayerTintIndex(i)];
    }

    public static int GetPlayerTintIndex(int viewID) {
        if(instance == null) instance = GameObject.FindGameObjectWithTag("TEXTURESWITCHER").GetComponent<PlanetSwitcher>();
        int iF = (viewID / 1000);
        int fin = iF % instance.typeOfPlanets;
        return fin;
    }

    //public static PlanetElement GetRandomPlanet() {
     //   return instance.texturePack.planets[Random.Range(0, instance.typeOfPlanets)];
   // }

    [System.Serializable]
    public class TexturePack {
        public string packName;
        public PlanetElement[] planets;
        public TextureElement asteroid;
        public TextureElement blackHole;
        public TextureElement Background;
        public TextureElement[] Ship;
    }

    [System.Serializable]
    public class PlanetElement {
        public string name;
        public Mesh model;
        public Mesh ring;
        public Material ringMaterial;
        public Material[] lightStages;

        [Range(0, 5)]
        public float scale = 1;
        public Color tint = Color.white;
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

    private static PlanetSwitcher instance;
    public static TexturePack GetCurrentTexturePack() {
        return instance.texturePack;
    }

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
    }

    public static void ForceUpdateTextures() {
        if(instance == null) {
            var obj = GameObject.FindGameObjectWithTag("TEXTURESWITCHER");
            if(obj != null) instance = obj.GetComponent<PlanetSwitcher>();
        }
        if(instance != null) instance.UpdateTexturePack();
    }

    public void UpdateTexturePack() {
      //  pack = change;
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

        if(planetsReference != null) for(int i = 0; i < planetsReference.Length; i++) planetsReference[i].SetPlanet(texturePack.planets[i % typeOfPlanets]);
        if(sunGlowReference != null) sunGlowReference.sprite = texturePack.blackHole.glow;
        if(sunReference != null) sunReference.sprite = texturePack.blackHole.src;

     //   var bgs = backgroundReference.GetComponent<Background>();
      //  bgs.SetTexture(texturePack);

        if(sun != null) sun.UpdateSize();

        if(asteroidReference != null) {
            var asts = asteroidReference.GetComponentsInChildren<Asteroid>();
            foreach(var i in asts) i.SetTexture(texturePack);
        }

        if(playerColors == null) playerColors = new Color[typeOfPlanets];
        for(int i = 0; i < GetCurrentTexturePack().planets.Length; i++) playerColors[i] = GetCurrentTexturePack().planets[i].tint;
    }
}