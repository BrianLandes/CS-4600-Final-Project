using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(OrbitalCameraController))]
public class OrbitalCameraEditor : Editor {

	OrbitalCameraController target;

	public override void OnInspectorGUI() {
		DrawDefaultInspector();

		target = (OrbitalCameraController)base.target;

		if (GUILayout.Button("Reposition")) {
			target.CalculatePosition();
			target.Reposition();
			UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();

		}
	}
}
