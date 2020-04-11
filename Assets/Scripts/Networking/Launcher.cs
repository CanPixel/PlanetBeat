using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class Launcher : MonoBehaviourPunCallbacks, ILobbyCallbacks, IInRoomCallbacks {
    [System.Serializable]
    public class SpaceRoom {
        public string name;
        public Sprite icon;
        public bool noCountdown = false;
    }
    public SpaceRoom[] rooms = new SpaceRoom[3];
    public Vector2 roomLabelSize = new Vector2(225, 50);

    string gameVersion = "1";

    [Space(15)]
    public string levelSceneName = "Space";

    private static Launcher self;

    [Range(1, 20)]
    [SerializeField] private byte maxPlayers = 5;

    [Header("REFERENCES")]
    [SerializeField] private GameObject controlPanel;
    public Sprite freeAim, lockOn, looker, player;

    public Button playButton;

    public Toggle SpectToggle;
    public Image SpectIcon;
    public Slider AimSlider;
    public Text lockonText, playText, playersOnline, playersInSpace, countOfRooms, title;
    public Image reticleIcon, aimSliderBackground;
    [Header("ROOM SELECTION")]
    public ToggleGroup roomGroup;
    public List<SpaceRoomToggle> roomList = new List<SpaceRoomToggle>();
    public GameObject roomTogglePrefab;

    private int amountPlayers;
    private bool connectNow = false;
    private float beginZoom;

    private int selectedRoom = -1;
    private int roomInit = 0;

    private bool update = true;

    public delegate bool SimpleAction();
    private Coroutine _connectingCoroutine;

    void Awake() {
        roomInit = 0;
        beginZoom = Camera.main.orthographicSize;
        playButton.interactable = false;
        playText.text = "CONNECTING...";

        if(self == null){ 
            self = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(self.gameObject);
            self = this;
        }

        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.NetworkingClient.EnableLobbyStatistics = true;
        SpectIcon.gameObject.SetActive(false);
        if(controlPanel != null) controlPanel.SetActive(true);

        selectedRoom = PlayerPrefs.GetInt("DEFAULT_ROOM");
        for(int i = 0; i < rooms.Length; i++) AddRoomToList(rooms[i], i);
        SetRoomSelection(selectedRoom);

        SpectToggle.isOn = false;
        OnChangeSpectate(SpectToggle.isOn);
        AimSlider.value = PlayerPrefs.GetInt("AIM_MODE");
        OnChangeAim(PlayerPrefs.GetInt("AIM_MODE"));

        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.ConnectUsingSettings();
    }

    private void AddRoomToList(SpaceRoom room, int i) {
        var img = room.icon;
        var name = room.name;
        Random.InitState(32452545 * i); 
        var obj = Instantiate(roomTogglePrefab).GetComponent<SpaceRoomToggle>();
        obj.SetProperties(name, img, roomGroup, i, maxPlayers, Random.ColorHSV(0, 1, 0, 1, 0, 0.8f));
        obj.transform.SetParent(roomGroup.transform);
        obj.transform.localScale = Vector3.one;
        obj.transform.localPosition = new Vector3(0, (roomLabelSize.y * (rooms.Length - 1)) / 2f - i * roomLabelSize.y, 0);
        obj.GetComponent<RectTransform>().sizeDelta = roomLabelSize;
        roomList.Add(obj);
    }

    public void ClearSelection() {
        for(int i = 0; i < roomList.Count; i++) roomList[i].SetState(false);
    }
    public void SetRoomSelection(int index, bool reset = false) {
        if(reset) ClearSelection();
        selectedRoom = index;
        if(roomList.Count > index) roomList[index].SetState(true);
        RoomListSelectChange(index);
    }

    void Update() {
        if(!update) return; 

        playersOnline.text = PhotonNetwork.CountOfPlayers + " player" + ((PhotonNetwork.CountOfPlayers == 1) ? "" : "s") + " online";
        playersInSpace.text = PhotonNetwork.CountOfPlayersInRooms + " player" + ((PhotonNetwork.CountOfPlayersInRooms == 1) ? "" : "s") + " in space";
        amountPlayers = PhotonNetwork.CountOfPlayers;
        countOfRooms.text = PhotonNetwork.CountOfRooms + " room" + ((PhotonNetwork.CountOfRooms == 1) ? "" : "s") + " active";

        if(connectNow) Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, beginZoom - 1.5f, Time.deltaTime * 1f);

        if(Input.GetKeyUp(KeyCode.Escape)) Screen.fullScreen = !Screen.fullScreen;
    }

    public void OnChangeSpectate(bool value) {
        if(!value) SpectIcon.sprite = player;
        else SpectIcon.sprite = looker;
        int spect = (value) ? 0 : 1;
        PlayerPrefs.SetInt("Spectate", spect);
    }
    public void OnChangeAim(System.Single value) {
        if(value == 0) {
            lockonText.enabled = false;
            reticleIcon.sprite = freeAim;
    }
        else {
            lockonText.enabled = true;
            reticleIcon.sprite = lockOn;
        }
        PlayerPrefs.SetInt("AIM_MODE", (int)value);
    }

    public void ClickSound(float pitch) {
        AudioManager.PLAY_SOUND("click", 1, pitch);
    }

    public void Quit() {
        Application.Quit();
    }

    public void Connect() {
        TextureSwitcher.Detach();

        if(controlPanel != null) controlPanel.SetActive(false);
        playersInSpace.gameObject.SetActive(false);
        playersOnline.gameObject.SetActive(false);
        countOfRooms.gameObject.SetActive(false);
        title.gameObject.SetActive(false);

        connectNow = true;
        EnterRoom(PlayerPrefs.GetInt("DEFAULT_ROOM"));
    }

    #region MonoBehaviourPunCallbacks Callbacks
    
    public override void OnConnectedToMaster() {
        if(connectNow) PhotonNetwork.JoinLobby();
        if(playText != null) playText.text = "PLAY";
        SpectIcon.gameObject.SetActive(true);
        playButton.interactable = true;
    }

    public override void OnJoinedLobby() {
        CreateRooms();
    }

    private void EnterRoom(int i) {
        var options = new RoomOptions();
        options.MaxPlayers = maxPlayers;
        options.IsVisible = true;
        options.IsOpen = true;
        PhotonNetwork.JoinOrCreateRoom(roomList[i].roomName, options, TypedLobby.Default);
    }

    private bool CreateRooms(bool join = false) {
        var options = new RoomOptions();
        options.MaxPlayers = maxPlayers;
        options.IsVisible = true;
        options.IsOpen = true;
        if(roomInit >= roomList.Count) return true;
        bool val = (join)? PhotonNetwork.JoinOrCreateRoom(roomList[roomInit].roomName, options, TypedLobby.Default) : PhotonNetwork.CreateRoom(roomList[roomInit].roomName, options, TypedLobby.Default);
        if(val) {
            if(roomInit < roomList.Count && !val) roomInit++;
            if(!join) PhotonNetwork.ConnectUsingSettings();
            return true;
        }
        return false;
    }

    public override void OnDisconnected(DisconnectCause cause) {
        PhotonNetwork.LeaveLobby();
        self = null;
        Destroy(gameObject);
        if(controlPanel != null) controlPanel.SetActive(true);
        base.OnDisconnected(cause);
    }

    public override void OnJoinedRoom() {
        base.OnJoinedRoom();
        PhotonNetwork.LoadLevel(levelSceneName);
        update = connectNow = false;
    }
    #endregion

    public void RoomListSelectChange(int chosenRoom) {
        PlayerPrefs.SetInt("DEFAULT_ROOM", chosenRoom);
    }

    public static bool GetSkipCountDown() {
        return self.rooms[self.selectedRoom].noCountdown;
    }
}
