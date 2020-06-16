using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TutorialStep : MonoBehaviour {
    [HideInInspector] public PlayerShip host;

    public UnityEvent tutorialEvent, onCompletion;
    public TutorialPositioner pressSpacePos;

    public string highlighterCircleFocalPoint;
    public bool PressSpaceToContinue = true;

    public Image[] arrows;

    [System.Serializable]
    public class TutorialPositioner {
        public GameObject HUDElement, focalPoint;
        public string specialFocalPoint;
        public Vector3 rotation, offset;
    }
    public TutorialPositioner[] tutorialPositioners;

    private TutorialAchievement[] achievements;
    private bool flipped = false;

    public void InitStep(PlayerShip playerShip) {
        host = playerShip;
        achievements = GetComponentsInChildren<TutorialAchievement>();
    }

    public void ScreenCheck(Vector3 pos) {
        if(pos.x > 5.5f && !flipped) {
            tutorialPositioners[0].offset *= -1f;
            flipped = true;
            foreach(var i in arrows) {
                i.transform.localScale = new Vector3(-i.transform.localScale.x, i.transform.localScale.y, i.transform.localScale.z);
                i.transform.localPosition = new Vector3(-i.transform.localPosition.x, i.transform.localPosition.y, i.transform.localPosition.z);
            }
        }
    }

    public void UpdateStep() {
        for(int i = 0; i < tutorialPositioners.Length; i++) {
            if(tutorialPositioners[i].focalPoint != null) {
                var target = tutorialPositioners[i].focalPoint.transform.position + tutorialPositioners[i].offset;
                tutorialPositioners[i].HUDElement.transform.position = new Vector3(target.x, target.y, -3);
            }
            else if(tutorialPositioners[i].specialFocalPoint.Length > 0) {
                if(tutorialPositioners[i].specialFocalPoint.ToLower() == "planet" && host != null && host.planet != null && tutorialPositioners[i].HUDElement != null) {
                    var target = host.planet.transform.position + tutorialPositioners[i].offset;
                    tutorialPositioners[i].HUDElement.transform.position = new Vector3(target.x, target.y, -3);
                }
            }
            tutorialPositioners[i].HUDElement.transform.eulerAngles = tutorialPositioners[i].rotation;
        }
        transform.localPosition = new Vector3(transform.localPosition.x, Mathf.Sin(Time.time * 6f) * 100f, transform.localPosition.z);
    }

    public void CompleteStep() {
        if(onCompletion != null) onCompletion.Invoke();
    }

    public void ReleaseAchievements() {
        foreach(var i in achievements) {
            i.transform.SetParent(transform.parent, true);
            i.Finish(host);
        }
    }

    public void DeleteStep() {
        Destroy(gameObject);
    }
}
