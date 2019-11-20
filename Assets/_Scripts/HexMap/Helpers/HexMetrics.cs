using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexMap {
	/// <summary>
	/// Data holder for a HexMap, containing information useful for mesh generation and map functions.
	/// Also encapsulates useful functions for height and UV coords.
	/// </summary>
	[Serializable]
	public class HexMetrics {
		public int radius = 3;
		public float mapScale = 1.0f;
		public float mapHeight = 10f;
		public float elevationStepHeight = 10f;
		public float tileSize = 20f;
		public float tileInnerRadiusPercent = 0.6f;
		public float tileOuterRadiusPercent = 0.7f;
		public float outerBridgePercent = 0.8f;
		//public float wallHeight = 10f;

		public int maxElevation = 10;
		public float elevationNoiseMapResolution = 0.01f;

		public float textureScale = 1.0f;

		[Tooltip("Use to perturb the terrain.")]
		public Texture2D noiseMap;

		[Tooltip("Use to create hills and valleys.")]
		public Texture2D elevationNoiseMap;

		public float XZPositionToHeight(float x, float z, bool scaleByMapHeight = false) {
			Vector2 mapPoint = new Vector2(x, z) / tileSize * mapScale;
			return HexUtils.GetHeightOnNoiseMap(noiseMap, mapPoint) *
				(scaleByMapHeight ? mapHeight : 1);
		}

		public float XZPositionToHeight(Vector3 position, bool scaleByMapHeight = false) {
			return XZPositionToHeight(position.x, position.z, scaleByMapHeight);
		}

		public Vector2 XZPositionToUV(Vector3 position) {
			return new Vector2(position.x, position.z) / tileSize * textureScale;
		}
	}
}