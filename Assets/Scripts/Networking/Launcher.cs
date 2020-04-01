using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class Launcher : MonoBehaviourPunCallbacks {
    [Header("REFERENCES")]
    [SerializeField] private GameObject controlPanel;
    public Sprite freeAim, lockOn, looker, player;

    public Button playButton;

    public Slider SpectSlider, AimSlider;
    public Text particiText, lockonText, playText, playersOnline, playersInSpace;
    public Image reticleIcon, spectateHandle, aimSliderBackground;

    private int amountPlayers;

    string gameVersion = "1";

    bool isConnecting;

    [Space(15)]
    public string LEVELNAME = "Space";
    public static string levelName;

    [Range(1, 20)]
    [SerializeField] private byte maxPlayers = 5;

    void Awake() {
        levelName = LEVELNAME;
        playButton.interactable = false;
        playText.text = "Connecting...";
        PhotonNetwork.AutomaticallySyncScene = true;
        if(controlPanel != null) controlPanel.SetActive(true);

        int val = (PlayerPrefs.GetInt("Spectate") == 0) ? 1 : 0;
        SpectSlider.value = val;
        OnChangeSpectate(SpectSlider.value);
        AimSlider.value = PlayerPrefs.GetInt("AIM_MODE");
        OnChangeAim(PlayerPrefs.GetInt("AIM_MODE"));

        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.GameVersion = gameVersion;
    }

    void Update() {
        playersOnline.text = PhotonNetwork.CountOfPlayers + " player" + ((PhotonNetwork.CountOfPlayers == 1) ? "" : "s") + " online";
        playersInSpace.text = PhotonNetwork.CountOfPlayersInRooms + " player" + ((PhotonNetwork.CountOfPlayersInRooms == 1) ? "" : "s") + " in space";
        amountPlayers = PhotonNetwork.CountOfPlayers;

        var activePlay = PhotonNetwork.CountOfPlayersInRooms <= 0;
        AimSlider.interactable = activePlay;
        aimSliderBackground.color = new Color(aimSliderBackground.color.r, aimSliderBackground.color.g, aimSliderBackground.color.g, (activePlay) ? 1f : 0.3f);

        if(Input.GetKeyUp(KeyCode.Escape)) Screen.fullScreen = !Screen.fullScreen;
    }

    public void OnChangeSpectate(System.Single value) {
        if(value == 0) {
            particiText.enabled = false;
            spectateHandle.sprite = player;
        }
        else {
            particiText.enabled = true;
            spectateHandle.sprite = looker;
        }
        int spect = (value == 0) ? 1 : 0;
        PlayerPrefs.SetInt("Spectate", spect);
    }

    public void OnChangeAim(System.Single value) {
        if(value == 0) {
            lockonText.enabled = false;
            reticleIcon.sprite = freeAim;
        }
        else {
            lockonText.enabled = true;
            reticleIcon.sprite = lockOn;
        }
        PlayerPrefs.SetInt("AIM_MODE", (int)value);
    }

    public void ClickSound(float pitch) {
        AudioManager.PLAY_SOUND("click", 1, pitch);
    }

    public void Quit() {
        Application.Quit();
    }

    public void Connect() {
        TextureSwitcher.Detach();
        PhotonNetwork.Disconnect();

        isConnecting = PhotonNetwork.ConnectUsingSettings();

        if(controlPanel != null) controlPanel.SetActive(false);
        playersInSpace.gameObject.SetActive(false);
        playersOnline.gameObject.SetActive(false);
        
        if(!PhotonNetwork.IsConnected) PhotonNetwork.JoinRandomRoom();
        else {
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = gameVersion;
        }
    }

    #region MonoBehaviourPunCallbacks Callbacks

    public override void OnConnectedToMaster() {
        playText.text = "Play";
        playButton.interactable = true;

        if(isConnecting) {
            PhotonNetwork.JoinRandomRoom();
            isConnecting = false;
        }
    }

    public override void OnDisconnected(DisconnectCause cause) {
        if(controlPanel != null) controlPanel.SetActive(true);
    }

    public override void OnJoinRandomFailed(short returnCode, string message) {
        Debug.Log("JoinRandomFailed(): No random room available, so we create one.");
        PhotonNetwork.CreateRoom(null, new RoomOptions(){MaxPlayers = maxPlayers});
    }

    public override void OnJoinedRoom() {
        Debug.Log("OnJoinedRoom(): Now this client is in a room.");
        PhotonNetwork.LoadLevel(levelName);
    }

    #endregion
}
