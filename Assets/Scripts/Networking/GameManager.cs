using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks {
   public static GameManager instance;

   public GameObject playerPrefab, PlayerName;
   public GameObject[] rocks;
   [Range(0, 10)]
   public int startAsteroidAmount = 1;
   public float playerScale = 0.02f;

   private int PLAYER_COUNT = 0;

   public static Dictionary<string, GameObject> playerLabels = new Dictionary<string, GameObject>();

   void OnValidate() {
      if(playerScale <= 0) playerScale = 0.01f;
   }

   void Start() {
      instance = this;
   }

   public override void OnEnable() {
      base.OnEnable();
      AddPlayer(PlayerShip.PLAYERNAME);
   }

   void Update() {
      if(PhotonNetwork.IsMasterClient && Input.GetKeyUp(KeyCode.R)) for(int i = 0; i < startAsteroidAmount; i++) AddRock();
   }

   private void AddRock() {
      float x = Random.Range(50, 400);
      float y = Random.Range(50, 400);
      if(Random.Range(0, 2) == 0) x = -x;
      if(Random.Range(0, 2) == 0) y = -y;
      var randRock = rocks[Random.Range(0, rocks.Length)];
      var rock = PhotonNetwork.InstantiateSceneObject(randRock.name, new Vector3(x, y, 0), Quaternion.identity, 0, null);
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
      //Debug.LogFormat("Player {0} entered the room!", other.NickName);
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
