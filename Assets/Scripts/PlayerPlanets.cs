using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class PlayerPlanets : MonoBehaviourPun {
    private PlayerShip player;
    public int playerNumber = 0;
    public float currentScore;
    public float maxScore = 100f;
    [HideInInspector] public float minScore;
    public Text scoreText;
    private Color orbitColor;
    public TrailRenderer orbitTrail; 
    public GameObject orbit;

    [HideInInspector] public float wiggleSpeed = 10, wiggleRange = 100f;

    public float maxScale = 4;
    private Vector3 baseScale;
    public AnimationCurve orbitScaleReduction;

    private PlanetGlow planetGlow;
    private Vector3 basePos;
    private float wiggleOffset;
    
    private Outline textOutline;
    private Vector2 outlineBase;
    private Vector2 scoreBaseScale;

    public bool HasPlayer() {
        return player != null && playerNumber > 0;
    }

    void OnValidate() {
        if(maxScale < 0) maxScale = 0;
    }

    public void OnEnable() {
        wiggleOffset = Random.Range(0, 10000f);
        basePos = transform.localPosition;
        planetGlow = GetComponent<PlanetGlow>();
    }

    void Start() {
        scoreBaseScale = scoreText.transform.localScale;
        textOutline = scoreText.GetComponent<Outline>();
        outlineBase = textOutline.effectDistance;
        baseScale = transform.localScale;
        currentScore = minScore = 0;
        if(player == null) {
            scoreText.enabled = false;
            return;
        }
    }

    public bool HasReachedMax() {
        return currentScore >= maxScore;
    }

    public void SetColor(Color col) {
        orbitColor = orbitTrail.material.color = scoreText.color = col;
    }

    [PunRPC]
    public void ResetPlanet() {
        playerNumber = -1;
        player = null;
        currentScore = minScore;
        if(scoreText != null) scoreText.enabled = false;
        GetComponent<UIFloat>().SetBaseScale(baseScale);
    }
    
    [PunRPC]
    public void ClaimPlayer(int playerNumbe, float r, float g, float b) {
        playerNumber = playerNumbe;
        var pl = PhotonNetwork.GetPhotonView(playerNumber);
        if(pl != null) player = pl.GetComponent<PlayerShip>();
        if(player == null) return;
        planetGlow.SetTexture(TextureSwitcher.GetCurrentTexturePack().planets[TextureSwitcher.GetPlayerTintIndex(playerNumbe)]);
        var col = new Color(r, g, b);
        player.playerColor = col;
        orbitColor = player.playerColor;
        scoreText.color = player.playerColor;
        scoreText.enabled = true;
        orbitTrail.material.color = orbitColor;
        currentScore = minScore = 0; 
        player.SetHomePlanet(gameObject);
    }

    public void AssignPlayer(PlayerShip player) {
        this.player = player;
        this.player.homePlanet = gameObject;
        playerNumber = player.playerNumber;
        scoreText = GetComponentInChildren<Text>();
        photonView.RPC("ClaimPlayer", RpcTarget.AllBuffered, playerNumber, player.playerColor.r, player.playerColor.g, player.playerColor.b);
        scoreText.enabled = true;
    }

    void Update() {
        if(currentScore >= maxScore && player != null && GameManager.GAME_STARTED && !GameManager.GAME_WON) {
            WinGame();
            SynchWin(player.photonView.ViewID);
        }

        orbit.transform.localScale = Vector3.Lerp(orbit.transform.localScale, transform.localScale / orbitScaleReduction.Evaluate(currentScore / maxScore), Time.deltaTime * 2f);
        transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(Mathf.Clamp(transform.localScale.x, 0, maxScale), Mathf.Clamp(transform.localScale.y, 0, maxScale), Mathf.Clamp(transform.localScale.z, 0, maxScale)), Time.deltaTime * 2f);

        transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(basePos.x + Mathf.Sin(Time.time * wiggleSpeed + wiggleOffset) * wiggleRange, basePos.y + Mathf.Sin(Time.time * wiggleSpeed + wiggleOffset) * wiggleRange, basePos.z), Time.deltaTime * 2f);

        if(scoreText != null) {
            scoreText.transform.rotation = Quaternion.identity;
            scoreText.transform.localScale = Vector2.Lerp(scoreText.transform.localScale, scoreBaseScale, Time.deltaTime * 1f);
            textOutline.effectDistance = Vector2.Lerp(textOutline.effectDistance, outlineBase, Time.deltaTime * 1.2f);

            scoreText.text = currentScore.ToString("F0");
            if(player != null) {
                orbitColor = player.playerColor;
                scoreText.color = orbitColor;
                orbitTrail.material.color = orbitColor * 1.5f; 
            }
        }
    }

    [PunRPC]
    public void SetResource(float i) {
        float amount = Mathf.Clamp(i, 0, maxScore);
        currentScore = amount;
        var newScale = transform.localScale + new Vector3(amount, amount, 0) / 50f;
        GetComponent<UIFloat>().SetBaseScale(newScale);
    }

    public void AddingResource(float amount) {
        if(playerNumber <= 0 || GameManager.GAME_WON) return;
        if (currentScore < maxScore) {
            AudioManager.PLAY_SOUND("Musicalhit", 1.5f);
            var newScale = transform.localScale + new Vector3(amount, amount, 0) / 150f;
            newScale = new Vector3(Mathf.Clamp(newScale.x, 0, maxScale), Mathf.Clamp(newScale.y, 0, maxScale), Mathf.Clamp(newScale.z, 0, maxScale));

            textOutline.effectDistance *= 2.25f;
            scoreText.transform.localScale *= 1.2f;

            GetComponent<UIFloat>().SetBaseScale(newScale);
            if(photonView != null) photonView.RPC("SetResource", RpcTarget.AllBufferedViaServer, currentScore + amount);
        }  
        if (currentScore <= minScore) currentScore = minScore;

        AudioManager.PLAY_SOUND("collect", 1, 1.2f);
    }

    protected void WinGame() {
        GameManager.GAME_WON = true;
        photonView.RPC("SynchWin", RpcTarget.AllViaServer, player.photonView.ViewID);
    }

    [PunRPC]
    public void SynchWin(int viewID) {
        GameManager.GAME_WON = true;
        GameManager.WinState(viewID);
    }
}