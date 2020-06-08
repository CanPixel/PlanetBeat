using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class PlayerReady : MonoBehaviourPun {
    public TextMeshProUGUI text;

    public TMP_FontAsset red, blue, green, yellow, cyan, pink;

    [System.Serializable]
    public enum PlayerColor {
        RED, BLUE, YELLOW, GREEN, CYAN, PINK
    }

    [PunRPC]
    public void Set(string name, Vector3 pos, PlayerColor col) {
        switch(col) {
            case PlayerColor.RED:
                text.font = red;
                break;
            case PlayerColor.BLUE:
                text.font = blue;
                break;
            case PlayerColor.YELLOW:
                text.font = yellow;
                break;
            case PlayerColor.GREEN:
                text.font = green;
                break;
            case PlayerColor.CYAN:
                text.font = cyan;
                break;
            default:
            case PlayerColor.PINK:
                text.font = pink;
                break;
        }
        text.text = "READY";
        this.transform.position = pos;
    }

    void Update() {
        if(GameManager.GAME_STARTED) {
            GameManager.DESTROY_SERVER_OBJECT(gameObject);
            Destroy(gameObject);
        }
    }
}
