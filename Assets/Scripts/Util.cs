using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Util {
    public static GameObject FindChildWithTag(Transform parent, string tag) {
        for(int i = 0; i < parent.childCount; i++) {
            var child = parent.GetChild(i);
            if(child.tag == tag) return child.gameObject;
        }
        return null;
    }

    public static Vector2 Abs(Vector2 root) {
        return new Vector2(Mathf.Abs(root.x), Mathf.Abs(root.y));
    }
}
