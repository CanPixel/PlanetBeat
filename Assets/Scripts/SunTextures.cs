﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SunTextures : MonoBehaviour {
    	public Image src, glow;

		void Start() {
			var pack = TextureSwitcher.GetCurrentTexturePack().blackHole;
			src.sprite = pack.src;
			glow.sprite = pack.glow;
			UpdateSize();
		}

		void Update() {
			//Glow fluctuation black hole
        	if(glow.enabled) glow.color = new Color(1, 1, 1, Mathf.Sin(Time.time * 5f) * 0.9f + 0.2f);

			transform.Rotate(0, 0, 10);
		}

		public void UpdateSize() {
			src.SetNativeSize();
			glow.SetNativeSize();
		}
}
