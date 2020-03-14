using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Planet3D : MonoBehaviour {
    [Header("References")]
    public Material m_GroundMaterial, m_OceanMaterial;
    GameObject m_GroundMesh, m_OceanMesh;

    [Header("TERRAFORMING")]
    public int   m_NumberOfContinents = 5;
    public float m_ContinentSizeMax   = 1.0f;
    public float m_ContinentSizeMin   = 0.1f;

    [HideInInspector]
    public bool settingFoldout;

    [Range(0, 4)]
    public int SUBDIVIDES = 3;

    public int   m_NumberOfHills = 5;
    public float m_HillSizeMax   = 1.0f;
    public float m_HillSizeMin   = 0.1f;

    public Color32 colorOcean = new Color32(  0,  80, 220,   0);
    public Color32 colorGrass = new Color32(  0, 220,   0,   0);
    public Color32 colorDirt  = new Color32(180, 140,  20,   0);
    public Color32 colorDeepOcean = new Color32(  0,  40, 110,   0);

    List<Polygon> m_Polygons;
    List<Vector3> m_Vertices;

    [Header("PHYSICS")]
    public Vector3 planetRotateSpeed = new Vector3(2, 4, 0.7f);
    public float gravity = -12;
    public float orbitSpeed = 4;

    public void Attract(Transform playerTransform) {
        Vector3 gravityUp = (playerTransform.position - transform.position).normalized;
        Vector3 localUp = playerTransform.up;

        playerTransform.GetComponent<Rigidbody>().AddForce(gravityUp * gravity);

        Quaternion targetRotation = Quaternion.FromToRotation(localUp, gravityUp) * playerTransform.rotation;
        playerTransform.rotation = Quaternion.Slerp(playerTransform.rotation, targetRotation, 50f * Time.deltaTime);
    }

    public void Start() {
        foreach(Transform t in transform) if(t.tag == "PlanetTerrain") DestroyImmediate(t.gameObject);
        foreach(Transform t in transform) if(t.tag == "PlanetTerrain") DestroyImmediate(t.gameObject);

        InitAsIcosohedron();
        Subdivide(SUBDIVIDES);
        CalculateNeighbors();

        foreach (Polygon p in m_Polygons) p.m_Color = colorOcean;
        PolySet landPolys = new PolySet();
        PolySet sides;

        for(int i = 0; i < m_NumberOfContinents; i++) {
            float continentSize = Random.Range(m_ContinentSizeMin, m_ContinentSizeMax);
            PolySet newLand = GetPolysInSphere(Random.onUnitSphere, continentSize, m_Polygons);
            landPolys.UnionWith(newLand);
        }

        var oceanPolys = new PolySet();
        foreach (Polygon poly in m_Polygons) {
            if (!landPolys.Contains(poly))
                oceanPolys.Add(poly);
        }

        var oceanSurface = new PolySet(oceanPolys);

        sides = Inset(oceanSurface, 0.05f);
        sides.ApplyColor(colorOcean);
        sides.ApplyAmbientOcclusionTerm(1.0f, 0.0f);

        if (m_OceanMesh != null) DestroyImmediate(m_OceanMesh);
        m_OceanMesh = GenerateMesh("Ocean Surface", m_OceanMaterial);

        foreach (Polygon landPoly in landPolys) landPoly.m_Color = colorGrass;

        sides = Extrude(landPolys, 0.05f);
        sides.ApplyColor(colorDirt);
        sides.ApplyAmbientOcclusionTerm(1.0f, 0.0f);

        PolySet hillPolys = landPolys.RemoveEdges();

        sides = Inset(hillPolys, 0.03f);
        sides.ApplyColor(colorGrass);
        sides.ApplyAmbientOcclusionTerm(0.0f, 1.0f);

        sides = Extrude(hillPolys, 0.05f);
        sides.ApplyColor(colorDirt);

        sides.ApplyAmbientOcclusionTerm(1.0f, 0.0f);

        sides = Extrude(oceanPolys, -0.02f);
        sides.ApplyColor(colorOcean);
        sides.ApplyAmbientOcclusionTerm(0.0f, 1.0f);

        sides = Inset(oceanPolys, 0.02f);
        sides.ApplyColor(colorOcean);
        sides.ApplyAmbientOcclusionTerm(1.0f, 0.0f);

        var deepOceanPolys = oceanPolys.RemoveEdges();
        sides = Extrude(deepOceanPolys, -0.05f);
        sides.ApplyColor(colorDeepOcean);
        deepOceanPolys.ApplyColor(colorDeepOcean);

        if (m_GroundMesh != null) DestroyImmediate(m_GroundMesh);
        m_GroundMesh = GenerateMesh("Ground Mesh", m_GroundMaterial);
    } 

    public void InitAsIcosohedron() {
        m_Polygons = new List<Polygon>();
        m_Vertices = new List<Vector3>();
        float t = (1.0f + Mathf.Sqrt(5.0f)) / 2.0f;

        m_Vertices.Add(new Vector3(-1,  t,  0).normalized);
        m_Vertices.Add(new Vector3( 1,  t,  0).normalized);
        m_Vertices.Add(new Vector3(-1, -t,  0).normalized);
        m_Vertices.Add(new Vector3( 1, -t,  0).normalized);
        m_Vertices.Add(new Vector3( 0, -1,  t).normalized);
        m_Vertices.Add(new Vector3( 0,  1,  t).normalized);
        m_Vertices.Add(new Vector3( 0, -1, -t).normalized);
        m_Vertices.Add(new Vector3( 0,  1, -t).normalized);
        m_Vertices.Add(new Vector3( t,  0, -1).normalized);
        m_Vertices.Add(new Vector3( t,  0,  1).normalized);
        m_Vertices.Add(new Vector3(-t,  0, -1).normalized);
        m_Vertices.Add(new Vector3(-t,  0,  1).normalized);

        m_Polygons.Add(new Polygon( 0, 11,  5));
        m_Polygons.Add(new Polygon( 0,  5,  1));
        m_Polygons.Add(new Polygon( 0,  1,  7));
        m_Polygons.Add(new Polygon( 0,  7, 10));
        m_Polygons.Add(new Polygon( 0, 10, 11));
        m_Polygons.Add(new Polygon( 1,  5,  9));
        m_Polygons.Add(new Polygon( 5, 11,  4));
        m_Polygons.Add(new Polygon(11, 10,  2));
        m_Polygons.Add(new Polygon(10,  7,  6));
        m_Polygons.Add(new Polygon( 7,  1,  8));
        m_Polygons.Add(new Polygon( 3,  9,  4));
        m_Polygons.Add(new Polygon( 3,  4,  2));
        m_Polygons.Add(new Polygon( 3,  2,  6));
        m_Polygons.Add(new Polygon( 3,  6,  8));
        m_Polygons.Add(new Polygon( 3,  8,  9));
        m_Polygons.Add(new Polygon( 4,  9,  5));
        m_Polygons.Add(new Polygon( 2,  4, 11));
        m_Polygons.Add(new Polygon( 6,  2, 10));
        m_Polygons.Add(new Polygon( 8,  6,  7));
        m_Polygons.Add(new Polygon( 9,  8,  1));
    }
    
    public void Subdivide(int recursions) {
        var midPointCache = new Dictionary<int, int>();
        for (int i = 0; i < recursions; i++) {
            var newPolys = new List<Polygon>();
            foreach (var poly in m_Polygons) {
                int a = poly.m_Vertices[0];
                int b = poly.m_Vertices[1];
                int c = poly.m_Vertices[2];

                int ab = GetMidPointIndex(midPointCache, a, b);
                int bc = GetMidPointIndex(midPointCache, b, c);
                int ca = GetMidPointIndex(midPointCache, c, a);

                newPolys.Add(new Polygon(a, ab, ca));
                newPolys.Add(new Polygon(b, bc, ab));
                newPolys.Add(new Polygon(c, ca, bc));
                newPolys.Add(new Polygon(ab, bc, ca));
            }
            m_Polygons = newPolys;
        }
    }

    public int GetMidPointIndex(Dictionary<int, int> cache, int indexA, int indexB) {
        int smallerIndex = Mathf.Min(indexA, indexB);
        int greaterIndex = Mathf.Max(indexA, indexB);
        int key = (smallerIndex << 16) + greaterIndex;

        int ret;
        if (cache.TryGetValue(key, out ret)) return ret;

        Vector3 p1 = m_Vertices[indexA];
        Vector3 p2 = m_Vertices[indexB];
        Vector3 middle = Vector3.Lerp(p1, p2, 0.5f).normalized;

        ret = m_Vertices.Count;
        m_Vertices.Add(middle);

        cache.Add(key, ret);
        return ret;
    }

    public void CalculateNeighbors() {
        foreach (Polygon poly in m_Polygons) {
            foreach (Polygon other_poly in m_Polygons) {
                if (poly == other_poly) continue;
                if (poly.IsNeighborOf(other_poly)) poly.m_Neighbors.Add(other_poly);
            }
        }
    }

    public List<int> CloneVertices(List<int> old_verts) {
        List<int> new_verts = new List<int>();
        foreach (int old_vert in old_verts) {
            Vector3 cloned_vert = m_Vertices[old_vert];
            new_verts.Add(m_Vertices.Count);
            m_Vertices.Add(cloned_vert);
        }
        return new_verts;
    }

    public PolySet StitchPolys(PolySet polys) {
        PolySet stitchedPolys = new PolySet();

        var edgeSet = polys.CreateEdgeSet();
        var originalVerts = edgeSet.GetUniqueVertices();
        var newVerts = CloneVertices(originalVerts);

        edgeSet.Split(originalVerts, newVerts);

        foreach (Edge edge in edgeSet) {
            var stitch_poly1 = new Polygon(edge.m_OuterVerts[0],
                                           edge.m_OuterVerts[1],
                                           edge.m_InnerVerts[0]);
            var stitch_poly2 = new Polygon(edge.m_OuterVerts[1],
                                           edge.m_InnerVerts[1],
                                           edge.m_InnerVerts[0]);
            edge.m_InnerPoly.ReplaceNeighbor(edge.m_OuterPoly, stitch_poly2);
            edge.m_OuterPoly.ReplaceNeighbor(edge.m_InnerPoly, stitch_poly1);

            m_Polygons.Add(stitch_poly1);
            m_Polygons.Add(stitch_poly2);
            stitchedPolys.Add(stitch_poly1);
            stitchedPolys.Add(stitch_poly2);
        }

        foreach (Polygon poly in polys) {
            for (int i = 0; i < 3; i++) {
                int vert_id = poly.m_Vertices[i];
                if (!originalVerts.Contains(vert_id)) continue;
                int vert_index = originalVerts.IndexOf(vert_id);
                poly.m_Vertices[i] = newVerts[vert_index];
            }
        }
        return stitchedPolys;
    }

    public PolySet Extrude(PolySet polys, float height) {
        PolySet stitchedPolys = StitchPolys(polys);
        List<int> verts = polys.GetUniqueVertices();

        foreach (int vert in verts) {
            Vector3 v = m_Vertices[vert];
            v = v.normalized * (v.magnitude + height);
            m_Vertices[vert] = v;
        }
        return stitchedPolys;
    }

    public PolySet Inset(PolySet polys, float interpolation) {
        PolySet stitchedPolys = StitchPolys(polys);
        List<int> verts = polys.GetUniqueVertices();

        Vector3 center = Vector3.zero;
        foreach (int vert in verts) center += m_Vertices[vert];
        center /= verts.Count;

        foreach (int vert in verts) {
            Vector3 v = m_Vertices[vert];
            float height = v.magnitude;
            v = Vector3.Lerp(v, center, interpolation);
            v = v.normalized * height;
            m_Vertices[vert] = v;
        }
        return stitchedPolys;
    }

    public PolySet GetPolysInSphere(Vector3 center, float radius, IEnumerable<Polygon> source) {
        PolySet newSet = new PolySet();
        foreach(Polygon p in source) {
            foreach(int vertexIndex in p.m_Vertices) {
                float distanceToSphere = Vector3.Distance(center, m_Vertices[vertexIndex]);
                if (distanceToSphere <= radius) {
                    newSet.Add(p);
                    break;
                }
            }
        }
        return newSet;
    }

    public GameObject GenerateMesh(string name, Material material) {
        var meshObject = new GameObject(name);
        meshObject.tag = "PlanetTerrain";
        meshObject.transform.parent = transform;

        MeshRenderer surfaceRenderer = meshObject.AddComponent<MeshRenderer>();
        surfaceRenderer.material     = material;

        Mesh terrainMesh = new Mesh();

        int vertexCount = m_Polygons.Count * 3;

        int[] indices = new int[vertexCount];

        Vector3[] vertices = new Vector3[vertexCount];
        Vector3[] normals  = new Vector3[vertexCount];
        Color32[] colors   = new Color32[vertexCount];
        Vector2[] uvs      = new Vector2[vertexCount];

        for (int i = 0; i < m_Polygons.Count; i++)
        {
            var poly = m_Polygons[i];

            indices[i * 3 + 0] = i * 3 + 0;
            indices[i * 3 + 1] = i * 3 + 1;
            indices[i * 3 + 2] = i * 3 + 2;

            vertices[i * 3 + 0] = m_Vertices[poly.m_Vertices[0]];
            vertices[i * 3 + 1] = m_Vertices[poly.m_Vertices[1]];
            vertices[i * 3 + 2] = m_Vertices[poly.m_Vertices[2]];

            uvs[i * 3 + 0] = poly.m_UVs[0];
            uvs[i * 3 + 1] = poly.m_UVs[1];
            uvs[i * 3 + 2] = poly.m_UVs[2];

            colors[i * 3 + 0] = poly.m_Color;
            colors[i * 3 + 1] = poly.m_Color;
            colors[i * 3 + 2] = poly.m_Color;

            if(poly.m_SmoothNormals)
            {
                normals[i * 3 + 0] = m_Vertices[poly.m_Vertices[0]].normalized;
                normals[i * 3 + 1] = m_Vertices[poly.m_Vertices[1]].normalized;
                normals[i * 3 + 2] = m_Vertices[poly.m_Vertices[2]].normalized;
            }
            else
            {
                Vector3 ab = m_Vertices[poly.m_Vertices[1]] - m_Vertices[poly.m_Vertices[0]];
                Vector3 ac = m_Vertices[poly.m_Vertices[2]] - m_Vertices[poly.m_Vertices[0]];

                Vector3 normal = Vector3.Cross(ab, ac).normalized;

                normals[i * 3 + 0] = normal;
                normals[i * 3 + 1] = normal;
                normals[i * 3 + 2] = normal;
            }
        }

        terrainMesh.vertices = vertices;
        terrainMesh.normals  = normals;
        terrainMesh.colors32 = colors;
        terrainMesh.uv       = uvs;

        terrainMesh.SetTriangles(indices, 0);

        MeshFilter terrainFilter = meshObject.AddComponent<MeshFilter>();
        terrainFilter.mesh = terrainMesh;

        meshObject.transform.localScale = Vector3.one * 15f;

        var collider = meshObject.AddComponent<MeshCollider>();
        collider.sharedMesh = terrainMesh;

        meshObject.transform.position = new Vector3(5, 2.5f, 0);

        return meshObject;
    }

    void Update() {
        transform.Rotate(new Vector3(Time.deltaTime * planetRotateSpeed.x, -Time.deltaTime * planetRotateSpeed.y, Time.deltaTime * planetRotateSpeed.z));
    }
}
