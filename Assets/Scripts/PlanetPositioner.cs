﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PlanetPositioner : MonoBehaviour {
    private PlayerPlanets[] planets;

    public float orbitDistance = 2;
    public Vector2 ellipseStretch = new Vector2(0.1f, 0f);

    void OnValidate() {
        if(orbitDistance < 1) orbitDistance = 1;
        #if (UNITY_EDITOR)
        PositionPlanets();
        #endif
    }

    void Start() {
        PositionPlanets();
    }

    protected void PositionPlanets() {
        planets = GetPlanets();
        for(int i = 0; i < planets.Length; i++) {
            var pos = GetCircle(orbitDistance, i, planets.Length);
            planets[i].transform.position = pos;
        }
    }

    private Vector2 GetCircle(float radius, float angle, int amountOfPlanets) {
        var center = Vector3.zero;
        var ang = angle * (360f / amountOfPlanets);
        Vector2 pos;
        pos.x = center.x + radius * Mathf.Sin(ang * Mathf.Deg2Rad) * (1 + ellipseStretch.x);
        pos.y = center.y + radius * Mathf.Cos(ang * Mathf.Deg2Rad) * (1 + ellipseStretch.y);
        return pos;
    }

    public PlayerPlanets[] GetPlanets() {
        return transform.GetComponentsInChildren<PlayerPlanets>();
    }
}