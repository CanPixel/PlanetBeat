using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orbit : MonoBehaviour {
    [SerializeField]
    private GameObject gravityRing, stableRing;

    public void SetOrbitDistance(float ringOffs) {
        gravityRing.transform.localPosition = new Vector3(ringOffs, 0, 0);
    }

    public void SetStableOrbit(float i) {
        stableRing.transform.localPosition = new Vector3(i, 0, 0);
    }
}
