using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EliminationBar : MonoBehaviour {
    public Image filling;

    private Image self;

    public void SetProgress(Color playerCol, float progress) {
        filling.color = playerCol;
        filling.fillAmount = progress;
    }

    public void SetAlpha(float a) {
        if(self == null) self = GetComponent<Image>();
        self.color = new Color(self.color.r, self.color.g, self.color.b, a);
        filling.color = new Color(filling.color.r, filling.color.g, filling.color.b, a);
    }

    public void LerpAlpha(float dest, float speed) {
        float a = self.color.a;
        self.color = Color.Lerp(self.color, new Color(self.color.r, self.color.g, self.color.b, dest), Time.deltaTime * speed);
        filling.color = Color.Lerp(filling.color, new Color(filling.color.r, filling.color.g, filling.color.b, dest), Time.deltaTime * speed);
    }
}
