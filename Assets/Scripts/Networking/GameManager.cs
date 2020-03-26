using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks {
   public bool isSinglePlayer = false;

   public static GameManager instance;
   [Space(10)]
   public GameObject singlePlayer;
   public GameObject playerPrefab;
   public GameObject PlayerName;
   public float playerScale = 0.02f;

   private int PLAYER_COUNT = 0;

   public static Dictionary<string, GameObject> playerLabels = new Dictionary<string, GameObject>();

   void OnValidate() {
      if(playerScale <= 0) playerScale = 0.01f;
   }

   void Start() {
      instance = this;
   }

   void Update() {
      if(Input.GetKeyUp(KeyCode.Escape)) Screen.fullScreen = !Screen.fullScreen;
   }

   public override void OnEnable() {
      base.OnEnable();
      
      if(!isSinglePlayer) {
         DestroyImmediate(singlePlayer);
         AddPlayer(PlayerShip.PLAYERNAME);
      }
   }

   public static GameObject SPAWN_SERVER_OBJECT(GameObject obj, Vector3 pos, Quaternion rot) {
      if(instance == null) return null;
      if(instance.isSinglePlayer) {
         var objF = Instantiate(obj, pos, rot);
         return objF;
      } else {
         var objF = PhotonNetwork.InstantiateSceneObject(obj.name, pos, rot, 0, null);
         return objF;
      }
   }

   public static void DESTROY_SERVER_OBJECT(GameObject obj) {
      if(instance == null) return;
      if(instance.isSinglePlayer) Destroy(obj);
      else PhotonNetwork.Destroy(obj);
   }

   private void AddPlayer(string name) {
      if(playerPrefab == null) {
         Debug.LogError("No PlayerPrefab reference!");
         return;
      }
      if(PlayerShip.LocalPlayerInstance == null) {
         var play = PLAYER_COUNT;
         if(play == 0) play = -1;
         var player = PhotonNetwork.Instantiate(playerPrefab.name, new Vector3(40 * (play), 40 * (play), 0), Quaternion.identity, 0);
         player.transform.localScale = new Vector3(playerScale, playerScale, playerScale);
         PLAYER_COUNT++;

         var playerName = Instantiate(PlayerName, player.transform.position, Quaternion.identity);
         var pN = playerName.GetComponent<PlayerName>();
         pN.SetHost(player, name);
      } 
   }

   void LoadArena() {
      if(!PhotonNetwork.IsMasterClient) {
         Debug.LogError("Trying to load level but we are not the master Client!");
         Debug.LogFormat("Loading Level : {0}", PhotonNetwork.CurrentRoom.PlayerCount);
         PhotonNetwork.LoadLevel("MULTIPLAY");
      }
   }

   public override void OnLeftRoom() {
      PhotonNetwork.LeaveLobby();
      PhotonNetwork.Disconnect();
      SceneManager.LoadScene(0);
   }

   public void LeaveRoom() {
      playerLabels.Clear();
      PhotonNetwork.LeaveRoom();
      PhotonNetwork.Disconnect();
   }

   public override void OnPlayerEnteredRoom(Player other) {
      base.OnPlayerEnteredRoom(other);
      Debug.LogError(other.NickName + " JOINED");
   }

   public override void OnPlayerLeftRoom(Player other) {
      PhotonNetwork.DestroyPlayerObjects(other); 
      Debug.LogErrorFormat("{0} LEFT", other.NickName);
      if(playerLabels.ContainsKey(other.NickName)) {
         DestroyImmediate(playerLabels[other.NickName]);
         playerLabels.Remove(other.NickName);
      }
   }
}
