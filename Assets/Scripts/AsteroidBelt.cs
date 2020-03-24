using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AsteroidBelt : MonoBehaviour
{

    public int numObjects = 10;

    public GameObject[] AsteroidsList;

    public GameObject prefab;
    private GameObject InstancedPrefab;

    public GameObject Parent;
    //public GameObject sun;

    public float thrust = 2000f;

    void Start()
    {
        
        Vector3 center = transform.position;
        Rigidbody2D prefabRigidbody = prefab.GetComponent<Rigidbody2D>();
        
        for (int i = 0; i < numObjects; i++)
        {
            Vector3 pos = RandomCircle(center, Random.Range(8f, 9f), Random.Range(0,360));
            Quaternion rot = Quaternion.FromToRotation(Vector2.up, center + pos);

            InstancedPrefab = Instantiate(prefab, pos, rot) as GameObject;
            InstancedPrefab.transform.parent = Parent.transform;
        }

        
    }

    void Update()
    {
        AsteroidsList = GameObject.FindGameObjectsWithTag("Resource");

        Debug.Log(AsteroidsList.Length);
        Vector3 center = transform.position;

        if (AsteroidsList.Length == 4)
        {

            Vector3 pos = RandomCircle(center, Random.Range(8f, 9f), Random.Range(0, 360));
            Quaternion rot = Quaternion.FromToRotation(Vector2.up, center + pos);

            InstancedPrefab = Instantiate(prefab, pos, rot) as GameObject;
            InstancedPrefab.transform.parent = Parent.transform;
        }
    }

    Vector3 RandomCircle(Vector3 center, float radius, int a)
    {
        Debug.Log(a);
        float ang = a;
        Vector3 pos;
        pos.x = center.x + radius * Mathf.Sin(ang * Mathf.Deg2Rad);
        pos.y = center.y + radius * Mathf.Cos(ang * Mathf.Deg2Rad);
        pos.z = center.z;
        return pos;
    }
}
