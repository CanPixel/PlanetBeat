using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SpaceRoomToggle : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    public Text text;
    public Image image, baseIMG;
    private int index = -1;
    private Toggle toggle;

    private Launcher launcher;

    private bool isPrevOn = false, hover = false;
    private byte maxPlayers;
    private int amountOfPlayers = 0;

    private Color titleColor;
    public Color uiColor;

    public string roomName;

    void Awake() {
        toggle = GetComponent<Toggle>();
        launcher = GameObject.FindGameObjectWithTag("LAUNCHER").GetComponent<Launcher>();
    }

    public void SetProperties(string text, Sprite img, ToggleGroup group, int index, byte maxPlayers, Color titleColor) {
        this.text.text = roomName = text;
        image.sprite = img;
        this.index = index;
        toggle.group = group;
        this.maxPlayers = maxPlayers;
        this.titleColor = titleColor;
    }

    public void SetState(bool toggle) {
        this.toggle.isOn = toggle;
    }

    void Update() {
        if(toggle.isOn) {
            baseIMG.color = Color.Lerp(baseIMG.color, uiColor, Time.deltaTime * 8f);
            launcher.SetRoomSelection(index);
        } else baseIMG.color = Color.Lerp(baseIMG.color, Color.white, Time.deltaTime * 7f);
        isPrevOn = toggle.isOn;

        text.color = Color.Lerp(text.color, (hover || toggle.isOn) ? Color.white : titleColor, Time.deltaTime * 20f);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        hover = true;
    }

    public void OnPointerExit(PointerEventData eventData) {
        hover = false;
    }
}
