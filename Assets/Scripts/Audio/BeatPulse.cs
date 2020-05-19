using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.Audio;

public class BeatPulse : MonoBehaviour {
    public float _bpm;
    float _beatTime, _beatTimeD8;
    public static bool _beatFull, _beatD8;
    float _beatTimer, _beatTimerD8;
    int _beatCount, _beatCountD;
    public static int _beatCountX2, _beatCountX4, _beatCountX8, _beatCountX16, _beatCountD2, _beatCountD4;

    public static bool BEGIN = false;

    // ------------ Bradley

    public static bool oneTime = false;
    public static bool AudioPhaseSwitcher = false;
    public static bool beatPulseBegin = false;

    private int oneTimeInt = 0;
    private int AudiophaseDelayInt = 0;

    public static int spawnDelayCount = 0;

    // ------------

    public UnityEvent PulseEvent;

    public UnityEvent PulseMaat1;
    public UnityEvent PulseMaat2;
    public UnityEvent PulseMaat3;
    public UnityEvent PulseMaat4;


    private float PulseDelay;
    public static int PulseCount = 1;
    private int PulseCountGlobal = 1;

    IEnumerator beatLength()
    {
        while (true) 
        {
            yield return new WaitForSeconds(3.692313f); //1 loop van 16tellen op 130bpm

            PulseEvent.Invoke();

            //AudioPhaseSwitcher = true; //om de 4 tellen // 
            //spawnDelayCount++;

            // 1 tel = 1 noot
            // 4 tellen = 1 maat
            // 4 maten = 1 loop

            // Bij maat 1, bouwt de beat op + Blackhole opens up
            // Bij maat 2, spawned de asteroid + Blackhole closes
            // Bij maat 3
            // Bij maat 4
        }
    }

    void Start ()
    {
        StartCoroutine("beatLength"); //Wordt 1keertje gecalled
    }

    public void loopEvent()
    {
        PulseCountGlobal++;
        PulseCount++;

        if (PulseCount >= 5)
        {
            PulseCount = 1;
        }

        PulseDelay = 3.5f;

        if (PulseCountGlobal > 4)
        {
            Debug.Log("PulseCount: " + PulseCount); // per loop hoor je 1 keer klik

            if (PulseCount == 1) PulseMaat1.Invoke();
            else if (PulseCount == 2) PulseMaat2.Invoke();
            else if (PulseCount == 3) PulseMaat3.Invoke();
            else if (PulseCount == 4) PulseMaat4.Invoke();
        }   

        //AudioPhaseSwitcher = true;
    }

    
	void Update ()
    {
        //Debug.Log(AudioPhaseSwitcher);

        AudioPhaseSwitcher = PulseDelay > 0;
        if (PulseDelay > 0)
            PulseDelay -= Time.deltaTime; 


        //AudioPhaseSwitcher = false;

        /*
        Debug.Log("AudioPhaseSwitcher " + AudioPhaseSwitcher);
        //Debug.Log("AudiophaseDelayInt " + AudiophaseDelayInt);

        
        if (AudioPhaseSwitcher == true)
            AudiophaseDelayInt++; //Is dit een oneTime script??

        if(AudiophaseDelayInt == 2)
        {
            AudioPhaseSwitcher = false;
        }
            
        */

        /*

        if (beatPulseBegin == true)
        {
            if(oneTimeInt == 0)
            {
                
            }
            oneTimeInt++;
        }

        */
    }
}