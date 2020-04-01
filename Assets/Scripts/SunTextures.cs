using System.Collections;
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

		public void UpdateSize() {
			src.SetNativeSize();
			glow.SetNativeSize();
		}
}
