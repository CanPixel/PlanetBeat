using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerNetworking : MonoBehaviour {
    [SerializeField] private GameObject playerCamera;
    [SerializeField] private MonoBehaviour[] scriptsToIgnore;

    PhotonView photonView;

    void Start() {
        photonView = GetComponent<PhotonView>();
        Initialize();
    }

    void Initialize() {
        if(photonView.IsMine) ;
        else {
            playerCamera.SetActive(false);
            foreach(MonoBehaviour item in scriptsToIgnore) item.enabled = false;
        }
    }
}
