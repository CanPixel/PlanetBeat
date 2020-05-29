using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlanetGlow : MonoBehaviour {
    private ProceduralAurora.AuroraMain auroraSRC;
    public GameObject aurora;
    [HideInInspector] public Animator auroraAnim;
    private float glowOffset;
    private PlayerPlanets playerPlanets;
    public MeshRenderer render;
    public Color planetColor;
    private float baseRange; 
    private List<GameObject> borealis = new List<GameObject>();

    [HideInInspector] public float partScale = 1f;

    private float flicker = 0, subFlicker = 0;

    void Start() {
        if(aurora != null) auroraSRC = aurora.GetComponent<ProceduralAurora.AuroraMain>();
        auroraAnim = GetComponent<Animator>();
        playerPlanets = GetComponent<PlayerPlanets>();
        glowOffset = 1f + Random.Range(0f, 1f);
        if(aurora != null) foreach(Transform t in aurora.transform) borealis.Add(t.gameObject);

        if(auroraSRC != null) {
            var keys = auroraSRC.auroraColorMain.colorKeys;
            for(int i = 0; i < auroraSRC.auroraColorMain.colorKeys.Length; i++) keys[i].color = playerPlanets.GetColor() * 2f;
            auroraSRC.auroraColorMain.SetKeys(keys, auroraSRC.auroraColorMain.alphaKeys);
        }
    }

    void Update() {
        if(flicker > 0) {
            flicker -= Time.deltaTime;
            subFlicker += Time.deltaTime;
            if(subFlicker > 0.2f) {
                render.enabled = !render.enabled;
                subFlicker = 0;
            }
        } else render.enabled = true;
    }

    void FixedUpdate() {
        if(aurora != null) {
            aurora.transform.position = transform.position;
            aurora.transform.rotation = Quaternion.Euler(90, 180, 0);

            foreach(var i in borealis) i.transform.localScale = Vector3.one * partScale;
        }
    }

    public void Flicker() {
        flicker = 1;
        subFlicker = 0;
    }

    public void Animate() {
        if(auroraAnim != null) auroraAnim.SetTrigger("Scorealis");
    }
}
