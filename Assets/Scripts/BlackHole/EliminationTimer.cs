using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Events;

public class EliminationTimer : MonoBehaviour {
    public float timeUntillElimination = 30f;
    private float elimTime;
    public UnityEvent eliminationEvent;

    public bool resetCount = false;
    private bool done = false;

    void Start() {
        elimTime = timeUntillElimination;
    }

    void Update() {
        if(!PhotonNetwork.IsMasterClient || !GameManager.GAME_STARTED) return;

        if(timeUntillElimination > 0) timeUntillElimination -= Time.deltaTime;
        else {
            if(!done) {
                eliminationEvent.Invoke();
                done = true;
                if(resetCount) {
                    timeUntillElimination = elimTime;
                    done = false;
                }
            }
        }
    }
}
