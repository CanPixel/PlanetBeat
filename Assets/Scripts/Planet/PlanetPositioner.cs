using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[ExecuteInEditMode]
public class PlanetPositioner : MonoBehaviourPun {
    private PlayerPlanets[] planets;

    public float orbitDistance = 2, planetReformSpeed = 10f;
    public Vector2 ellipseStretch = new Vector2(0.1f, 0f);

    public bool turn = true;
    public float turnSpeed = 1f;

    private float move = 0;
    private float planetAmount = -1, oldPlanetAmount;
    private float reformDelay = 0;

    protected List<Vector2> planetPos = new List<Vector2>();
    protected List<float> planetAngle = new List<float>();

    void OnValidate() {
        if(orbitDistance < 1) orbitDistance = 1;
        #if (UNITY_EDITOR)
            PositionPlanets();
        #endif
    }

    void FixedUpdate() {
        if(reformDelay > 0) reformDelay -= Time.deltaTime;
        else if(planets != null) {
            for(int i = 0; i < planets.Length; i++) {
                if(planets[i] == null) continue;
                planets[i].trails.emitting = true;
            }
        }

        if(GameManager.GAME_STARTED && turn && Application.isPlaying && EliminationTimer.TIMER_START) {
            move += Time.deltaTime * (turnSpeed / 20f);
            PositionPlanets();
        }
    }

    protected void PositionPlanets() {
        planets = GetPlanets();
      /*   if(planetPos.Count <= 0) {
            for(int i = 0; i < planets.Length; i++) {
                planetPos.Add(planets[i].transform.position);
                planetAngle.Add(i);
            }
        }*/

        if(planetAmount < 0) planetAmount = oldPlanetAmount = planets.Length;
        if(oldPlanetAmount != planets.Length) {
            reformDelay = 0.05f;
            for(int i = 0; i < planets.Length; i++) if(planets[i] != null) planets[i].trails.emitting = false;
            oldPlanetAmount = planets.Length;
        }
        planetAmount = Mathf.Lerp(planetAmount, planets.Length, Time.deltaTime * planetReformSpeed);
        
        for(int i = 0; i < planets.Length; i++) planets[i].transform.position = GetCircle(orbitDistance, i + move, planetAmount);
           
           // float posX1, posY1;
           // posX1 = Mathf.Cos (Time.time * i) * orbitDistance;
           // posY1 = Mathf.Sin (Time.time * i) * orbitDistance / 2;

            //var planetPosition = new Vector3(posX1, posY1);
            //planetPos[i] = planetPosition;
        //}

//        for(int i = 0; i < planets.Length; i++) {
  //          planetAngle[i] = planetAngle[i] + Time.deltaTime * (turnSpeed);
    //        if(planetAngle[i] >= 360f) planetAngle[i] = 0;
      //  }

        //if(PhotonNetwork.IsMasterClient) photonView.RPC("SynchPositions", RpcTarget.All, move);
    }

     private Vector3 GetCircle(float radius, float angle, float amountOfPlanets) {
        var center = Vector3.zero;
        var ang = angle * (360f / amountOfPlanets);
        Vector3 pos;
        pos.x = center.x + radius * Mathf.Sin(ang * Mathf.Deg2Rad) * (1 + ellipseStretch.x);
        pos.y = center.y + radius * Mathf.Cos(ang * Mathf.Deg2Rad) * (1 + ellipseStretch.y);
        pos.z = center.z;
        return pos;
    }

    public PlayerPlanets[] GetPlanets() {
        return transform.GetComponentsInChildren<PlayerPlanets>();
    }
}