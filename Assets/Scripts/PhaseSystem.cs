using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.UI;

public class PhaseSystem : MonoBehaviour {
    public Text text;

    [System.Serializable]
    public class Phase {
        public string phaseName;
        public int minScore;
        [Range(0, 1)]
        public float ratioOfPlayersWithScore;
        public UnityEvent phaseUpdate;
    }
    public List<Phase> gamePhases = new List<Phase>();

    public int phaseIndex = 0;

    private PlayerPlanets[] planets;

    void Start() {
        planets = GameObject.FindGameObjectWithTag("PLANETS").GetComponentsInChildren<PlayerPlanets>();
    }

    void Update() {
        //Phase Logic
        float currentRatio = 0;
        phaseIndex = 0;
        for(int i = 0; i < gamePhases.Count; i++) {
            int playerWealth = 0;
            var currentPhase = gamePhases[i];
            foreach(var planet in planets) if(planet.HasPlayer() && planet.currentScore >= currentPhase.minScore) playerWealth++;

            currentRatio = (playerWealth / GameManager.LIVE_PLAYER_COUNT);
            Debug.LogError(currentRatio);

            if(currentRatio >= currentPhase.ratioOfPlayersWithScore) phaseIndex = i;
            else break;
        }
//        Debug.LogError(currentRatio + " || " + gamePhases[phaseIndex].ratioOfPlayersWithScore);
        text.text = "[" + phaseIndex + "] " + gamePhases[phaseIndex].phaseName.ToUpper();

        //Phase Events
        if(phaseIndex > 0 && phaseIndex < gamePhases.Count && gamePhases[phaseIndex].phaseUpdate != null) gamePhases[phaseIndex].phaseUpdate.Invoke();
    }
}