using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;

public class AsteroidBelt : MonoBehaviour {
    public int asteroidAmount = 4;

    private GameObject[] AsteroidsList;
    public GameObject prefab;

    void Update() {
        if(!GameManager.GAME_STARTED) return;

        AsteroidsList = GameObject.FindGameObjectsWithTag("Resource");
        if (AsteroidsList.Length < asteroidAmount) SpawnAsteroid();
    }

    protected void SpawnAsteroid() {
        Vector3 center = transform.position;
        Vector3 pos = RandomCircle(center, Random.Range(8f, 9f), Random.Range(0, 360));
        Quaternion rot = Quaternion.FromToRotation(Vector2.up, center + pos);
        
        GameObject InstancedPrefab = GameManager.SPAWN_SERVER_OBJECT(prefab, pos, rot);
        if(InstancedPrefab != null) InstancedPrefab.transform.SetParent(transform);
    }

    Vector3 RandomCircle(Vector3 center, float radius, int a) {
        float ang = a;
        return new Vector3(center.x + radius * Mathf.Sin(ang * Mathf.Deg2Rad), center.y + radius * Mathf.Cos(ang * Mathf.Deg2Rad), center.z);
    }
}
