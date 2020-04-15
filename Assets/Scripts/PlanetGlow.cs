﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlanetGlow : MonoBehaviour {
    public Image glow, src;
    private float glowOffset;
    private PlayerPlanets playerPlanets;

    public bool randomGen = false;

    private float flicker = 0, subFlicker = 0;

    void Start() {
        playerPlanets = GetComponent<PlayerPlanets>();
        glowOffset = 1f + Random.Range(0f, 1f);
    }

    void Update() {
        glow.color = Color.Lerp(glow.color, glow.color + new Color(0, 0, 0, Mathf.Sin(Time.time * 1.5f + glowOffset * 10) - 0.1f), Time.deltaTime * 1f);
    
        if(flicker > 0) {
            flicker -= Time.deltaTime;
            subFlicker += Time.deltaTime;
            if(subFlicker > 0.2f) {
                src.enabled = !src.enabled;
                subFlicker = 0;
            }
        } else src.enabled = true;
    }

    void OnEnable() {
        if(randomGen || PhotonNetwork.IsMasterClient) SetTexture(TextureSwitcher.GetRandomPlanet());
    }

    public void Flicker() {
        flicker = 1;
        subFlicker = 0;
    }

    public void SetTexture(TextureSwitcher.TextureElement element) {
        if(src != null && element != null) src.sprite = element.src;
        if(element.glow == null) {
            glow.sprite = null;
            glow.color = new Color(0, 0, 0, 0);
            glow.enabled = false;
            return;
        }
        if(glow == null) return;
        glow.enabled = true;
        glow.color = new Color(1, 1, 1, 1);
        glow.sprite = element.glow;
    }
}
