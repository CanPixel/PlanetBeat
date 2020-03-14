using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Polygon {
    public List<int>     m_Vertices;
    public List<Vector2> m_UVs;
    public List<Polygon> m_Neighbors;
    public Color32       m_Color;
    public bool          m_SmoothNormals;

    public Polygon(int a, int b, int c) {
        m_Vertices      = new List<int>() { a, b, c };
        m_Neighbors     = new List<Polygon>();
        m_UVs = new List<Vector2>() {Vector2.zero, Vector2.zero, Vector2.zero};
        m_SmoothNormals = true;
        m_Color = new Color32(255, 0, 255, 255);
    }

    public bool IsNeighborOf(Polygon other_poly) {
        int shared_vertices = 0;
        foreach (int vertex in m_Vertices) {
            if (other_poly.m_Vertices.Contains(vertex)) shared_vertices++;
        }
        return shared_vertices == 2;
    }

    public void ReplaceNeighbor(Polygon oldNeighbor, Polygon newNeighbor) {
        for(int i = 0; i < m_Neighbors.Count; i++) {
            if(oldNeighbor == m_Neighbors[i]) {
                m_Neighbors[i] = newNeighbor;
                return;
            }
        }
    }
}

public class PolySet : HashSet<Polygon> {
    public PolySet() {}
    public PolySet(PolySet source) : base(source) {}

    public int m_StitchedVertexThreshold = -1;

    public EdgeSet CreateEdgeSet() {
        EdgeSet edgeSet = new EdgeSet();
        foreach (Polygon poly in this) {
            foreach (Polygon neighbor in poly.m_Neighbors) {
                if (this.Contains(neighbor)) continue;
                Edge edge = new Edge(poly, neighbor);
                edgeSet.Add(edge);
            }
        }
        return edgeSet;
    }

    public PolySet RemoveEdges() {
        var newSet = new PolySet();
        var edgeSet = CreateEdgeSet();
        var edgeVertices = edgeSet.GetUniqueVertices();
        foreach(Polygon poly in this) {
            bool polyTouchEdge = false;
            for(int i = 0; i < 3; i++) {
                if(edgeVertices.Contains(poly.m_Vertices[i])) {
                    polyTouchEdge = true;
                    break;
                }
            }
            if(polyTouchEdge) continue;
            newSet.Add(poly);
        }
        return newSet;
    }

    public List<int> GetUniqueVertices() {
        List<int> verts = new List<int>();
        foreach (Polygon poly in this) {
            foreach (int vert in poly.m_Vertices) {
                if (!verts.Contains(vert)) verts.Add(vert);
            }
        }
        return verts;
    }

    public void ApplyAmbientOcclusionTerm(float AOOriginal, float AONew) {
        foreach(var poly in this) {
            for(int i = 0; i < 3; i++) {
                float aoTerm = (poly.m_Vertices[i] > m_StitchedVertexThreshold) ? AONew : AOOriginal;
                Vector2 uv = poly.m_UVs[i];
                uv.y = aoTerm;
                poly.m_UVs[i] = uv;
            }
        }
    }

    public void ApplyColor(Color32 c) {
        foreach(Polygon poly in this) poly.m_Color = c;
    }
}
