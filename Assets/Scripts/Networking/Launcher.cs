using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class Launcher : MonoBehaviourPunCallbacks, IInRoomCallbacks {
    [Header("REFERENCES")]
    [SerializeField] private GameObject controlPanel;
    public Sprite freeAim, lockOn, looker, player;

    public Button playButton;

    public Toggle SpectToggle;
    public Image SpectIcon;
    public Slider AimSlider;
    public Text lockonText, playText, playersOnline, playersInSpace, countOfRooms, title;
    public Image reticleIcon, aimSliderBackground;

    private int amountPlayers;

    string gameVersion = "1";
    public string roomName = "PLANETSPACE";

    [Space(15)]
    public string LEVELNAME = "Space";

    [Range(1, 20)]
    [SerializeField] private byte maxPlayers = 5;

    private bool connectNow = false;

    private float beginZoom;

    public override void OnEnable() {
        PhotonNetwork.AddCallbackTarget(this);
        base.OnEnable();
    }

    public override void OnDisable() {
        PhotonNetwork.RemoveCallbackTarget(this);
        base.OnDisable();
    }

    void Awake() {
        beginZoom = Camera.main.orthographicSize;
        playButton.interactable = false;
        playText.text = "CONNECTING...";
        PhotonNetwork.AutomaticallySyncScene = true;
        SpectIcon.gameObject.SetActive(false);
        if(controlPanel != null) controlPanel.SetActive(true);

        SpectToggle.isOn = false;
        OnChangeSpectate(SpectToggle.isOn);
        AimSlider.value = PlayerPrefs.GetInt("AIM_MODE");
        OnChangeAim(PlayerPrefs.GetInt("AIM_MODE"));

        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.GameVersion = gameVersion;
    }

    void Update() {
        playersOnline.text = PhotonNetwork.CountOfPlayers + " player" + ((PhotonNetwork.CountOfPlayers == 1) ? "" : "s") + " online";
        playersInSpace.text = PhotonNetwork.CountOfPlayersInRooms + " player" + ((PhotonNetwork.CountOfPlayersInRooms == 1) ? "" : "s") + " in space";
        amountPlayers = PhotonNetwork.CountOfPlayers;
        countOfRooms.text = PhotonNetwork.CountOfRooms + " room" + ((PhotonNetwork.CountOfRooms == 1) ? "" : "s") + " active";

        if(connectNow) Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, beginZoom - 1.5f, Time.deltaTime * 1f);

        var activePlay = PhotonNetwork.CountOfPlayersInRooms <= 0;
        AimSlider.interactable = activePlay;
        aimSliderBackground.color = new Color(aimSliderBackground.color.r, aimSliderBackground.color.g, aimSliderBackground.color.g, (activePlay) ? 1f : 0.3f);

        if(Input.GetKeyUp(KeyCode.Escape)) Screen.fullScreen = !Screen.fullScreen;
    }

    public void OnChangeSpectate(bool value) {
        if(!value) SpectIcon.sprite = player;
        else SpectIcon.sprite = looker;
        int spect = (value) ? 0 : 1;
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

    public void KickAll() {
        //Photon.Realtime.Player players = Photon.Realtime.
        //foreach(var i in PhotonNetwork.PlayerList) i
    }

    public void Connect() {
        TextureSwitcher.Detach();
        PhotonNetwork.Disconnect();

        if(controlPanel != null) controlPanel.SetActive(false);
        playersInSpace.gameObject.SetActive(false);
        playersOnline.gameObject.SetActive(false);
        countOfRooms.gameObject.SetActive(false);
        title.gameObject.SetActive(false);

        connectNow = true;
        PhotonNetwork.ConnectUsingSettings();
        //PhotonNetwork.JoinRoom(roomName);
    }

    #region MonoBehaviourPunCallbacks Callbacks

    public override void OnConnectedToMaster() {
        //Debug.LogError("CONNECT TO MASTER");
        playText.text = "PLAY";
        SpectIcon.gameObject.SetActive(true);
        playButton.interactable = true;
        if(connectNow) PhotonNetwork.JoinRoom(roomName);
    }

    public override void OnJoinedLobby() {
        Debug.LogError("Joined Lobby");
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList) {
        Debug.LogError("ROOMLISTUPDATE");
        foreach(var i in roomList) Debug.LogError(i.Name);
        base.OnRoomListUpdate(roomList);
    }

    public override void OnCreateRoomFailed(short returnCode, string message) {
        Debug.LogError("Create Room Failed: " + message);
        base.OnCreateRoomFailed(returnCode, message);
    }

    public override void OnDisconnected(DisconnectCause cause) {
        if(controlPanel != null) controlPanel.SetActive(true);
        base.OnDisconnected(cause);
    }

    public override void OnJoinRoomFailed(short returnCode, string message) {
        base.OnJoinRoomFailed(returnCode, message);
        PhotonNetwork.CreateRoom(roomName, new RoomOptions(){MaxPlayers = maxPlayers});
    }

    public override void OnJoinedRoom() {
        if(!connectNow) return;
        base.OnJoinedRoom();
//        Debug.LogError("Client joined room " + roomName);
        PhotonNetwork.LoadLevel(LEVELNAME);
    }

    #endregion
}
