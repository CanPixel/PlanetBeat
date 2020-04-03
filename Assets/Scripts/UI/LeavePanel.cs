using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeavePanel : MonoBehaviour {
    private Vector3 basePos;
    public Vector3 targetPos;

    void Start() {
        basePos = transform.localPosition;
    }

    void Update() {
        if(Input.mousePosition.y < (Screen.height / 5) * 4) transform.localPosition = Vector3.Lerp(transform.localPosition, basePos, Time.deltaTime * 6f);
        else transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * 3f);
    }
}
