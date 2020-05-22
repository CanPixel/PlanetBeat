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
        text.text = "<color='#"+ColorUtility.ToHtmlStringRGB(new Color(r, g, b))+"'>READY</color>";
        this.transform.localPosition = pos + Vector3.down * 100f;
    }

    void Update() {
        if(GameManager.GAME_STARTED) {
            GameManager.DESTROY_SERVER_OBJECT(gameObject);
            Destroy(gameObject);
        }
    }
}
