using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BlastRadius : MonoBehaviour {
    private bool hurt = false;
    public GameObject explodeParticles;

    public int segments = 50;
    public float maxRange = 10;
    public float timeUntillDestroy = 1;
    public Color radiusColor;

    private LineRenderer line;

    private float expandSpeed, lifeSpan;

    void Start() {
        line = GetComponent<LineRenderer>();
        line.positionCount = segments + 1;
        line.startColor = line.endColor = radiusColor;
        CreatePoints(1);
        transform.localScale = Vector3.zero;
    }
    
    public void Hurt(float expand) {
        hurt = true;
        expandSpeed = expand * 2f;
    }

    void OnValidate() {
        if(segments < 0) segments = 0;
    }

    void Update() {
        lifeSpan += Time.deltaTime;

        CreatePoints(1);
        line.startColor = line.endColor = Color.Lerp(line.startColor, new Color(radiusColor.r, radiusColor.g, radiusColor.b, 0), Time.deltaTime * 0.85f);

        transform.localScale += Vector3.one * Time.deltaTime * expandSpeed;
        if(expandSpeed > 0.1f) expandSpeed -= Time.deltaTime / 2f;
        if(transform.localScale.x > maxRange || lifeSpan > timeUntillDestroy) GameManager.DESTROY_SERVER_OBJECT(gameObject);
    }

    void OnTriggerEnter2D(Collider2D col) {
        if(hurt && col.gameObject.tag == "PLAYERSHIP" && col.gameObject.GetComponent<PlayerShip>().CanExplode()) {
            var i = Instantiate(explodeParticles, col.transform.position, Quaternion.identity);
            i.transform.localScale /= 2f;
            col.gameObject.GetComponent<PlayerShip>().Explode();
        }
    }

    void CreatePoints(float radius) {
        line.positionCount = segments + 1;
        float x;
        float y;
        float angle = 20f;
        for(int i = 0; i < segments + 1; i++) {
            x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
            y = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;

            line.SetPosition(i, new Vector3(x, y, 0));
            angle += (360f / segments);
        }
    }
}
