using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Script_FollowPlanet : MonoBehaviour
{
    public GameObject planetRed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.position = planetRed.transform.position;
    }
}
