using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

public class PhaseSystem : MonoBehaviour {
    private PlanetPositioner planetPositioner;
    private PlayerPlanets[] planets;

    public static float highestScore;

    public AudioMixer layerMixer;
    public AudioMixer AnticipationMixer;

    public AudioMixerSnapshot phase1;
    public AudioMixerSnapshot phase2;
    public AudioMixerSnapshot phase3;
    public AudioMixerSnapshot phase4;
    public AudioMixerSnapshot phase5;

    public AudioMixerSnapshot anticipationPhase;
    public AudioMixerSnapshot anticipationNewPhase;
    public AudioMixerSnapshot anticipationPhaseOff;
    public static bool anticipationPhasePlay = false;

    private int omDeVierLoops = 4;
    public static int newPhaseWaitTimer = 0;

    private int phaseOneTimer = 0;
    private int phaseTwoTimer = 0;
    private int phaseThreeTimer = 0;
    private int phaseFourTimer = 0;
    private int phaseFiveTimer = 0;
    private int phaseSixTimer = 0;

    private bool newPhase = false;
    public static bool goingIntoNewPhase = false;
    private int goingIntoNewPhaseSetToFalse = 0;


    public static bool AsteroidAndPhaseSwitch = false;


    public static bool startOpenBlackHole = false;
    public static bool startResourceSpawning = false;

    //Dit script kijkt of de phases wel in de juiste volgorde loopt
    private int PhaseChecker = 1;

    AudioSource m_AudioSource;

    public static bool phaseOne, phaseTwo, phaseThree, phaseFour, phaseFive;

    [Header("Test")]
    [Space(10)]
    public GameObject phaseOneUI;
    public GameObject phaseTwoUI;
    public GameObject phaseThreeUI;
    public GameObject phaseFourUI;
    public GameObject phaseFiveUI;
    [Space(5)]
    public GameObject waitingForPhaseChangeUI;

    void Start() {
        m_AudioSource = GetComponent<AudioSource>();
        //Debug.Log("Audiosource lenght: " + m_AudioSource.clip.length);

        planetPositioner = GameObject.FindGameObjectWithTag("PLANETS").GetComponent<PlanetPositioner>();
        planets = planetPositioner.GetPlanets();

        phase1.TransitionTo(0.0f);

        phaseOne = true;
        phaseTwo = false;
        phaseThree = false;
        phaseFour = false;
        phaseFive = false;

        waitingForPhaseChangeUI.SetActive(false);
    }

