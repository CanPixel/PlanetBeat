using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks {
   public bool isSinglePlayer = false;
   public bool spectating = false;

   public static GameManager instance;
   [Space(10)]
   public GameObject singlePlayer;
   public GameObject playerPrefab;
   public GameObject PlayerName;
   public float playerScale = 0.02f;

   private int PLAYER_COUNT = 0;

   public static Dictionary<string, GameObject> playerLabels = new Dictionary<string, GameObject>();
   private static List<PlayerShip> players = new List<PlayerShip>();

   [SerializeField] private List<PlayerPlanets> planetsAvailable = new List<PlayerPlanets>(); 
   public static Dictionary<PlayerPlanets, PlayerShip> playerPlanets = new Dictionary<PlayerPlanets, PlayerShip>();

   public static void AddPlayerToList(PlayerShip obj) {
      players.Remove(obj);
   }

   public static void ClaimPlanet(PlayerShip ship) {
      instance.ClaimFreePlanet(ship);
   }

   private void ClaimFreePlanet(PlayerShip player) {
      if(player.homePlanet != null) return;
      foreach(var i in planetsAvailable) {
         if(i.HasPlayer()) continue;
         var planet = i;
         player.homePlanet = planet.gameObject;
         planet.AssignPlanet(player);
         instance.planetsAvailable.Remove(i);
         Debug.Log("Player assigned to planet " + planet.name);
         break;
        }
   }

   public static List<PlayerShip> GetPlayerList() {
      return players;
   }

   void OnValidate() {
      if(playerScale <= 0) playerScale = 0.01f;
   }

   void Update() {
      spectating = PlayerPrefs.GetInt("Spectate") == 0;
      if(Input.GetKeyUp(KeyCode.Escape)) Screen.fullScreen = !Screen.fullScreen;

      //Actual playerlist
      var list = PhotonNetwork.FindGameObjectsWithComponent(typeof(PlayerShip));
      players.Clear();
      foreach(var i in list) players.Add(i.GetComponent<PlayerShip>());
   }

   public override void OnEnable() {
      if(instance == null) instance = this;
      base.OnEnable();
      
      if(!isSinglePlayer) {
         DestroyImmediate(singlePlayer);
         if(PlayerPrefs.GetInt("Spectate") != 0) AddPlayer(PlayerShip.PLAYERNAME);
      }
   }

   void Awake() {
      planetsAvailable.Clear();
      var planetList = GameObject.FindGameObjectsWithTag("PLAYERPLANET");
      foreach(var i in planetList) planetsAvailable.Add(i.GetComponent<PlayerPlanets>());
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
      else {
         if(PlayerShip.LocalPlayerInstance.GetPhotonView().IsMine) PhotonNetwork.Destroy(obj);
      }
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
         var playerShip = player.GetComponent<PlayerShip>();
         playerShip.playerColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
         var playerName = Instantiate(PlayerName, player.transform.position, Quaternion.identity);
         var pN = playerName.GetComponent<PlayerName>();
         pN.SetHost(player, name);
      } 
   }

   void LoadArena() {
      if(!PhotonNetwork.IsMasterClient) {
         Debug.LogError("Trying to load level but we are not the master Client!");
         Debug.LogFormat("Loading Level : {0}", PhotonNetwork.CurrentRoom.PlayerCount);
         PhotonNetwork.LoadLevel(Launcher.levelName);
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
