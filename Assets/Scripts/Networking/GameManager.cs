using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviourPunCallbacks {
   public bool spectating = false;

   public Background background;
   public GameObject gameField;
   private float gameFieldScale;
   public float gameFieldStartScale;
   public Text winText;
   private bool turnValue;

   public Text countdown;
   [Header("Start Game")]
   public int playerCount = 4;
   private bool skipCountdown = false;  

   public static Dictionary<string, GameObject> playerLabels = new Dictionary<string, GameObject>();

   public static bool GAME_STARTED = false, GAME_WON = false;
   private bool startCountdown = false;
   private float countdownTimer, startupDelayTimer;
   private int count = 3;

   public static GameManager instance;
   [Space(10)]
   public GameObject playerPrefab;
   public GameObject PlayerName;
   public float playerScale = 0.02f;

   public static Dictionary<int, int> ActorToViewID = new Dictionary<int, int>();
   private static List<PlayerPlanets> allPlanets = new List<PlayerPlanets>();

   public static PlayerShip LOCAL_PLAYER;

   public static void ClaimPlanet(PlayerShip ship) {
      if(ship.photonView.IsMine) instance.ClaimFreePlanet(ship);
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

   public static void ClickSound(float pitch) {
      AudioManager.PLAY_SOUND("click", 1, pitch);
   }

   void OnValidate() {
      if(playerScale <= 0) playerScale = 0.01f;
   }

   void Update() {
      gameField.transform.localScale = Vector3.Lerp(gameField.transform.localScale, gameFieldScale * Vector3.one, Time.deltaTime * 7f);

      spectating = PlayerPrefs.GetInt("Spectate") == 0;
      if(Input.GetKeyUp(KeyCode.Escape)) Screen.fullScreen = !Screen.fullScreen;

     if(PhotonNetwork.IsMasterClient && !GAME_STARTED) {
         var plays = GameObject.FindGameObjectsWithTag("PLAYERSHIP");
         if(plays.Length >= playerCount && !startCountdown) {
            photonView.RPC("EnableCountdown", RpcTarget.All, count);
            startCountdown = true;
         }
      }

      if(startCountdown && PhotonNetwork.IsMasterClient) {
         countdownTimer += Time.deltaTime;
         if(countdownTimer > 1) {
            if(count > 0) {
               count--;
               AudioManager.PLAY_SOUND("click");
            }
            else GAME_STARTED = true;
            photonView.RPC("EnableCountdown", RpcTarget.All, count);
            countdownTimer = 0;
         }
      }

      if(startupDelayTimer > 0.8f && skipCountdown) GAME_STARTED = true;
      else startupDelayTimer += Time.deltaTime;

      BeatPulse.BEGIN = GAME_STARTED;
   }

   [PunRPC]
   public void EnableCountdown(int count) {
      startCountdown = true;
      countdown.gameObject.SetActive(count > 0);
      this.count = count;
      countdown.text = count.ToString();
      if(count <= 0) {
         background.TURN = true;
         GAME_STARTED = true;
         countdown.gameObject.SetActive(false);
      }
   }

   public override void OnEnable() {
      if(instance == null) instance = this;
      gameFieldScale = gameField.transform.localScale.x;
      gameField.transform.localScale = Vector3.one * gameFieldStartScale;

      base.OnEnable();
      Random.InitState((int)Time.time * 1000);

      turnValue = background.TURN;
      background.TURN = false;

      GAME_WON = false;

      startupDelayTimer = 0;
      count = 3;
      countdownTimer = 0;
      startCountdown = false;
      GAME_STARTED = false;

      TextureSwitcher.ForceUpdateTextures();

      allPlanets.Clear();
      var plans = GameObject.FindGameObjectsWithTag("PLAYERPLANET");
      foreach(var i in plans) allPlanets.Add(i.GetComponent<PlayerPlanets>());

      if(PlayerPrefs.GetInt("Spectate") != 0) AddLocalClient(PlayerShip.PLAYERNAME);

      skipCountdown = Launcher.GetSkipCountDown();
      AudioManager.PLAY_SOUND("sizzle", 1f, 1.5f);
   }

   protected void AssignPlayerIdentity(int ID) {
      var playerID = TextureSwitcher.GetPlayerTintIndex(ID);
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
      var play = PhotonNetwork.GetPhotonView(playerNum);
      if(play == null) return;
      var pl = play.GetComponent<PlayerShip>();
      pl.ForceColor(r, g, b);
   }

   public static GameObject SPAWN_SERVER_OBJECT(GameObject obj, Vector3 pos, Quaternion rot) {
      if(instance == null) return null;
      var objF = PhotonNetwork.InstantiateSceneObject(obj.name, pos, rot, 0, null);
      return objF;
   }
   public static void DESTROY_SERVER_OBJECT(GameObject obj) {
      if(instance == null) return;
      if(!PhotonNetwork.IsMasterClient) Destroy(obj);
      else if(LOCAL_PLAYER.photonView.IsMine) PhotonNetwork.Destroy(obj);
   }

   private void AddLocalClient(string name) {
      if(LOCAL_PLAYER == null) {
         TextureSwitcher.ForceUpdateTextures();
         var player = PhotonNetwork.Instantiate(playerPrefab.name, Vector3.zero, Quaternion.identity, 0);
         player.transform.localScale = new Vector3(playerScale, playerScale, playerScale);
         var playerShip = player.GetComponent<PlayerShip>();
         LOCAL_PLAYER = playerShip;
         var playerName = Instantiate(PlayerName, player.transform.position, Quaternion.identity);

         var pN = playerName.GetComponent<PlayerName>();
         pN.SetHost(player, name);
         playerShip.playerName = pN;
         AssignPlayerIdentity(playerShip.photonView.ViewID);
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

   public static void WinState(int viewID) {
      var player = PhotonNetwork.GetPhotonView(viewID).GetComponent<PlayerShip>();
      instance.winText.text = "Player <color='#"+ColorUtility.ToHtmlStringRGB(player.playerColor).ToString()+"'> " + player.playerName.GetName() + "</color> wins!";
      instance.winText.gameObject.SetActive(true);
   }

   public override void OnPlayerEnteredRoom(Player other) {
      base.OnPlayerEnteredRoom(other);
//      Debug.LogError(other.NickName + " JOINED");
   }

   public override void OnPlayerLeftRoom(Player other) {
    //  Debug.LogErrorFormat("{0} LEFT", other.NickName);
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
            i.ResetPlanet();
            allPlanets.Remove(i);
            if(ActorToViewID.ContainsKey(ActorNumber)) ActorToViewID.Remove(ActorNumber);
            break;
         }
      }
   }
}
