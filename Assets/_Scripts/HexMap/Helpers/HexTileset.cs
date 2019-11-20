using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexMap {

	[Serializable]
	public class HexTileset {

		public Color steepSlopeColor = Color.black;

		public Color[] elevationColors;
		
		public Color GetColorForElevation(int elevation, int maxElevation) {
			int e = Mathf.FloorToInt((float)elevation / (float)maxElevation * (float)elevationColors.Length);

			if (e < 0 ) {
				return elevationColors[0];
			}
			if ( e >= elevationColors.Length) {
				return elevationColors[elevationColors.Length - 1];
			}

			return elevationColors[e];
		}
	}
}