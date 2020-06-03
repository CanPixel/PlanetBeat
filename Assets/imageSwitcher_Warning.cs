using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class imageSwitcher_Warning : MonoBehaviour
{
    Image warningImageComponent;

    public Sprite redWarning;
    public Sprite pinkWarning;
    public Sprite blueWarning;
    public Sprite yellowWarning;
    public Sprite greenWarning;
    public Sprite cyanWarning;

    void Start()
    {
        warningImageComponent = GetComponent<Image>();
    }

    public void SetWarningRed(){
        warningImageComponent.sprite = redWarning;
    }

    public void SetWarningPink(){
        warningImageComponent.sprite = pinkWarning;
    }

    public void SetWarningBlue() {
        warningImageComponent.sprite = blueWarning;
    }

    public void SetWarningYellow(){
        warningImageComponent.sprite = yellowWarning;
    }

    public void SetWarningGreen(){
        warningImageComponent.sprite = greenWarning;
    }

    public void SetWarningCyan(){
        warningImageComponent.sprite = cyanWarning;
    }
}
