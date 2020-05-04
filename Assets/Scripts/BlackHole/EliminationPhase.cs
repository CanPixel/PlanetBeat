using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class EliminationPhase : MonoBehaviourPun {
    private bool eliminate = false;

    private PlanetPositioner planetPositioner;
    private PlayerPlanets[] planets;

    public EliminationBar eliminationBar; 

    [Header("ELIMINATION")]
    public float eliminationSpeed = 2;
    public float eliminationDuration = 10f;
    public float regenSpeed = 1f;

    private PlayerPlanets eliminationTarget;

    void Start() {
        planetPositioner = GameObject.FindGameObjectWithTag("PLANETS").GetComponent<PlanetPositioner>();
        eliminationBar.gameObject.SetActive(false);
    }

    void Update() {
        if(eliminate && PhotonNetwork.IsMasterClient) {
            if(eliminationTarget != null && eliminationTarget.eliminationTimer > 0) eliminationTarget.eliminationTimer -= Time.deltaTime * eliminationSpeed;

            PlayerPlanets lowest = null;
            foreach(var i in planets) if((lowest == null || i.currentScore < lowest.currentScore) && i.HasPlayer()) lowest = i;
            eliminationTarget = lowest;
            
            foreach(var i in planets) {
                if(i.playerNumber != eliminationTarget.playerNumber) i.eliminationTimer = Mathf.Lerp(i.eliminationTimer, eliminationDuration, Time.deltaTime * regenSpeed);
            }
            
            var progress = Util.Map(eliminationTarget.eliminationTimer, 0, eliminationDuration, 0f, 1f);
            var color = eliminationTarget.GetColor();
            photonView.RPC("SynchBar", RpcTarget.All, color.r, color.g, color.b, lowest.transform.position, progress);

            if(progress <= 0 && eliminationTarget != null) {
                EliminatePlayer(eliminationTarget);
                photonView.RPC("UnsynchBar", RpcTarget.All);
                eliminate = false;
            }
        }
    }

    [PunRPC]
    public void UnsynchBar() {
        eliminationBar.gameObject.SetActive(false);
    }

    [PunRPC]
    public void SynchBar(float r, float g, float b, Vector3 pos, float progress) {
        eliminationBar.transform.position = Vector3.Lerp(eliminationBar.transform.position, pos, Time.deltaTime * 2f);
        eliminationBar.SetProgress(new Color(r, g, b), progress);   
        eliminationBar.gameObject.SetActive(true);
    }

    public void EliminatePlayer(PlayerPlanets lowest) {
        lowest.KillPlanet();
    }

    public void StartEliminate() {
        planets = planetPositioner.GetPlanets();
        foreach(var i in planets) i.SetElimination(eliminationDuration);
        eliminate = true;
    }
}
