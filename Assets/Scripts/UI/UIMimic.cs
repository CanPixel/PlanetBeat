using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMimic : MonoBehaviour {
	public MaskableGraphic host;
	public float alphaOffset = 0;
	public Vector3 posOffset;

	public bool mimicPosition, mimicRotation, mimicScale, mimicUIColor, mimicAlpha;

	private MaskableGraphic current;

	void Start () {
		current = GetComponent<Image>();
		if(current == null) current = GetComponent<RawImage>();
		if(current == null) current = GetComponent<Text>();
	}
	
	void Update () {
		if(mimicPosition) transform.position = host.transform.position + posOffset;
		if(mimicRotation) transform.rotation = host.transform.rotation;
		if(mimicScale) transform.localScale = host.transform.localScale;
		if(mimicUIColor) current.color = host.color;
		if(mimicAlpha) current.color = new Color(current.color.r, current.color.g, current.color.b, host.color.a + alphaOffset);
	}
}
