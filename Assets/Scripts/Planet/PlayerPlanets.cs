using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class PlayerPlanets : MonoBehaviourPun {
    private PlanetStages stages;
    private PlayerShip player;
    [HideInInspector] public int playerNumber = 0;
    [HideInInspector] public float currentScore;
    public float minScore = 0;
    public float maxScore = 100f;
    public Text scoreText, increasePopupTxt;
    private Color orbitColor;
    public TrailRenderer orbitTrail; 
    public Orbit orbit;
    public EliminationBar rechargeBar;
    public TrailRenderer trails;

    public Color greenIncrease, redDecrease;

    public float maxScale = 4;
    private Vector3 baseScale;
    public AnimationCurve orbitScaleReduction;

    private PlanetGlow planetGlow;
    private float increasePopupHideTimer;
    
    private Outline textOutline;
    private Vector2 outlineBase;
    private Vector2 scoreBaseScale;

    private float baseWarningScale;

    public Image warningSign, warningArrow;
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

    [HideInInspector] public bool tutorial = false;

    public bool HasPlayer() {
        return player != null && playerNumber > 0;
    }

    void OnValidate() {
        if(maxScale < 0) maxScale = 0;
    }

    public void OnEnable() {
        stages = GetComponent<PlanetStages>();
        planetGlow = GetComponent<PlanetGlow>();
        rechargeBar.SetAlpha(0);
        baseWarningScale = warningSign.transform.localScale.x;
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
//        planetGlow.SetPlanet(PlanetSwitcher.GetCurrentTexturePack().planets[PlanetSwitcher.GetPlayerTintIndex(playerNumbe)]);
        var col = new Color(r, g, b);
        player.playerColor = col;
        orbitColor = player.playerColor;
        scoreText.color = player.playerColor;
        scoreText.enabled = true;
        orbitTrail.material.color = orbitColor;
        currentScore = minScore; 
        player.SetHomePlanet(gameObject);
        scoreText.transform.position = transform.position - new Vector3(0, 0.05f, 1);
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
        scoreText.transform.position = transform.position - new Vector3(0, 0.05f, 1);
    }

    void Update() {
        if(player != null && player.photonView.IsMine) {
            if(infected) {
                warningSign.transform.position = Vector3.Lerp(warningSign.transform.position, player.transform.position + Vector3.up / 1.5f, Time.deltaTime * 4f);
                warningSign.transform.localScale = Vector3.Lerp(warningSign.transform.localScale, Vector3.one * baseWarningScale + (new Vector3(1.2f, 1.2f, 1.2f) * Mathf.Sin(Time.time * 10f) * 0.02f), Time.deltaTime * 4f);
                warningArrow.transform.position = Vector3.Lerp(warningArrow.transform.position, transform.position + (new Vector3(1, 0, 0) * Mathf.Sin(Time.time * 10f) * 0.2f) + Vector3.right * 1.5f, Time.deltaTime * 4f);
            } else {
                warningSign.transform.position = Vector3.Lerp(warningSign.transform.position, new Vector3(0, 10, 0), Time.deltaTime * 8f);
                warningArrow.transform.position = Vector3.Lerp(warningArrow.transform.position, new Vector3(0, 10, 0), Time.deltaTime * 8f);
            }
        }

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

        orbit.transform.localScale = Vector3.Lerp(orbit.transform.localScale, transform.localScale / orbitScaleReduction.Evaluate(currentScore / maxScore), Time.deltaTime * 2f);
        transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(Mathf.Clamp(transform.localScale.x, 0, maxScale), Mathf.Clamp(transform.localScale.y, 0, maxScale), Mathf.Clamp(transform.localScale.z, 0, maxScale)), Time.deltaTime * 2f);

        if(scoreText != null) {
            var basePos = transform.position - new Vector3(-0.025f, 0f, 1);
            scoreText.transform.rotation = Quaternion.identity;
            scoreText.transform.position = Vector3.Lerp(scoreText.transform.position, (!tutorial) ? basePos : basePos - new Vector3(0, 0.5f, 0), Time.deltaTime * (tutorial ? 2f : 6f));

            scoreText.transform.localScale = Vector2.Lerp(scoreText.transform.localScale, scoreBaseScale, Time.deltaTime * 1f);
            textOutline.effectDistance = Vector2.Lerp(textOutline.effectDistance, outlineBase, Time.deltaTime * 1.2f);

            scoreText.text = currentScore.ToString();
            if(player != null) {
                orbitColor = player.playerColor;
                scoreText.color = orbitColor;
                orbitTrail.material.color = orbitColor * 1.5f; 
            }
        }
    }

    [PunRPC]
    public void SetResource(float i) {
        float amount = Mathf.Clamp(i, minScore, maxScore);
        currentScore = amount;

        var curStage = Mathf.RoundToInt((i / maxScore) * (float)PlanetStages.lightStageAmount);
        stages.SetLightStage((int)curStage);

        if(currentScore >= maxScore) WinGame();

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
            if(player != null) player.Destroy();
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
        SoundManager.PLAY_SOUND("ScoreDecrease");
        increasePopupTxt.enabled = true;    
        increasePopupTxt.color = redDecrease;
        increasePopupTxt.text = "-" + penalty.ToString() + "!";
        increasePopupHideTimer = 0.1f;
        increasePopupTxt.transform.localScale = Vector3.one * 4f;
    }

    public void AddingResource(float amount) {
        if(playerNumber <= 0) return;
        
        SoundManager.PLAY_SOUND("ScoreIncrease");

        currentScore += amount;
        if(currentScore > maxScore) currentScore = maxScore;
        photonView.RPC("SetResource", RpcTarget.AllBufferedViaServer, currentScore);

        increasePopupTxt.enabled = true;    
        increasePopupTxt.color = greenIncrease;
        increasePopupTxt.text = "+" + amount.ToString() + "!";
        increasePopupHideTimer = 0.1f;
        increasePopupTxt.transform.localScale = Vector3.one * 4f;
    }

    protected void WinGame() {
        photonView.RPC("SynchWin", RpcTarget.AllViaServer, player.photonView.ViewID);
    }

    [PunRPC]
    public void SynchWin(int viewID) {
        GameManager.GAME_WON = true;
        GameManager.WinState(viewID);
    }
}