    void Update()
    {
        //Debug.Log("newPhaseWaitTimer: " + newPhaseWaitTimer);
        //Debug.Log("goingIntoNewPhaseSetToFalse: " + goingIntoNewPhaseSetToFalse);

        //Test UI
        if (goingIntoNewPhase == true)
        {
            waitingForPhaseChangeUI.SetActive(true);
        }
        else
        {
            waitingForPhaseChangeUI.SetActive(false);
        }

        //PhaseCheck

        if (phaseOne == true)
        {
            phase1.TransitionTo(0.0f);

            // -------------------------- TestUI
            phaseOneUI.SetActive(true);
            phaseTwoUI.SetActive(false);
            phaseThreeUI.SetActive(false);
            phaseFourUI.SetActive(false);
            phaseFiveUI.SetActive(false);
        }

        if (phaseTwo == true)
        {
            phase2.TransitionTo(0.0f);

            goingIntoNewPhaseSetToFalse++;
            if (goingIntoNewPhaseSetToFalse == 1)
            {
                goingIntoNewPhase = false; //Mag maar 1 keer gebeuren. (Hij gaat steeds weer op true)
            }

            // -------------------------- TestUI
            phaseOneUI.SetActive(true);
            phaseTwoUI.SetActive(true);
            phaseThreeUI.SetActive(false);
            phaseFourUI.SetActive(false);
            phaseFiveUI.SetActive(false);
        }

        if (phaseThree == true)
        {
            phase3.TransitionTo(0.0f);

            goingIntoNewPhaseSetToFalse++;
            if (goingIntoNewPhaseSetToFalse == 1)
            {
                goingIntoNewPhase = false; //Mag maar 1 keer gebeuren. (Hij gaat steeds weer op true)
            }
            // -------------------------- TestUI
            phaseOneUI.SetActive(true);
            phaseTwoUI.SetActive(true);
            phaseThreeUI.SetActive(true);
            phaseFourUI.SetActive(false);
            phaseFiveUI.SetActive(false);
        }

        if (phaseFour == true)
        {
            phase4.TransitionTo(0.0f);

            goingIntoNewPhaseSetToFalse++;
            if (goingIntoNewPhaseSetToFalse == 1)
            {
                goingIntoNewPhase = false; //Mag maar 1 keer gebeuren. (Hij gaat steeds weer op true)
            }
            // -------------------------- TestUI
            phaseOneUI.SetActive(true);
            phaseTwoUI.SetActive(true);
            phaseThreeUI.SetActive(true);
            phaseFourUI.SetActive(true);
            phaseFiveUI.SetActive(false);
        }

        if (phaseFive == true)
        {
            phase5.TransitionTo(0.0f);

            goingIntoNewPhaseSetToFalse++;
            if (goingIntoNewPhaseSetToFalse == 1)
            {
                goingIntoNewPhase = false; //Mag maar 1 keer gebeuren. (Hij gaat steeds weer op true)
            }

            // Test
            phaseOneUI.SetActive(true);
            phaseTwoUI.SetActive(true);
            phaseThreeUI.SetActive(true);
            phaseFourUI.SetActive(true);
            phaseFiveUI.SetActive(true);
        }
        // Checked wat de hoogste score in het veld is. (Beat is afhankelijk van de hoogste score)

        //Hier moet de code komen die checked of X aantal spelers een bepaalde score bereikt hebben.
        /*
        PlayerPlanets highest = null;
        foreach (var i in planets)
        {
            if ((highest == null || i.currentScore > highest.currentScore) && i.HasPlayer())
            {
                highest = i;
                highestScore = highest.currentScore;
                //Debug.Log(highestScore);

                //Hier moet een script komen dat kijkt naar de max 3 hoogste scores.
            }
        }
        */
        /*
        // AsteroidSpawning
        if (anticipationPhasePlay == true)
        {
            if (BeatPulse.AudioPhaseSwitcher == true && (BeatPulse.spawnDelayCount - 1) == 2) // == 3de maat
            {

                if (goingIntoNewPhase == false)
                {
                    anticipationPhase.TransitionTo(0.0f);
                    startOpenBlackHole = true;
                    startResourceSpawning = true;
                }
            }
            else
            {
                anticipationPhaseOff.TransitionTo(0.0f);
                startOpenBlackHole = false;
            }

            if (goingIntoNewPhase == true && newPhaseWaitTimer == 1 && (BeatPulse.spawnDelayCount - 1) == 2)
            {
                anticipationNewPhase.TransitionTo(0.0f);
                startOpenBlackHole = true;
                //startResourceSpawning = true;
            }
        }
        */

        /*
        if (highestScore >= 20 && highestScore < 40)
        {
            anticipationPhasePlay = true; //Mag beginnen met opbouwen van de beat.
            if(BeatPulse.spawnDelayCount == omDeVierLoops)
            {
                phaseTwo = true;
                phaseOne = phaseThree = phaseFour = phaseFive = false;
            }
        }

        if (highestScore >= 40 && highestScore < 60)
        {
            anticipationPhasePlay = true; //Mag beginnen met opbouwen van de beat.
            if (BeatPulse.spawnDelayCount == omDeVierLoops)
            {
                phaseThree = true;
                phaseTwo = phaseOne = phaseFour = phaseFive = false;
            }
        }

        if (highestScore >= 60 && highestScore < 80)
        {
            anticipationPhasePlay = true; //Mag beginnen met opbouwen van de beat.
            if (BeatPulse.spawnDelayCount == omDeVierLoops)
            {
                phaseFour = true;
                phaseTwo = phaseThree = phaseOne = phaseFive = false;
            }
        }

        if (highestScore >= 80)
        {
            anticipationPhasePlay = true; //Mag beginnen met opbouwen van de beat.
            if (BeatPulse.spawnDelayCount == omDeVierLoops)
            {
                phaseFive = true;
                phaseTwo = phaseThree = phaseFour = phaseOne = false;
            }
        }
        */
    
    // ************* ------------------ Beatchange when score reached
    //Ervoor zorgen dat de beat wel op de juiste manier opbbouwt. (in stappen van ints)
    //De beat mag wel abrupt afbouwen.

    //Ervoor zorgen dat er vantevoren wordt gekeken of je naar een nieuwe phase gaat. if so, delay nog eens 4 stappen.

    if (highestScore >= 0 && highestScore < 20)
    { 
        // --------------- Check if new in phase
        if (phaseTwoTimer > 0 || phaseThreeTimer > 0 || phaseFourTimer > 0 || phaseFiveTimer > 0)
            newPhase = true;


        phaseOneTimer++;
        if (newPhase == true)
        {
            resetPhase();
        }


        // -------------------------------------

/*         if (BeatPulse.AudioPhaseSwitcher == true && BeatPulse.spawnDelayCount == omDeVierLoops)
        {
            Debug.Log("TestScore");
            phaseOne = true;
            phaseTwo = phaseThree = phaseFour = phaseFive = false;
            // Waarom is phase 1 anders? dan de andere phases
        }
    } */

    if (highestScore >= 20 && highestScore < 40)
    {
            //Hij moet x callen alleen als die in een nieuwe phase komt (dan ga ik de audio van de resource veranderen)
        if (phaseOneTimer > 0 || phaseThreeTimer > 0 || phaseFourTimer > 0 || phaseFiveTimer > 0)
             newPhase = true;

        phaseTwoTimer++;
        if (newPhase == true)
        {
            resetPhase();
        }



        // -------------------------------------

        if (BeatPulse.AudioPhaseSwitcher == true && BeatPulse.spawnDelayCount == omDeVierLoops) //Hij mag dit maar 1 keer doen
        {
            if (goingIntoNewPhase == true && newPhaseWaitTimer == 1)
            {
                phaseTwo = true;
                phaseOne = phaseThree = phaseFour = phaseFive = false;
                goingIntoNewPhase = false;
            }
            goingIntoNewPhase = true;
        }
    } 

    if (highestScore >= 40 && highestScore < 60) 
    {
        if (phaseTwoTimer > 0 || phaseOneTimer > 0 || phaseFourTimer > 0 || phaseFiveTimer > 0)
            newPhase = true;

        phaseThreeTimer++;
        if (newPhase == true)
        {
            resetPhase();
        }

        // -------------------------------------
        if (BeatPulse.AudioPhaseSwitcher == true && BeatPulse.spawnDelayCount == omDeVierLoops)
        {
            if (goingIntoNewPhase == true && newPhaseWaitTimer == 1)
            {
                phaseThree = true;
                phaseTwo = phaseOne = phaseFour = phaseFive = false;
                goingIntoNewPhase = false;
            }
            goingIntoNewPhase = true; //Dit moet ergens anders... 
            Debug.Log("Ik ga niet uit");
        }
    }

    if (highestScore >= 60 && highestScore < 80)
    {
        if (phaseTwoTimer > 0 || phaseThreeTimer > 0 || phaseOneTimer > 0 || phaseFiveTimer > 0)
            newPhase = true;

        phaseFourTimer++;
        if (newPhase == true)
        {
            resetPhase();
        }

        // -------------------------------------
        if (BeatPulse.AudioPhaseSwitcher == true && BeatPulse.spawnDelayCount == omDeVierLoops)
        {
            if (goingIntoNewPhase == true && newPhaseWaitTimer == 1)
            {
                phaseFour = true;
                phaseTwo = phaseOne = phaseOne = phaseFive = false;
                goingIntoNewPhase = false;
            }
            goingIntoNewPhase = true;
        }
    }

    if (highestScore >= 80)
    {
        if (phaseTwoTimer > 0 || phaseThreeTimer > 0 || phaseFourTimer > 0 || phaseOneTimer > 0)
            newPhase = true;

        phaseFiveTimer++;
        if (newPhase == true)
        {
            resetPhase();
        }

        // -------------------------------------
        if (BeatPulse.AudioPhaseSwitcher == true && BeatPulse.spawnDelayCount == omDeVierLoops)
        {
            if (goingIntoNewPhase == true && newPhaseWaitTimer == 1)
            {
                phaseFive = true;
                phaseTwo = phaseOne = phaseFour = phaseOne = false;
                goingIntoNewPhase = false;
            }
            goingIntoNewPhase = true;
        }
    }
}

    public void CheckPhase()
    {
        PlayerPlanets highest = null;
        foreach (var i in planets)
        {
            if ((highest == null || i.currentScore > highest.currentScore) && i.HasPlayer())
            {
                highest = i;
                highestScore = highest.currentScore;
                //Debug.Log(highestScore);

                //Hier moet een script komen dat kijkt naar de max 3 hoogste scores.
            }
        }
    }

    private void resetPhase()
    {
        //Debug.Log("ResetPhase");
        goingIntoNewPhaseSetToFalse = 0;

        phaseOneTimer = 0;
        phaseTwoTimer = 0;
        phaseThreeTimer = 0;
        phaseFourTimer = 0;
        phaseFiveTimer = 0;

        newPhase = false;
        
    }
}