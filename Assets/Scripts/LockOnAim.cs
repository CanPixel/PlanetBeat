using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class LockOnAim : MonoBehaviour {
    public PlayerShip ship;

    public EdgeCollider2D edge;
    public GameObject pivot;
    public LineRenderer line;

    public Color selectColor;

    [Range(0, 50)]
    public int segments = 50;

    public float expandSpeed = 1.5f;

    public float maxRadius = 500;
    public float minRadius = 20;
    
    private float radius = 0;

    void Start() {
        radius = minRadius;
        line.positionCount = segments + 1;
        CreatePoints(radius);
    }

    void Update() {
        if(ship != null && ship.hookMethod != PlayerShip.HookMethod.LockOn) {
            radius = 0;
            line.startColor = line.endColor = new Color(0, 0, 0, 0);
            return;
        }
        line.startColor = line.endColor = selectColor;

        if(!GameManager.GAME_STARTED || ship.hookMethod != PlayerShip.HookMethod.LockOn) return;

        if(Input.GetKey(KeyCode.Space) && ship.photonView.IsMine) if(radius < maxRadius * 10f) radius += Time.deltaTime * 1000f * expandSpeed;
        if(Input.GetKeyDown(KeyCode.Space)) AudioManager.PLAY_SOUND("Leap", 0.45f, 0.8f);
        if(Input.GetKeyUp(KeyCode.Space)) radius = minRadius;

        edge.edgeRadius = radius / 2350f;
        CreatePoints(radius);
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
