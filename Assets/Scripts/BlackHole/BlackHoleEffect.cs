using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BlackHoleEffect : MonoBehaviour {
    public Shader shader;
    public Transform blackHole;
    private float ratio = 0.5625f;
    public float radius;

    private Camera cam;
    private Material _material;
    
    Material material {
        get {
            if(_material == null) {
                _material = new Material(shader);
                _material.hideFlags = HideFlags.HideAndDontSave;
            }
            return _material;
        }
    }
    
    void OnEnable() {
        cam = GetComponent<Camera>();
        ratio = 1f / cam.aspect;
    }

    void OnDisable() {
        if(_material) DestroyImmediate(_material);
    }

    Vector3 wtsp;
    Vector2 pos;
    void OnRenderImage(RenderTexture src, RenderTexture dest) {
        ratio = 1f / cam.aspect;
        if(shader && material && blackHole) {
            wtsp = cam.WorldToScreenPoint(blackHole.position);
            
            //Is the black hole in front of camera?
            if(wtsp.z > 0) {
                pos = new Vector2(wtsp.x / cam.pixelWidth, wtsp.y / cam.pixelHeight);

                _material.SetVector("_Position", pos);
                _material.SetFloat("_Ratio", ratio);
                _material.SetFloat("_Rad", radius);
                _material.SetFloat("_Distance", Vector3.Distance(blackHole.position, transform.position));

                Graphics.Blit(src, dest, material);
            }
        }
    }
}
