using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Photon.Pun;

public class PlayerTutorial : MonoBehaviour {
    public GameObject resourcePrefab, infectroidPrefab;

    private LineRenderer line;

    [Range(0, 50)]
    public int segments = 50;
    private float radius = 0;

    [Header("Tutorial Vars")]
    public float resourceOrbitSpeed = 0.5f;

    public PlayerShip host;

    private bool showHighlight = true;

    [System.Serializable]
    public class TutorialPiece {
        public string tutorialName;

        public TutorialStep step;

        [Header("TIMING")]
        public float duration = 3;
        public float afterDuration = 0;
        public bool completeAfterDuration = false;
        [HideInInspector] public bool completed = false;

        [System.Serializable]
        public class SubTask {
            [HideInInspector] public TutorialPiece reference;

            public string taskName;
            public UnityEvent onSubComplete;

            [HideInInspector] public bool completed = false;
        }
        public SubTask[] subTasks;
    }
    
    [Space(10)]
    public TutorialPiece[] tutorialSteps;
    public int tutorialProgress = 0;
    private float tutorialTimer = 0, tutorialWait = 0;
    private float resourceTick = 0, infectroidTick = 0;

    private Asteroid tutorialResource;
    private Infectroid tutorialInfectroid;

    public Dictionary<string, TutorialPiece> tutorialPiecesByName = new Dictionary<string, TutorialPiece>();
    public Dictionary<string, TutorialPiece.SubTask> tutorialTasks = new Dictionary<string, TutorialPiece.SubTask>();

    void Start() {
        line = GetComponent<LineRenderer>();
        line.positionCount = segments + 1;
        foreach(var i in tutorialSteps) i.step.gameObject.SetActive(false);

        transform.SetParent(host.transform.parent, true);
        transform.localScale = Vector3.one * 0.02f;
        
        if(!Launcher.GetSkipCountDown() && host.photonView.IsMine) tutorialSteps[tutorialProgress].step.gameObject.SetActive(true);
        tutorialPiecesByName.Clear();
        foreach(var i in tutorialSteps) {
            i.step.InitStep(host);
            tutorialPiecesByName.Add(i.tutorialName, i);
            foreach(var sub in i.subTasks) {
                sub.reference = i;
                tutorialTasks.Add(sub.taskName.ToLower(), sub);
            }
        }
        if(Launcher.GetSkipCountDown()) {
            host.planet.FinishTutorial();
            line.enabled = false;
        }
    }

    public void CompleteSubTask(string name) {
        if(Launcher.GetSkipCountDown() || !tutorialTasks.ContainsKey(name.ToLower())) return;

        if(tutorialTasks.ContainsKey(name.ToLower())) {
            if(tutorialTasks[name.ToLower()].reference.tutorialName.ToLower() == tutorialSteps[tutorialProgress].tutorialName.ToLower()) {
                tutorialTasks[name.ToLower()].onSubComplete.Invoke();
                tutorialTasks[name.ToLower()].completed = true;

                bool fullComplete = true;
                for(int i = 0; i < tutorialSteps[tutorialProgress].subTasks.Length; i++) {
                    if(!tutorialSteps[tutorialProgress].subTasks[i].completed) {
                        fullComplete = false;
                        break;
                    }
                }
                if(fullComplete) tutorialSteps[tutorialProgress].completed = true;
            }
        }
    }

    void Update() {
        if(Launcher.GetSkipCountDown()) return;

        TutorialTick();

        //Highlighter circle animation
        radius = Mathf.Lerp(radius, 0.05f, Time.deltaTime * 2f);
        if(!GameManager.GAME_STARTED && host.photonView.IsMine) {
            var sine = Mathf.Sin(Time.time * 7f);
            radius = Mathf.Lerp(radius, 6500 + sine * 1000f, Time.deltaTime * 3f);
            CreatePoints(radius);
        } else {
            radius = Mathf.Lerp(radius, 0, Time.deltaTime * 3f);
            if(radius <= 45) line.enabled = false;
            else CreatePoints(radius);
        }
    }

    private void IncrementTutorial() {
        foreach(var i in tutorialSteps) i.step.gameObject.SetActive(false);
        tutorialProgress++;
        tutorialSteps[tutorialProgress].step.gameObject.SetActive(true);
        if(tutorialProgress < tutorialSteps.Length) tutorialSteps[tutorialProgress].step.tutorialEvent.Invoke();
        tutorialTimer = tutorialWait = 0;
    }

