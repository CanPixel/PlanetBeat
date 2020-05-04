using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Util {
    public static float Map(float x, float in_min, float in_max, float out_min, float out_max) {
        return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
    }

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

    public static bool Approximately(float a, float b, float threshold = 0.05f) {
        float max = Mathf.Max(a, b);
        float min = Mathf.Min(a, b);
        return Mathf.Abs(max - min) <= threshold;
    }

    public static void HiResScreenshot(string name, int res) {
        ScreenCapture.CaptureScreenshot(name, res);
    }
}
