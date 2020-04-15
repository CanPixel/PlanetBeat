using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHighlighter : MonoBehaviour {
    private LineRenderer line;
    private Text text;

    [Range(0, 50)]
    public int segments = 50;
    private float radius = 0;

    public PlayerShip host;

    void Start() {
        text = GetComponentInChildren<Text>();
        line = GetComponent<LineRenderer>();
        line.positionCount = segments + 1;
        text.color = new Color(1, 1, 1, 1);

        string hookString = "";
        if(host.hookMethod == PlayerShip.HookMethod.FreeAim) hookString = "Space to shoot grapple";
        else hookString = "Hold space to extend grappling range";
        text.text = "THIS IS YOU! \n \n" + "<size=110>< and > to steer\n ^ to thrust \n" + "F to throw \n"+hookString+"</size>";
    }

    void Update() {
        var sine = Mathf.Sin(Time.time * 7f);

        text.transform.rotation = Quaternion.identity;
        text.transform.position = new Vector3(transform.position.x + 2.25f + sine / 7f, transform.position.y + 0.25f, 0);
        text.color = line.startColor = line.endColor = new Color(1, 1, 1, radius / 100f);

        if(!GameManager.GAME_STARTED && host.photonView.IsMine) {
            line.enabled = text.enabled = true;
            radius = Mathf.Lerp(radius, 6500 + sine * 1000f, Time.deltaTime * 3f);
            CreatePoints(radius);
        } else {
            radius = Mathf.Lerp(radius, 0, Time.deltaTime * 3f);
            if(radius <= 45) line.enabled = text.enabled = false;
            else CreatePoints(radius);
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
}
