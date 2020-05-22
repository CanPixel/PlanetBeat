using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceOnCanvas : MonoBehaviour {
    void Start() {
        transform.SetParent(GameObject.FindGameObjectWithTag("GAMEFIELD").transform, false);
    }
}
