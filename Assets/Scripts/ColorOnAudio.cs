using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorOnAudio : MonoBehaviour {
    
    
    public Color _color;
    Renderer _r;
    Material _m;
    Animator _a;
    private float intensity;
    [Range(0.8f, 0.99f)]
    public float _fallbackFactor;
    [Range(1, 4)]
    public float _colorMultiplier;
    
    void Start ()
    {
        _r = GetComponent<Renderer>();
        _m = _r.sharedMaterial;
        _a = gameObject.GetComponent<Animator>();
        intensity = 0;
        _m.EnableKeyword("_EMISSION");
        DontDestroyOnLoad(gameObject);
        
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.C))
        {
            Colorize();
            PulseBackground();
        }
        print(intensity);
        if (intensity > 0)
        {
            intensity *= _fallbackFactor;
            if (intensity < 0.1f)
            {
                intensity = 0;
            }
        }
        else
        {
            intensity = 0;
        }
        _m.SetColor("_EmissionColor", _color * intensity * _colorMultiplier);
        
    }

    public void Colorize()
    {
        intensity = 1;
    }
    public void PulseBackground()
    {
        _a.SetBool("kick", true);
    }
    public void PulseBackgroundOFF()
    {
        _a.SetBool("kick", false);
    }
    public void PulseYellow()
    {
        _a.SetBool("pulse", true);
    }
    public void PulseYellowOFF()
    {
        _a.SetBool("pulse", false);
    }
}
