using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;

[RequireComponent(typeof(InputField))]
public class PlayerNameInputField : MonoBehaviour {
    const string playerNamePrefKey = "PlayerName";

    void Start() {
        string defaultName = string.Empty;
        InputField input = this.GetComponent<InputField>();
        if(input != null) {
            if(PlayerPrefs.HasKey(playerNamePrefKey)) {
                defaultName = PlayerPrefs.GetString(playerNamePrefKey);
                input.text = defaultName;
            }
        }
        PhotonNetwork.NickName = defaultName;
    }

    public void SetPlayerName(string value) {
        if(string.IsNullOrEmpty(value)) {
            Debug.LogError("Player Name is empty!");
            return;
        }
        PlayerPrefs.SetString(playerNamePrefKey, value);
    }
}
