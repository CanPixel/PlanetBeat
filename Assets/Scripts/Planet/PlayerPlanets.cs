using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class PlayerPlanets : MonoBehaviourPun {
    private PlayerShip player;
    public int playerNumber = 0;
    [HideInInspector] public float currentScore;
    public float minScore = 0;
    public float maxScore = 100f;
    public Text scoreText, increasePopupTxt;
    private Color orbitColor;
    public TrailRenderer orbitTrail; 
    public GameObject orbit;
    public EliminationBar rechargeBar;
    public TrailRenderer trails;

    [HideInInspector] public float wiggleSpeed = 10, wiggleRange = 100f;

    public float maxScale = 4;
    private Vector3 baseScale;
    public AnimationCurve orbitScaleReduction;

    private PlanetGlow planetGlow;
    private Vector3 basePos;
    private float wiggleOffset, increasePopupBaseSize, increasePopupHideTimer;
    
    private Outline textOutline;
    private Vector2 outlineBase;
    private Vector2 scoreBaseScale;

    private bool ScorePoint = false;
    private float lastAmount;

    [Space(5)]
    public Image[] infectionNotifiers; 
    [HideInInspector] public bool infected = false;

    [HideInInspector] public float eliminationTimer;
    private bool destructionInit = false;
    public void SetElimination(float prog) {
        if(destructionInit) return;
        eliminationTimer = prog;
        destructionInit = true;
    }

    public void RechargeElimination(float duration, float speed) {
        if(player == null) {
            rechargeBar.SetAlpha(0);
            return;
        }
        eliminationTimer = Mathf.Lerp(eliminationTimer, duration, Time.deltaTime * speed);
        var progress = Util.Map(eliminationTimer, 0, duration, 0f, 1f);
        
        rechargeBar.SetAlpha(0.65f);
        rechargeBar.SetProgress(player.playerColor, progress);
    }

    public bool HasPlayer() {
        return player != null && playerNumber > 0;
    }

    void OnValidate() {
        if(maxScale < 0) maxScale = 0;
    }

    public void OnEnable() {
        foreach(var i in infectionNotifiers) i.color = new Color(i.color.r, i.color.g, i.color.b, 0);
        wiggleOffset = Random.Range(0, 10000f);
        basePos = transform.localPosition;
        planetGlow = GetComponent<PlanetGlow>();
        rechargeBar.SetAlpha(0);
    }

    void Start() {
        scoreBaseScale = scoreText.transform.localScale;
        textOutline = scoreText.GetComponent<Outline>();
        outlineBase = textOutline.effectDistance;
        baseScale = transform.localScale;
        currentScore = minScore;
        increasePopupTxt.enabled = false;
        if(player == null) {
            scoreText.enabled = false;
            return;
        }
        increasePopupBaseSize = increasePopupTxt.transform.localScale.x;
        increasePopupTxt.transform.localScale = Vector3.zero;
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
        var fl = GetComponent<UIFloat>();
        if(fl != null) fl.SetBaseScale(baseScale);
    }
    
    [PunRPC]
    private void ClaimPlayer(int playerNumbe, float r, float g, float b) {
        playerNumber = playerNumbe;
        var pl = PhotonNetwork.GetPhotonView(playerNumber);
        if(pl != null) player = pl.GetComponent<PlayerShip>();
        if(player == null) return;
        //planetGlow.SetPlanet(PlanetSwitcher.GetCurrentTexturePack().planets[PlanetSwitcher.GetPlayerTintIndex(playerNumbe)]);
        var col = new Color(r, g, b);
        player.playerColor = col;
        //orbitColor = player.playerColor;
        //scoreText.color = player.playerColor;
        scoreText.enabled = true;
        //orbitTrail.material.color = orbitColor;
        currentScore = minScore; 
        player.SetHomePlanet(gameObject);
    }

    public Color GetColor() {
        if(player == null) return Color.black;
        return player.playerColor;
    }

    public void AssignPlayer(PlayerShip player) {
        this.player = player;
        this.player.planet = this;
        playerNumber = player.playerNumber;
        scoreText = GetComponentInChildren<Text>();
        photonView.RPC("ClaimPlayer", RpcTarget.AllBufferedViaServer, playerNumber, player.playerColor.r, player.playerColor.g, player.playerColor.b);
        scoreText.enabled = true;
    }

    void Update() {
        foreach(var i in infectionNotifiers) i.color = Color.Lerp(i.color, new Color(i.color.r, i.color.g, i.color.b, (infected && player != null && player.photonView.IsMine) ? 0.8f : 0), Time.deltaTime * 2f);

        rechargeBar.LerpAlpha(0, 4f);

        if(increasePopupHideTimer > 0) increasePopupHideTimer += Time.deltaTime;
        increasePopupTxt.transform.rotation = Quaternion.Euler(0, 0, Mathf.Sin(Time.time * 5f) * 10f);
        if(increasePopupHideTimer > 4f) {
            increasePopupTxt.transform.localScale = Vector3.Lerp(increasePopupTxt.transform.localScale, Vector3.zero, Time.deltaTime * 2f);
            if(increasePopupHideTimer > 8f) {
                increasePopupHideTimer = 0;
                increasePopupTxt.enabled = false;
            }
        }

        if(currentScore >= maxScore && player != null && GameManager.GAME_STARTED && !GameManager.GAME_WON) {
            WinGame();
            SynchWin(player.photonView.ViewID);
        }

        orbit.transform.localScale = Vector3.Lerp(orbit.transform.localScale, transform.localScale / orbitScaleReduction.Evaluate(currentScore / maxScore), Time.deltaTime * 2f);
        transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(Mathf.Clamp(transform.localScale.x, 0, maxScale), Mathf.Clamp(transform.localScale.y, 0, maxScale), Mathf.Clamp(transform.localScale.z, 0, maxScale)), Time.deltaTime * 2f);
        //transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(basePos.x + Mathf.Sin(Time.time * wiggleSpeed + wiggleOffset) * wiggleRange, basePos.y + Mathf.Sin(Time.time * wiggleSpeed + wiggleOffset) * wiggleRange, basePos.z), Time.deltaTime * 2f);

        if(scoreText != null) {
            scoreText.transform.position = transform.position - new Vector3(0, 0.05f, 10);
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
        //var newScale = transform.localScale + new Vector3(amount, amount, 0) / 50f;
        //GetComponent<UIFloat>().SetBaseScale(newScale);
    }

    public void KillPlanet() {
        photonView.RPC("KillPlayer", RpcTarget.All, playerNumber);
    }

    [PunRPC]
    public void KillPlayer(int ID) {
        if(ID == playerNumber) {
            trails.Clear();
            trails.emitting = false;
           // if(player != null) GameManager.DESTROY_SERVER_OBJECT(player);
            if(player != null) player.Destroy();
            //GameManager.DESTROY_SERVER_OBJECT(gameObject);
            //PhotonNetwork.Destroy(gameObject);
            Destroy(gameObject);
        }
    }

    public void Explode(float penalty) {
        photonView.RPC("ExplodeReduce", RpcTarget.All, penalty);
    }

    [PunRPC]
    protected void ExplodeReduce(float penalty) {
        if(playerNumber <= 0 || GameManager.GAME_WON) return;
        planetGlow.Flicker();
        if(currentScore - penalty >= 0) currentScore -= penalty;
        else currentScore = 0;
        increasePopupTxt.enabled = true;    
        increasePopupTxt.text = "-" + penalty.ToString() + "!";
        increasePopupHideTimer = 0.1f;
        increasePopupTxt.transform.localScale = Vector3.one * increasePopupBaseSize;
    }

    public void AddingResource(float amount) {
        if(playerNumber <= 0 || GameManager.GAME_WON) return;
        if(currentScore < maxScore) {
            AudioManager.PLAY_SOUND("Musicalhit", 0.7f, 0.95f);
            lastAmount = amount;
            photonView.RPC("SetResource", RpcTarget.AllBufferedViaServer, currentScore + lastAmount);
        }  
        if (currentScore <= minScore) currentScore = minScore;
        ScorePoint = true;
    }

/* 
    public void AdddOnBeat() {
        if(player == null || playerNumber <= 0 || GameManager.GAME_WON || !ScorePoint) return;
        if(ScorePoint) {
            if (currentScore < maxScore) {
                AudioManager.PLAY_SOUND("Musicalhit", 3.5f);
                var newScale = transform.localScale + new Vector3(lastAmount, lastAmount, 0) / 150f;
                newScale = new Vector3(Mathf.Clamp(newScale.x, 0, maxScale), Mathf.Clamp(newScale.y, 0, maxScale), Mathf.Clamp(newScale.z, 0, maxScale));
                textOutline.effectDistance *= 2.25f;
                scoreText.transform.localScale *= 1.2f;
                GetComponent<UIFloat>().SetBaseScale(newScale);
            }  
            AudioManager.PLAY_SOUND("collect", 2.5f);
            ScorePoint = false;
        }
    } */

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