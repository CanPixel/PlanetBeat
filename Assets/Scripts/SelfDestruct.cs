using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
public float liveTime = 5.0f;
    // Start is called before the first frame update
    void OnEnable()
    {
        transform.SetParent(GameObject.Find("Canvas").transform, true);
    }

    // Update is called once per frame
    void Update()
    {
        liveTime -= Time.deltaTime;
        if (liveTime <= 0)
        {
             GameManager.DESTROY_SERVER_OBJECT(this.gameObject);
             Destroy(this.gameObject);
        }
    }
}
