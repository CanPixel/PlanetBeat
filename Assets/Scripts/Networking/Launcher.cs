using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class Launcher : MonoBehaviourPunCallbacks {
    [Header("REFERENCES")]
    [SerializeField] private GameObject controlPanel;
    [SerializeField] private GameObject progressLabel;

    public Slider SpectSlider;
    public Text particiText;
    private bool spectate = false;

    string gameVersion = "1";

    bool isConnecting;

    [Space(15)]
    public string LEVELNAME = "MULTIPLAY";

    [Range(1, 20)]
    [SerializeField] private byte maxPlayers = 5;

    void Awake() {
        PhotonNetwork.AutomaticallySyncScene = true;
        if(progressLabel != null) progressLabel.SetActive(false);
        if(controlPanel != null) controlPanel.SetActive(true);

        SpectSlider.value = PlayerPrefs.GetInt("Spectate");
    }

    void Update() {
        if(Input.GetKeyUp(KeyCode.Escape)) Screen.fullScreen = !Screen.fullScreen;
    }

    public void OnChangeSpectate(System.Single value) {
        if(value == 0) particiText.enabled = false;
        else particiText.enabled = true;
        spectate = value == 0;
        PlayerPrefs.SetInt("Spectate", (int)value);
    }

    public void Quit() {
        Application.Quit();
    }

    public void Connect() {
        PhotonNetwork.Disconnect();

        isConnecting = PhotonNetwork.ConnectUsingSettings();

        if(progressLabel != null) progressLabel.SetActive(true);
        if(controlPanel != null) controlPanel.SetActive(false);
        
        if(!PhotonNetwork.IsConnected) PhotonNetwork.JoinRandomRoom();
        else {
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = gameVersion;
        }
    }

    #region MonoBehaviourPunCallbacks Callbacks

    public override void OnConnectedToMaster() {
        if(isConnecting) {
            PhotonNetwork.JoinRandomRoom();
            isConnecting = false;
        }
    }

    public override void OnDisconnected(DisconnectCause cause) {
        if(progressLabel != null) progressLabel.SetActive(false);
        if(controlPanel != null) controlPanel.SetActive(true);
    }

    public override void OnJoinRandomFailed(short returnCode, string message) {
        Debug.Log("JoinRandomFailed(): No random room available, so we create one.");
        PhotonNetwork.CreateRoom(null, new RoomOptions(){MaxPlayers = maxPlayers});
    }

    public override void OnJoinedRoom() {
        Debug.Log("OnJoinedRoom(): Now this client is in a room.");
        PhotonNetwork.LoadLevel(LEVELNAME);
    }

    #endregion
}
