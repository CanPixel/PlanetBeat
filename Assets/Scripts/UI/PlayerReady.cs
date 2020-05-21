using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerReady : MonoBehaviourPun {
    private Text text;

    [PunRPC]
    public void Set(string name, Vector3 pos, float r, float g, float b) {
        if(text == null) text = GetComponent<Text>();
        text.text = "Player <color='#"+ColorUtility.ToHtmlStringRGB(new Color(r, g, b))+"'>" + name + "</color> ready";

        this.transform.localPosition = pos / 1.5f;
    }
}
