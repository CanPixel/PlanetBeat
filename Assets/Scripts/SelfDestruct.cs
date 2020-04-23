using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
public float liveTime = 5.0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        liveTime -= Time.deltaTime;
        if (liveTime <= 0)
        {
             Destroy(this.gameObject);
        }
    }
}
