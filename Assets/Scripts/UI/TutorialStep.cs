using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TutorialStep : MonoBehaviour {
    [HideInInspector] public PlayerShip host;

    public UnityEvent tutorialEvent;

    public string highlighterCircleFocalPoint;

    [System.Serializable]
    public class TutorialPositioner {
        public GameObject HUDElement, focalPoint;
        public string specialFocalPoint;
        public Vector3 rotation, offset;
    }
    public TutorialPositioner[] tutorialPositioners;

    public void InitStep(PlayerShip playerShip) {
        host = playerShip;
    }

    public void UpdateStep() {
        for(int i = 0; i < tutorialPositioners.Length; i++) {
            if(tutorialPositioners[i].focalPoint != null) tutorialPositioners[i].HUDElement.transform.position = tutorialPositioners[i].focalPoint.transform.position + tutorialPositioners[i].offset;
            else if(tutorialPositioners[i].specialFocalPoint.Length > 0) {
                if(tutorialPositioners[i].specialFocalPoint.ToLower() == "planet") tutorialPositioners[i].HUDElement.transform.position = host.planet.transform.position + tutorialPositioners[i].offset;
            }
            tutorialPositioners[i].HUDElement.transform.eulerAngles = tutorialPositioners[i].rotation;
        }
    }
}
