using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeavePanel : MonoBehaviour {
    private Vector3 basePos;

    void Start() {
        basePos = transform.localPosition;
    }

    void Update() {
        if(Input.mousePosition.y > (Screen.height / 4) * 3) transform.localPosition = Vector3.Lerp(transform.localPosition, basePos, Time.deltaTime * 6f);
        else transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(0, 400, 0), Time.deltaTime * 3f);
    }
}
