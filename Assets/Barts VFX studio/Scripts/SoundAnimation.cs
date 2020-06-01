using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundAnimation : MonoBehaviour
{
	public Material _bgMaterial;
    

    public Color _color;
	public string _colorProperty;

	public float _strength;
	[Range(0.8f, 0.99f)]
	public float _fallbackFactor;
	[Range(1, 4)]
	public float _colorMultiplier;

	void Start()
	{
        //_materialInstance = new Material(_bgMaterial);
        //_materialInstance.EnableKeyword("_EMISSION");

        _bgMaterial = GetComponent<Renderer>().sharedMaterial;
        _strength = 0;
	}
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            print("Colorizing ");
            _bgMaterial.SetColor("_EmissionColor", _color * _strength);
        }
        print(_strength);

        if (_strength > 0.1)
        {
            _strength *= _fallbackFactor;
        }
        else
        {
            _strength = 0;
        }
        

    }
}
