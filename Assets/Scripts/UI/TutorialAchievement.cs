using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialAchievement : MonoBehaviour {
    public Outline[] outlines;

    public void SetColor() {
        foreach(var i in outlines) i.effectColor = new Color(0, 1, 0, 1);
    }
}
