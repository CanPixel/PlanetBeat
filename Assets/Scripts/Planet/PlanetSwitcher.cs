using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlanetSwitcher : MonoBehaviour {
    public TexturePack texturePack;

    [HideInInspector] public BlackHoleTextures sun;

   private Dictionary<Color32, PlayerElement> colorPlayerTexture = new Dictionary<Color32, PlayerElement>();

   public static PlayerElement GetPlayerTexture(Color col) {
       if(instance == null) return null;
       return instance.colorPlayerTexture[col];
   }

    public static int GetPlayerTintIndex(int viewID) {
        if(instance == null) instance = GameObject.FindGameObjectWithTag("TEXTURESWITCHER").GetComponent<PlanetSwitcher>();
        int iF = (viewID / 1000);
        int fin = iF % instance.typeOfPlanets;
        return fin;
    }

    [System.Serializable]
    public class TexturePack {
        public string packName;
        public PlanetElement[] planets;
        public TextureElement asteroid;
        public TextureElement blackHole;
        public TextureElement Background;
        public PlayerElement[] Ship;
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
        //public Color tint = Color.white;
    }

    [System.Serializable]
    public class PlayerElement {
        public GameObject model;
        public Material ship, tail;
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
    public int typeOfPlanets = 6;

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
        if(instance != null) Destroy(gameObject);

        if(instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        UpdateTexturePack();
    }

    public static void ForceUpdateTextures() {
        if(instance == null) {
            var obj = GameObject.FindGameObjectWithTag("TEXTURESWITCHER");
            if(obj != null) instance = obj.GetComponent<PlanetSwitcher>();
        }
        if(instance != null) instance.UpdateTexturePack();
    }

    public void UpdateTexturePack() {
        if(planetsReference == null) planetsReference = GameObject.FindGameObjectWithTag("PLANETS").GetComponentsInChildren<PlanetGlow>();
        if(asteroidReference == null) asteroidReference = GameObject.FindGameObjectWithTag("ASTEROIDBELT");
        if(backgroundReference == null) backgroundReference = GameObject.FindGameObjectWithTag("BACKGROUND");

        if(sunGlowReference != null) sunGlowReference.sprite = texturePack.blackHole.glow;
        if(sunReference != null) sunReference.sprite = texturePack.blackHole.src;

        if(asteroidReference != null) {
            var asts = asteroidReference.GetComponentsInChildren<Asteroid>();
            foreach(var i in asts) i.SetTexture(texturePack);
        }

        if(colorPlayerTexture.Count <= 0) {
            for(int i = 0; i < texturePack.Ship.Length; i++) colorPlayerTexture.Add(texturePack.Ship[i].tint, texturePack.Ship[i]);
        }
    }
}