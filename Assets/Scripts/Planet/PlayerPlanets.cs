using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Photon.Pun;

public class PlayerPlanets : MonoBehaviourPun {
    public AnimationClip borealisAnim;
    public GameObject tutorialColliders;
    private PlanetStages stages;
    private PlayerShip player;
    [HideInInspector] public int playerNumber = 0;
    [HideInInspector] public float currentScore;
    private float lerpScore;
    public float minScore = 0;
    public float maxScore = 100f;
    public TextMeshProUGUI scoreText, increasePopupTxt;
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

    public Image warningSign;
    
    public Sprite redWarning;
    public Sprite pinkWarning;
    public Sprite blueWarning;
    public Sprite yellowWarning;
    public Sprite greenWarning;
    public Sprite cyanWarning;

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

    private bool tutorial = false;

    public void StartTutorial() {
        tutorial = true;
    }

    public void FinishTutorial() {
        tutorial = false;
    }

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
        lerpScore = minScore;
        scoreBaseScale = scoreText.transform.localScale;

        baseScale = transform.localScale;
        currentScore = minScore;
        increasePopupTxt.enabled = false;

        if(Launcher.GetSkipCountDown()) FinishTutorial();

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
        //scoreText.color = player.playerColor;
        scoreText.enabled = true;
        orbitTrail.material.color = orbitColor;
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
        scoreText = GetComponentInChildren<TextMeshProUGUI>();
        photonView.RPC("ClaimPlayer", RpcTarget.AllBufferedViaServer, playerNumber, player.playerColor.r, player.playerColor.g, player.playerColor.b);
        scoreText.enabled = true;
        player.PositionToPlanet();
    }

    public void WarningSignSwitcher() {
        if (player.planet.gameObject == GameObject.Find("PLANETRED")) warningSign.sprite = redWarning;
        else if (player.planet.gameObject == GameObject.Find("PLANETCYAN")) warningSign.sprite = cyanWarning;
        else if (player.planet.gameObject == GameObject.Find("PLANETBLUE")) warningSign.sprite = blueWarning;
        else if (player.planet.gameObject == GameObject.Find("PLANETPINK")) warningSign.sprite = pinkWarning;
        else if (player.planet.gameObject == GameObject.Find("PLANETYELLOW")) warningSign.sprite = yellowWarning;
        else if (player.planet.gameObject == GameObject.Find("PLANETGREEN")) warningSign.sprite = greenWarning;
    }

    void Update() {
        if(tutorialColliders != null) {
            if(GameManager.GAME_STARTED) Destroy(tutorialColliders);

            tutorialColliders.transform.position = transform.position;
            tutorialColliders.transform.rotation = Quaternion.identity;
        }

        if (currentScore >= maxScore)
            PlanetStages.finalLightStage = 1;

        lerpScore = Mathf.Lerp(lerpScore, (currentScore + 1), borealisAnim.length * Time.deltaTime);
        if(currentScore == 0) lerpScore = 0;

        if(player != null && player.photonView.IsMine) {
            if(infected) {
                warningSign.transform.position = Vector3.Lerp(warningSign.transform.position, player.transform.position + Vector3.up / 2.0f, Time.deltaTime * 10f); //was 1.5 en 4
                warningSign.transform.localScale = Vector3.Lerp(warningSign.transform.localScale, Vector3.one * baseWarningScale + (new Vector3(1.2f, 1.2f, 1.2f) * Mathf.Sin(Time.time * 10f) * 0.02f), Time.deltaTime * 4f);
                WarningSignSwitcher();
            } else warningSign.transform.position = Vector3.Lerp(warningSign.transform.position, new Vector3(0, 10, 0), Time.deltaTime * 18f);
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
            var basePos = transform.position - new Vector3(-0.025f, 0.55f, 1);
            scoreText.transform.rotation = Quaternion.Euler(0, 0, 0);
            
            //if(!GameManager.GAME_STARTED) scoreText.transform.position = Vector3.Lerp(scoreText.transform.position, (!tutorial) ? basePos : basePos - new Vector3(0, 0.5f, 0), Time.deltaTime * (tutorial ? 2f : 6f));
            scoreText.transform.position = basePos;

            scoreText.transform.localScale = Vector2.Lerp(scoreText.transform.localScale, scoreBaseScale, Time.deltaTime * 1f);

            scoreText.text = ((int)lerpScore).ToString();
            if(player != null) {
                orbitColor = player.playerColor;
                //scoreText.color = orbitColor;
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
        
        if(currentScore - penalty >= 0) currentScore -= penalty;
        else currentScore = 0;
        
        if(player.photonView.IsMine) SoundManager.PLAY_SOUND("ScoreDecrease");

        increasePopupTxt.enabled = true;    
        increasePopupTxt.color = redDecrease;
        increasePopupTxt.text = "-" + penalty.ToString() + "!";
        increasePopupHideTimer = 0.1f;
        increasePopupTxt.transform.localScale = Vector3.one * 4f;

        var curStage = Mathf.RoundToInt((currentScore / maxScore) * (float)PlanetStages.lightStageAmount);
        stages.SetLightStage((int)curStage);
    }

    public void AddingResource(float amount) {
        if(playerNumber <= 0 || GameManager.GAME_WON) return;
        
        if(player.photonView.IsMine) SoundManager.PLAY_SOUND("ScoreIncrease");

        currentScore += amount;
        if(currentScore > maxScore) currentScore = maxScore;
        planetGlow.Animate();
        photonView.RPC("SetResource", RpcTarget.AllBufferedViaServer, currentScore);

        increasePopupTxt.enabled = true;    
        increasePopupTxt.color = greenIncrease;
        increasePopupTxt.text = "+" + amount.ToString() + "!";
        increasePopupHideTimer = 0.1f;
        increasePopupTxt.transform.localScale = Vector3.one * 4f;
    }

    protected void WinGame() {
        if(GameManager.GAME_WON) return;
        photonView.RPC("SynchWin", RpcTarget.AllViaServer, player.photonView.ViewID);
    }

    [PunRPC]
    public void SynchWin(int viewID) {
        GameManager.GAME_WON = true;
        GameManager.WinState(viewID);
    }
}