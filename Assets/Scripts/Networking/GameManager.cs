using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks {
   public static GameManager instance;

   public GameObject playerPrefab;
   public float playerScale = 0.02f;

   void OnValidate() {
      if(playerScale <= 0) playerScale = 0.01f;
   }

   void Start() {
      instance = this;
      AddPlayer();
   }

   private void AddPlayer() {
      if(playerPrefab == null) {
         Debug.LogError("No PlayerPrefab reference!");
         return;
      }
      if(PlayerShip.LocalPlayerInstance == null) {
         var player = PhotonNetwork.Instantiate(playerPrefab.name, new Vector3(210, 0, 0), Quaternion.identity, 0);
         player.transform.localScale = new Vector3(playerScale, playerScale, playerScale);
         Debug.LogFormat("Instantiating "+ PhotonNetwork.NickName +" from {0}", SceneManagerHelper.ActiveSceneName);
      } else Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);
   }

   void LoadArena() {
      if(!PhotonNetwork.IsMasterClient) {
         Debug.LogError("Trying to load level but we are not the master Client!");
         Debug.LogFormat("Loading Level : {0}", PhotonNetwork.CurrentRoom.PlayerCount);
         PhotonNetwork.LoadLevel("MULTIPLAY");
      }
   }

   public override void OnLeftRoom() {
      SceneManager.LoadScene(0);
   }

   public void LeaveRoom() {
      PhotonNetwork.LeaveRoom();
   }

   public override void OnPlayerEnteredRoom(Player other) {
      Debug.LogFormat("Player {0} entered the room!", other.NickName);

      base.OnPlayerEnteredRoom(other);
      AddPlayer();
      //if(PhotonNetwork.IsMasterClient) {
       //  Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient);
        // LoadArena();
     // }
   }

   public override void OnPlayerLeftRoom(Player other) {
      Debug.LogFormat("Player {0} left the room!", other.NickName); // seen when other disconnects

      if (PhotonNetwork.IsMasterClient) {
         Debug.Log("You are the MasterClient now"); // called before OnPlayerLeftRoom
         LoadArena();
      }
   }
}
