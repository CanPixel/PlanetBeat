using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks {
   public bool isSinglePlayer = false;
   public bool spectating = false;

   public static Dictionary<string, GameObject> playerLabels = new Dictionary<string, GameObject>();

   public string feedbackURL = "";

   public static GameManager instance;
   [Space(10)]
   public GameObject singlePlayer;
   public GameObject playerPrefab;
   public GameObject PlayerName;
   public float playerScale = 0.02f;

   private int PLAYER_COUNT = 0;

   public static Dictionary<int, int> ActorToViewID = new Dictionary<int, int>();
   private static List<PlayerPlanets> allPlanets = new List<PlayerPlanets>();

   public static void ClaimPlanet(PlayerShip ship) {
      if(ship.photonView != null && ship.photonView.IsMine) instance.ClaimFreePlanet(ship);
   }

   //ONLY LOCAL PLAYERS
   private void ClaimFreePlanet(PlayerShip player) {
      if(player.GetHomePlanet() != null || player.photonView == null) return;

      int playerVal = TextureSwitcher.GetPlayerTintIndex(player.photonView.ViewID);
      if(PhotonNetwork.IsMasterClient) ChoosePlanet(player);
      else photonView.RPC("RequestPlanet", RpcTarget.MasterClient, playerVal, player.photonView.ViewID);
   }

   [PunRPC]
   public void RequestPlanet(int playerVal, int viewID) {
      var player = PhotonNetwork.GetPhotonView(viewID).GetComponent<PlayerShip>();
      ChoosePlanet(player);
   }

   private void ChoosePlanet(PlayerShip player) {
      for(int i = 0; i < allPlanets.Count; i++) {
         var planet = allPlanets[i];
         if(planet.HasPlayer()) continue;
         player.SetHomePlanet(planet.gameObject);
         planet.AssignPlayer(player);
         break;
      }
   }

   void OnValidate() {
      if(playerScale <= 0) playerScale = 0.01f;
   }

   void Update() {
      spectating = PlayerPrefs.GetInt("Spectate") == 0;
      if(Input.GetKeyUp(KeyCode.Escape)) Screen.fullScreen = !Screen.fullScreen;
   }

   public override void OnEnable() {
      if(instance == null) instance = this;
      base.OnEnable();

      TextureSwitcher.ForceUpdateTextures();

      allPlanets.Clear();
      var plans = GameObject.FindGameObjectsWithTag("PLAYERPLANET");
      foreach(var i in plans) allPlanets.Add(i.GetComponent<PlayerPlanets>());
      
      if(!isSinglePlayer) {
         DestroyImmediate(singlePlayer);
         if(PlayerPrefs.GetInt("Spectate") != 0) AddPlayer(PlayerShip.PLAYERNAME);
      }
      AssignPlayerIdentity(1001);
   }

   public static void AssignPlayerIdentity(int ID) {
      var photon = PhotonNetwork.GetPhotonView(ID);
      if(photon == null) return;
      var pl = photon.GetComponent<PlayerShip>();
      if(pl == null) return;
      pl.playerColor = TextureSwitcher.GetPlayerTint(ID);
      var col = pl.playerColor;
      if(instance != null) instance.photonView.RPC("AssignMasterPlanet", RpcTarget.AllViaServer, ID, col.r, col.g, col.b);
   }

   [PunRPC]
   public void AssignMasterPlanet(int playerNum, float r, float g, float b) {
      PhotonNetwork.GetPhotonView(playerNum).GetComponent<PlayerShip>().ForceColor(r, g, b);
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
      if(instance.isSinglePlayer || !PhotonNetwork.IsMasterClient) Destroy(obj);
      else {
         if(PlayerShip.LocalPlayerInstance.GetPhotonView().IsMine) PhotonNetwork.Destroy(obj);
      }
   }

   private void AddPlayer(string name) {
      if(playerPrefab == null) {
         Debug.LogError("No PlayerPrefab reference!");
         return;
      }
      //Adds the local player (Client)
      if(PlayerShip.LocalPlayerInstance == null) {
         TextureSwitcher.ForceUpdateTextures();
         var play = PLAYER_COUNT;
         if(play == 0) play = -1;
         var player = PhotonNetwork.Instantiate(playerPrefab.name, new Vector3(40 * play, 40 * play, 0), Quaternion.identity, 0);
         PlayerShip.LocalPlayerInstance = player;
         player.transform.localScale = new Vector3(playerScale, playerScale, playerScale);
         PLAYER_COUNT++;
         var playerShip = player.GetComponent<PlayerShip>();
         var playerName = Instantiate(PlayerName, player.transform.position, Quaternion.identity);
         var pN = playerName.GetComponent<PlayerName>();
         pN.SetHost(player, name);
         playerShip.playerName = pN;
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
      //Debug.LogError(other.NickName + " JOINED");
   }

   public override void OnPlayerLeftRoom(Player other) {
      //Debug.LogErrorFormat("{0} LEFT", other.NickName);
      int viewID = -1;
      foreach(KeyValuePair<int, int> pair in ActorToViewID) if(pair.Key == other.ActorNumber) {
         viewID = pair.Value;
         break;
      }

      //Clear Player Name
      if(playerLabels.ContainsKey(other.NickName)) {
         DestroyImmediate(playerLabels[other.NickName]);
         playerLabels.Remove(other.NickName);
      }

      photonView.RPC("ClearPlanet", RpcTarget.All, viewID, other.ActorNumber);
   }
   [PunRPC]
   public void ClearPlanet(int viewID, int ActorNumber) {
      foreach(var i in allPlanets) {
         if(i.playerNumber == viewID) {
            i.ResetPlanet(viewID);
            allPlanets.Remove(i);
            if(ActorToViewID.ContainsKey(ActorNumber)) ActorToViewID.Remove(ActorNumber);
            break;
         }
      }
   }

   public void AccessFeedback() {
      Application.OpenURL(feedbackURL);
   }
}
