using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Planet3D))]
public class PlanetEditor : Editor {
    public override void OnInspectorGUI() {
		var tar = target as Planet3D;

		DrawDefaultInspector();

		if(GUILayout.Button("Generate")) tar.Start();

		EditorGUILayout.Space();
	}
}
