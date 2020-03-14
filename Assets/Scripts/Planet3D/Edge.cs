using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edge {
    public Polygon m_InnerPoly; 
    public Polygon m_OuterPoly;
    public List<int> m_OuterVerts; 
    public List<int> m_InnerVerts;
    public int m_InwardDirectionVertex;

    public Edge(Polygon inner_poly, Polygon outer_poly) {
        m_InnerPoly  = inner_poly;
        m_OuterPoly  = outer_poly;
        m_OuterVerts = new List<int>(2);
        m_InnerVerts = new List<int>(2);

        foreach (int vertex in inner_poly.m_Vertices) {
            if(outer_poly.m_Vertices.Contains(vertex)) m_InnerVerts.Add(vertex);
            else m_InwardDirectionVertex = vertex;
        }

        if(m_InnerVerts[0] == inner_poly.m_Vertices[0] && m_InnerVerts[1] == inner_poly.m_Vertices[2]) {
            int temp = m_InnerVerts[0];
            m_InnerVerts[0] = m_InnerVerts[1];
            m_InnerVerts[1] = temp;
        }
        m_OuterVerts = new List<int>(m_InnerVerts);
    }
}

public class EdgeSet : HashSet<Edge> {
    public Dictionary<int, Vector3> GetInwardDirections(List<Vector3> vertexPos) {
        var inwardDirections = new Dictionary<int, Vector3>();
        var numContributions = new Dictionary<int, int>();

        foreach(Edge edge in this) {
            Vector3 innerVertexPosition = vertexPos[edge.m_InwardDirectionVertex];
            
            Vector3 edgePosA = vertexPos[edge.m_InnerVerts[0]];
            Vector3 edgePosB = vertexPos[edge.m_InnerVerts[1]];
            Vector3 edgeCenter = Vector3.Lerp(edgePosA, edgePosB, 0.5f);
            Vector3 innerVector = (innerVertexPosition - edgeCenter).normalized;

            for(int i = 0; i < 2; i++) {
                int edgeVertex = edge.m_InnerVerts[i];
                if(inwardDirections.ContainsKey(edgeVertex)) {
                    inwardDirections[edgeVertex] += innerVector;
                    numContributions[edgeVertex]++;
                } else {
                    inwardDirections.Add(edgeVertex, innerVector);
                    numContributions.Add(edgeVertex, 1);
                }
            }
        }

        foreach(KeyValuePair<int, int> kvp in numContributions) {
            int vertexIndex = kvp.Key;
            int contributionsToVertex = kvp.Value;
            inwardDirections[vertexIndex] = (inwardDirections[vertexIndex] / contributionsToVertex).normalized;
        }
        return inwardDirections;
    }

    public void Split(List<int> oldVertices, List<int> newVertices) {
        foreach(Edge edge in this) {
            for(int i = 0; i < 2; i++) edge.m_InnerVerts[i] = newVertices[oldVertices.IndexOf(edge.m_OuterVerts[i])];
        }
    }

    public List<int> GetUniqueVertices() {
        List<int> vertices = new List<int>();
        foreach (Edge edge in this)
            foreach (int vert in edge.m_OuterVerts) {
                if (!vertices.Contains(vert)) vertices.Add(vert);
            }
        return vertices;
    }
}