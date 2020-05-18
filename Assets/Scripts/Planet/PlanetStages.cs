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
        public float lightIntensity = 0f;
        public int moons = 0;
        public Material material;

        public void ApplyStage(GameObject root, GameObject prefab) {
            var moonList = root.GetComponentsInChildren<Moon>();
            int moonsToSpawn = moons - moonList.Length;

            if(moonsToSpawn > 0) {
                for(int i = 0; i < moonsToSpawn; i++) {
                    var moonOBJ = Instantiate(prefab);
                    moonOBJ.name = "Moon";
                    moonOBJ.transform.SetParent(root.transform);
                    moonOBJ.transform.localScale = Vector3.one * 100f;
                    moonOBJ.transform.localPosition = new Vector3(0, 0, -200 * (i + 1));
                }
            }
        }
    }    

    [Space(5)]
    public LightStage[] lightStages;

    protected LightStage curStage;

    public void SetLightStage(int i) {
        currentLightStage = i;
        curStage = lightStages[i];
        meshRenderer.material = curStage.material;
        curStage.ApplyStage(gameObject, moonPrefab);
    }
}
