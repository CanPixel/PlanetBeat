using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Events;
using UnityEngine.UI;

public class EliminationTimer : MonoBehaviourPun {
    public Text eliminationCounter;
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

        if(PhotonNetwork.IsMasterClient) photonView.RPC("SynchTimer", RpcTarget.All, (int)timeUntillElimination);

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

    [PunRPC]
    public void SynchTimer(int time) {
        eliminationCounter.text = "0:" + ((time < 10)? "0" : "") + time.ToString();
    }
}
