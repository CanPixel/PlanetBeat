using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialAchievement : MonoBehaviour {
    public Vector3 baseScale;
    public Outline[] outlines;

    private Vector3 basePos;
    private bool moveToPlayer = false;

    private GameObject player;

    void Start() {
        basePos = transform.localPosition;
    }

    public void SetColor() {
        foreach(var i in outlines) i.effectColor = new Color(0, 1, 0, 1);
        transform.localScale = baseScale * 1.25f;
    }

    void Update() {
        if(!moveToPlayer) transform.localScale = Vector3.Lerp(transform.localScale, baseScale, Time.deltaTime * 3f);
        else {
            transform.position = Vector3.Lerp(transform.position, player.transform.position, Time.deltaTime * 4f);
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, Time.deltaTime * 2f);

            if(transform.localScale.x < 6f) Destroy(gameObject);
        }
    }

    public void Finish(PlayerShip pl) {
        player = pl.gameObject;
        moveToPlayer = true;

        var dinges = GetComponentsInChildren<Text>();
        foreach(var i in dinges) Destroy(i.gameObject);
    }
}
