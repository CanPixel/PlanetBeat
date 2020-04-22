using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class AsteroidNetwork : MonoBehaviourPun {
    void OnCollisionEnter2D(Collision2D col) {
        if(col.gameObject.tag == "PLAYERSHIP") {
            if(photonView != null) photonView.TransferOwnership(PhotonNetwork.LocalPlayer.ActorNumber);
        }
    }
}
