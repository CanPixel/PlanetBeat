using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Launcher : MonoBehaviourPunCallbacks {
    [Header("REFERENCES")]
    [SerializeField] private GameObject controlPanel;
    [SerializeField] private GameObject progressLabel;

    string gameVersion = "1";

    bool isConnecting;

    [Range(1, 20)]
    [SerializeField] private byte maxPlayers = 4;

    void Awake() {
        PhotonNetwork.AutomaticallySyncScene = true;
        if(progressLabel != null) progressLabel.SetActive(false);
        if(controlPanel != null) controlPanel.SetActive(true);
    }

    public void Connect() {
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
        progressLabel.SetActive(false);
        controlPanel.SetActive(true);
        Debug.LogWarningFormat("DISCONNECT was called because of {0}", cause);
    }

    public override void OnJoinedLobby() {
        
    }

    public override void OnJoinRandomFailed(short returnCode, string message) {
        Debug.Log("JoinRandomFailed(): No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");

        // #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
        PhotonNetwork.CreateRoom(null, new RoomOptions(){MaxPlayers = maxPlayers});
    }

    public override void OnJoinedRoom() {
        Debug.Log("OnJoinedRoom(): Now this client is in a room.");
        PhotonNetwork.LoadLevel("MULTIPLAY");
    }

    #endregion
}
