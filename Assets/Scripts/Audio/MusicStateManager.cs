using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class MusicStateManager : MonoBehaviour {
    //[FMODUnity.EventRef]
    [FMODUnity.ParamRef]
    float musicstate;

    public void MusicGameState(float gamestate) {
        musicstate = gamestate;

        RuntimeManager.StudioSystem.setParameterByName("par", musicstate);
        //Debug.Log(musicstate);
    }
}
