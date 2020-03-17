using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks {
   void LoadArena() {
      if(!PhotonNetwork.IsMasterClient) {
         Debug.LogError("Trying to load level but we are not the master Client!");
         Debug.LogFormat("Loading Level : {0}", PhotonNetwork.CurrentRoom.PlayerCount);
         PhotonNetwork.LoadLevel("Room for " + PhotonNetwork.CurrentRoom.PlayerCount);
      }
   }

   public override void OnLeftRoom() {
       SceneManager.LoadScene(0);
   }

   public void LeaveRoom() {
      PhotonNetwork.LeaveRoom();
   }

   public override void OnPlayerEnteredRoom(Player other) {
      Debug.LogFormat("OnPlayerEnteredRoom() {0}", other.NickName);

      if(PhotonNetwork.IsMasterClient) {
         Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient);
         LoadArena();
      }
   }

   public override void OnPlayerLeftRoom(Player other) {
      Debug.LogFormat("OnPlayerLeftRoom() {0}", other.NickName); // seen when other disconnects

      if (PhotonNetwork.IsMasterClient) {
         Debug.LogFormat("OnPlayerLeftRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom
         LoadArena();
      }
   }
}