    private void TutorialTick() {
        if(tutorialProgress < tutorialSteps.Length) {
            var curTutorial = tutorialSteps[tutorialProgress];

            if(curTutorial.step != null) {
                curTutorial.step.UpdateStep();

                var focalString = curTutorial.step.highlighterCircleFocalPoint;
                Vector3 vocalPoint = Vector3.zero;
                if(curTutorial.step.highlighterCircleFocalPoint.Length > 0) {
                    switch(focalString.ToLower()) {
                        case "planet":
                            vocalPoint = host.planet.transform.position;
                            break;
                        case "player":
                            vocalPoint = host.transform.position;
                            break;
                        default: 
                            showHighlight = false;
                            break;
                    }
                    showHighlight = true;
                    transform.position = Vector3.Lerp(transform.position, vocalPoint, Time.deltaTime * 8f);
                }
                else line.enabled = showHighlight = false;
            }

            //float offset = 2.75f;
            //var sine = Mathf.Sin(Time.time * 7f);
           // curTutorial.step.transform.rotation = Quaternion.identity;
            //if(transform.position.x > 1) offset = -3f;
            //curTutorial.step.transform.position = new Vector3(transform.position.x + offset + sine / 14f, transform.position.y, 0);

            /* if(curTutorial.textFocalPoint != null) {
                var focalPoint = curTutorial.textFocalPoint.gameObject;
                if(curTutorial.tutorialName.Contains("PLANET!") && host.planet != null) focalPoint = host.planet.gameObject;

                float offset = 2.75f;
                var sine = Mathf.Sin(Time.time * 7f);
                if(focalPoint.transform.position.x > 1) offset = -3f;
                textPrefab.transform.position = new Vector3(focalPoint.transform.position.x + textPos.x + offset + sine / 14f, focalPoint.transform.position.y + textPos.y, 0);
            } */

            tutorialTimer += Time.deltaTime;
            if(tutorialTimer > curTutorial.duration && (curTutorial.completeAfterDuration || curTutorial.completed)) {
                if(curTutorial.afterDuration > 0) {
                    tutorialWait += Time.deltaTime;
                    if(tutorialWait > curTutorial.afterDuration) IncrementTutorial();
                } else IncrementTutorial();
            }
        }

        //Tuturial Resource
        if(tutorialResource != null && !tutorialResource.held) {
            resourceTick += Time.deltaTime;    
            if(host != null && host.planet != null) tutorialResource.transform.position = GetOrbit(host.planet.transform.position, Mathf.Clamp(resourceTick, 0, 1), resourceTick * resourceOrbitSpeed);
        }
        if(tutorialInfectroid != null && !tutorialInfectroid.throwed && !tutorialInfectroid.held) {
            infectroidTick += Time.deltaTime;    
            if(host != null && host.planet != null) tutorialInfectroid.transform.position = GetOrbit(host.planet.transform.position, Mathf.Clamp(infectroidTick, 0, 0.9f), infectroidTick * resourceOrbitSpeed);
        }
    }

     void CreatePoints(float radius) {
        float x;
        float y;
        float angle = 20f;
        for(int i = 0; i < (segments + 1); i++) {
            x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
            y = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;

            line.SetPosition(i, new Vector3(x, y, 0));
            angle += (360f / segments);
        }
    }

    public void SpawnTutorialResource() {
        if(GameManager.SkipCountdown() || !host.photonView.IsMine) return;

        var obj = Instantiate(resourcePrefab);
        tutorialResource = obj.GetComponent<Asteroid>();
        tutorialResource.transform.position = transform.position;
        tutorialResource.tag = "ResourceTutorial";
        tutorialResource.value = 10;
        //focalObject = tutorialResource.gameObject;
        if(host != null && host.planet != null) host.planet.StartTutorial();
    }
    public void SpawnTutorialInfectroid() {
        if(GameManager.SkipCountdown() || !host.photonView.IsMine) return;

        var obj = Instantiate(infectroidPrefab);
        tutorialInfectroid = obj.GetComponent<Infectroid>();
        tutorialInfectroid.transform.position = transform.position;
        tutorialInfectroid.tag = "InfectroidTutorial";
        //focalObject = tutorialInfectroid.gameObject;
        host.planet.currentScore = 10;

        //line.startColor = line.endColor = host.playerColor;
        //host.planet.Explode(10);
    }

    public void ResetOrbitColor() {
        foreach(var i in tutorialSteps) i.step.gameObject.SetActive(false);
        showHighlight = false;
        line.startColor = line.endColor = Color.white;
        if(tutorialInfectroid != null && tutorialInfectroid.gameObject != null) Destroy(tutorialInfectroid.gameObject);

        host.planet.photonView.RPC("SetResource", RpcTarget.AllBuffered, 0f);

        host.photonView.RPC("ReadyPlayer", RpcTarget.MasterClient, host.photonView.ViewID);
        
        var obj = PhotonNetwork.Instantiate("PLAYERREADY", transform.position, Quaternion.identity) as GameObject;
        obj.GetPhotonView().RPC("Set", RpcTarget.All, PlayerShip.PLAYERNAME, host.transform.localPosition, host.playerColor.r, host.playerColor.g, host.playerColor.b);

        host.planet.FinishTutorial();
    }

    private Vector3 GetOrbit(Vector3 center, float radius, float angle) {
        var ang = angle * 360f;
        Vector3 pos;
        pos.x = center.x + radius * Mathf.Sin(ang * Mathf.Deg2Rad);
        pos.y = center.y + radius * Mathf.Cos(ang * Mathf.Deg2Rad);
        pos.z = center.z;
        return pos;
    }
}