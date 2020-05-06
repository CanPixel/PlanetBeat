using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Events;
using UnityEngine.UI;

public class EliminationTimer : MonoBehaviourPun {
    public Text eliminationCounter;
    private EliminationPhase phase;
    public float timeUntillElimination = 30f;
    private float elimTime;
    public UnityEvent eliminationEvent;

    public static bool TIMER_START = false;

    private int elimCount = 0;

    public bool resetCount = false;
    private bool done = false;

    private PlayerPlanets[] planets;

    void Start() {
        phase = GetComponent<EliminationPhase>();
        elimTime = timeUntillElimination;
        planets = GameObject.FindGameObjectWithTag("PLANETS").GetComponentsInChildren<PlayerPlanets>();
    }

    void Update() {
        bool everyoneHasWealth = true;
        foreach(var i in planets) if(i.HasPlayer() && i.currentScore <= 0) {
            everyoneHasWealth = false;
            break;
        } 

        if(!PhotonNetwork.IsMasterClient || !GameManager.GAME_STARTED || !everyoneHasWealth) return;
        TIMER_START = true;

        photonView.RPC("SynchTimer", RpcTarget.All, (int)timeUntillElimination);

        if(!phase.IsEliminating()) {
            if(timeUntillElimination > 0) timeUntillElimination -= Time.deltaTime;
            else {
                if(!done) {
                    elimCount++;
                    eliminationEvent.Invoke();
                    done = true;
                    if(resetCount) {
                        timeUntillElimination = elimTime / (elimCount + 1);
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
