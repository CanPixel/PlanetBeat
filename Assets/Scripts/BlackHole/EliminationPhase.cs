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

    private float eliminationTimer = 0;

    void Start() {
        planetPositioner = GameObject.FindGameObjectWithTag("PLANETS").GetComponent<PlanetPositioner>();
        eliminationBar.gameObject.SetActive(false);
    }

    void Update() {
        if(eliminate && PhotonNetwork.IsMasterClient) {
            if(eliminationTimer > 0) eliminationTimer -= Time.deltaTime * eliminationSpeed;
            planets = planetPositioner.GetPlanets();

            PlayerPlanets lowest = null;
            foreach(var i in planets) if((lowest == null || i.currentScore < lowest.currentScore) && i.HasPlayer()) lowest = i;
            var progress = Util.Map(eliminationTimer, 0, eliminationDuration, 0f, 1f);
            var color = lowest.GetColor();
            photonView.RPC("SynchBar", RpcTarget.All, color.r, color.g, color.b, lowest.transform.position, progress);

            if(progress <= 0 && lowest != null) {
                EliminatePlayer(lowest);
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
        eliminationBar.transform.position = pos;
        eliminationBar.SetProgress(new Color(r, g, b), progress);   
        eliminationBar.gameObject.SetActive(true);
    }

    public void EliminatePlayer(PlayerPlanets lowest) {
        lowest.KillPlanet();
    }

    public void StartEliminate() {
        eliminate = true;
        eliminationTimer = eliminationDuration;
    }
}
