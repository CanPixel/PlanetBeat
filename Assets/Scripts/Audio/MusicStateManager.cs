using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class MusicStateManager : MonoBehaviour {
    [FMODUnity.ParamRef]
    float musicstate;

    private static MusicStateManager self;

    void Start() {
        if(self != null) Destroy(gameObject);

        self = this;
        DontDestroyOnLoad(gameObject);
    }

    public static void MusicGameState(float gamestate) {
        if(self == null) return;
        self.musicstate = gamestate;
        RuntimeManager.StudioSystem.setParameterByName("par", self.musicstate);
    }
}
