using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerName : MonoBehaviour {
    private GameObject host;
    private Text text;

    void OnEnable() {
        text = GetComponent<Text>();
        //if(text != null && host != null) text.color = host.GetComponent<PlayerShip>().playerColor;
        transform.SetParent(GameObject.FindGameObjectWithTag("GAMEFIELD").transform, false);
    }

    void Update() {
        if(host != null) transform.localPosition = host.transform.localPosition + new Vector3(0, -70, 0);
    }

    public void SetHost(GameObject host, string name) {
        transform.SetParent(GameObject.FindGameObjectWithTag("GAMEFIELD").transform, false);
        this.host = host;
        text.text = name;
        text.color = host.GetComponent<PlayerShip>().playerColor;
    }

    public void Rename(string name) {
        text.text = name;
    }

    public void SetColor(Color col) {
        text.color = col;
    }

    public string GetName() {
        return text.text;
    }
}
