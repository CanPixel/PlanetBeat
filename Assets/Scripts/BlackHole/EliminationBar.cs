using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EliminationBar : MonoBehaviour {
    public Image filling;

    public void SetProgress(Color playerCol, float progress) {
        filling.color = playerCol;
        filling.fillAmount = progress;
    }
}
