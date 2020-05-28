using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetStages : MonoBehaviour {
    public GameObject moonPrefab;
    public MeshRenderer meshRenderer;

    public int currentLightStage = 0;
    public const int lightStageAmount = 5;

    [System.Serializable]
    public class LightStage {
        public int moons = 0;
        public Material material;

        public void ApplyStage(GameObject root, GameObject prefab, int moonAmount) {
            var moonList = root.GetComponentsInChildren<Moon>();
            int moonsToSpawn = moons - moonList.Length;
            if(moonsToSpawn > 0) {
                for(int i = 0; i < moonsToSpawn; i++) {
                    Random.InitState((int)Time.time);
                    var moonOBJ = Instantiate(prefab);
                    moonOBJ.name = "Moon";
                    moonOBJ.transform.SetParent(root.transform);
                    moonOBJ.transform.localPosition = new Vector3(0, 0, -Random.Range(400, 600));
                    moonOBJ.GetComponent<Moon>().Init(Random.Range(20, 50) * ((Random.Range(0, 2) == 1) ? -1f : 1f), (Random.Range(0, 2) == 1), Random.Range(0.25f, 0.7f));
                }
            }
        }
    }

    [Space(5)]
    public LightStage[] lightStages;

    protected LightStage curStage;

    public void SetLightStage(int i) {
        if(i > lightStageAmount - 1) return;
        currentLightStage = i;
        curStage = lightStages[i];
        meshRenderer.material = curStage.material;
        curStage.ApplyStage(gameObject, moonPrefab, lightStages[i].moons);
        var moonList = GetComponentsInChildren<Moon>();
        if(moonList.Length > 0) for(int m = 0; m < (moonList.Length - lightStages[i].moons); m++) DestroyImmediate(moonList[moonList.Length - 1 - m].gameObject);
    }
}
