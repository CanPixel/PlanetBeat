﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShockwaveScript : MonoBehaviour {
    private Vector3 baseScale;
    private Material mat;

    public GameObject explodeParticles;

    private bool once = false;
    public float expandSpeed = 1f;

    private float expandTime = 0;
    private float intens, baseIntens;

    private bool hurt = false;

    void Start() {
        baseScale = transform.localScale;
        mat = GetComponent<SpriteRenderer>().material;
    }

	void Update () {
        transform.localScale += Vector3.one * Time.deltaTime * expandSpeed;
        if(!once && transform.localScale.x > baseScale.x + 3f) transform.localScale = baseScale;

        if(once) {
            intens = Mathf.Lerp(intens, 0f, Time.deltaTime * 0.5f);
            SetIntensity(intens);

            if(expandTime > 0) expandTime -= Time.deltaTime;
            else GameManager.DESTROY_SERVER_OBJECT(gameObject);
        }
    }

    public void SetIntensity(float intensity) {
        if(mat == null) mat = GetComponent<SpriteRenderer>().material;
        mat.SetFloat("_Intensity", intensity);
    }

    public void Detonate() {
        if(mat == null) mat = GetComponent<SpriteRenderer>().material;
        baseIntens = mat.GetFloat("_Intensity");
        intens = baseIntens;
        expandTime = 1;
        hurt = true;
        expandSpeed = 0.5f;
        once = true;
        transform.localScale = Vector3.zero;
    }

    void OnTriggerEnter2D(Collider2D col) {
        if(hurt && col.gameObject.tag == "PLAYERSHIP" && col.gameObject.GetComponent<PlayerShip>().CanExplode()) {
            var i = Instantiate(explodeParticles, col.transform.position, Quaternion.identity);
            i.transform.localScale /= 2f;
            col.gameObject.GetComponent<PlayerShip>().Explode();
        }
    }
}
    