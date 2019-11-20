
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace HexMap {
	[CustomEditor(typeof(HexMap))]
	//[CanEditMultipleObjects]
	public class HexMapEditor : Editor {

		HexMap hexMap;

		public override void OnInspectorGUI() {
			DrawDefaultInspector();

			hexMap = (HexMap)target;

			if (GUILayout.Button("Generate")) {

				if (!Application.isPlaying) {

					hexMap.GenerateStartArea();
					UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
				}

			}
			

			//if (GUILayout.Button("Fill Tile Table")) {
			//	hexMap.FillHexTileTable();
			//	UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();

			//}
			
		}
	}
}