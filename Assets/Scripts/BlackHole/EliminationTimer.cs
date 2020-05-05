using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Events;
using UnityEngine.UI;

public class EliminationTimer : MonoBehaviourPun {
    public Text eliminationCounter;
    public EliminationPhase phase;
    public float timeUntillElimination = 30f;
    private float elimTime;
    public UnityEvent eliminationEvent;

    private int elimCount = 0;

    public bool resetCount = false;
    private bool done = false;

    void Start() {
        phase = GetComponent<EliminationPhase>();
        elimTime = timeUntillElimination;
    }

    void Update() {
        if(!PhotonNetwork.IsMasterClient || !GameManager.GAME_STARTED) return;

        photonView.RPC("SynchTimer", RpcTarget.All, (int)timeUntillElimination);

        if(!phase.IsEliminating()) {
            if(timeUntillElimination > 0) timeUntillElimination -= Time.deltaTime;
            else {
                if(!done) {
                    elimCount++;
                    eliminationEvent.Invoke();
                    done = true;
                    if(resetCount) {
                        timeUntillElimination = elimTime / elimCount;
                        done = false;
                    }
                }
            }
        }
    }

    [PunRPC]
    public void SynchTimer(int time) {
        eliminationCounter.text = "0:" + ((time < 10)? "0" : "") + time.ToString();
    }
}
