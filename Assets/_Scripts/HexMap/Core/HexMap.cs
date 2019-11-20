
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HexMap {
	[Serializable]
	public class HexWallTableBool : HexWallTable<bool> { }

	[RequireComponent(typeof(HexagonMaker))]
	public class HexMap : MonoBehaviour {

		public HexTileset tileset;

		public int generateDistance = 15;
		public int destroyDistance = 8;

		//public bool keepPlayerAbove = true;

		//public int spawnWallSteps = 8;

		public GameObject targetObject;

		public GameObject TargetObject { get { return targetObject;  } }

		private HexTable<HexTile> tileTable = new HexTable<HexTile>();

		private HexagonMaker maker;

		[SerializeField]
		private HexTable<int> elevationTable = new HexTable<int>();
		
		private Vector2Int lastPlayerCoords;
		private Vector2Int playerCoords;

		private int topRow = 0;
		private int bottomRow = 0;
		private int minColumn = 0;
		private int maxColumn = 0;
		private int minZ = 0;
		private int maxZ = 0;

		//private int steps = 0;

		//private bool isBeingDestroyed = false;

		public HexMetrics Metrics {
			get {
				maker = GetComponent<HexagonMaker>();
				return maker.metrics;
			}
		}

		
		// Use this for initialization
		void Start() {
			maker = GetComponent<HexagonMaker>();
			if (targetObject == null) {
				targetObject = GameObject.FindGameObjectWithTag("Player");
			}

			FillHexTileTable();
			
		}

		public void FillHexTileTable() {
			tileTable.Clear();

			// Something about how it serializes doesn't keep the tileTable in the HexagonMaker stable between edit mode and play mode
			// So let's waste a compounding amount of effort recursivally rebuilding it on startup
			foreach (var tile in GetComponentsInChildren<HexTile>()) {
				tile.map = this;
				tileTable.Set(tile.column, tile.row, tile);
				topRow = topRow > tile.row ? topRow : tile.row;
				bottomRow = bottomRow < tile.row ? bottomRow : tile.row;
				maxColumn = maxColumn > tile.column ? maxColumn : tile.column;
				minColumn = minColumn < tile.column ? minColumn : tile.column;
				int z = HexUtils.Z(tile.column, tile.row);
				maxZ = maxZ > z ? maxZ : z;
				minZ = minZ < z ? minZ : z;
			}
		}
		
		public void GenerateStartArea() {
			maker = GetComponent<HexagonMaker>();
			maker.ClearAllTiles();
			
			maker.GenerateTilesAt(0,0,Metrics.radius);
		}
		
		// Update is called once per frame
		void Update() {

			

			//if (keepPlayerAbove ) {
			//	HexNavMeshManager.EnsureAboveMap(targetObject.transform);
			//}

			playerCoords = maker.WorldPositionToAxialCoords(targetObject.transform.position);

			//Debug.Log(playerCoords);
			
			if (HexUtils.Distance(lastPlayerCoords, playerCoords) > 0) {
				//int x = playerCoords.x - lastPlayerCoords.x;
				//int y = playerCoords.y - lastPlayerCoords.y;
				//// TODO: if the player teleports this function doesn't know what to do
				//HexDirection direction = HexUtils.VectorToDirection(x, y);

				int z = HexUtils.Z(playerCoords.x, playerCoords.y);
				//switch (direction) {
				//	case HexDirection.E:
				RemoveMinColumns();
				AddMaxColumns();

				RemoveMaxZs(z);
				AddMinZs(z);

				//		break;
				//	case HexDirection.NE:
				RemoveMinRows();
				AddMaxRows();

				//		RemoveMaxZs(z);
				//		AddMinZs(z);
				//		break;
				//	case HexDirection.NW:
				//		RemoveMinRows();
				//		AddMaxRows();

				RemoveMaxColumns();
				AddMinColumns();

				//		break;
				//	case HexDirection.W:
				//		RemoveMaxColumns();
				//		AddMinColumns();

				RemoveMinZs(z);
				AddMaxZs(z);

				//		break;
				//	case HexDirection.SW:
				RemoveMaxRows();
				AddMinRows();

				//		RemoveMinZs(z);
				//		AddMaxZs(z);
				//		break;
				//	case HexDirection.SE:
				//		RemoveMinColumns();
				//		AddMaxColumns();
				//		RemoveMaxRows();
				//		AddMinRows();
				//		break;
				//}





			}
			
			lastPlayerCoords = playerCoords;
		}

		private void AddRow(int row ) {
			for( int c = minColumn - 1; c <= maxColumn+1; c ++ ) {
				NewTileMaybe(c, row);
				//pendingTileModifications.Add(new Vector2Int(c, row));
			}
		}

		private void AddColumn(int column) {
			for (int r = bottomRow-1; r <= topRow + 1; r++) {
				NewTileMaybe(column, r);
				//pendingTileModifications.Add(new Vector2Int(column, r));
			}
		}

		private void AddZ(int z) {
			for (int r = bottomRow - 1; r <= topRow + 1; r++) {
				NewTileMaybe(-r - z, r);
				//pendingTileModifications.Add(new Vector2Int(-r - z, r));
			}
		}

		private void NewTileMaybe(int column, int row) {
			if ( !tileTable.Contains(column, row) && HexUtils.Distance(column, row, playerCoords.x, playerCoords.y) <= generateDistance + 1) {
				HexTile hexTile = maker.GenerateTile( column, row );
				tileTable.Set(column, row, hexTile);
			}
		}

		private void RemoveMinColumns() {
			while (playerCoords.x - minColumn > destroyDistance) {
				foreach( var tile in tileTable.RemoveColumn(minColumn) ) {
					maker.ReturnTileToPool(tile);
					//Destroy(tile.gameObject);
				}
				minColumn++;
			}
		}

		private void RemoveMaxColumns() {
			while (maxColumn - playerCoords.x > destroyDistance) {
				foreach (var tile in tileTable.RemoveColumn(maxColumn)) {
					maker.ReturnTileToPool(tile);
					//Destroy(tile.gameObject);
				}
				maxColumn--;
			}
		}

		private void RemoveMinRows() {
			while (playerCoords.y - bottomRow > destroyDistance) {
				foreach (var tile in tileTable.RemoveRow(bottomRow)) {
					maker.ReturnTileToPool(tile);
					//Destroy(tile.gameObject);
				}
				bottomRow++;
			}
		}

		private void RemoveMaxRows() {
			while (topRow - playerCoords.y > destroyDistance) {
				foreach (var tile in tileTable.RemoveRow(topRow)) {
					maker.ReturnTileToPool(tile);
					//Destroy(tile.gameObject);
				}
				topRow--;
			}
		}

		private void RemoveMinZs(int z) {
			while (z - minZ > destroyDistance) {
				foreach (var tile in tileTable.RemoveZ(minZ)) {
					maker.ReturnTileToPool(tile);
					//Destroy(tile.gameObject);
				}
				minZ++;
			}
		}

		private void RemoveMaxZs(int z) {
			while (maxZ - z > destroyDistance) {
				foreach (var tile in tileTable.RemoveZ(maxZ)) {
					maker.ReturnTileToPool(tile);
					//Destroy(tile.gameObject);
				}
				maxZ--;
			}
		}


		private void AddMinColumns() {
			while (playerCoords.x - minColumn < generateDistance) {
				AddColumn(--minColumn);
			}
		}

		private void AddMaxColumns() {
			while (maxColumn - playerCoords.x < generateDistance) {
				AddColumn(++maxColumn);
			}
		}

		private void AddMinRows() {
			while (playerCoords.y - bottomRow < generateDistance) {
				AddRow(--bottomRow);
			}
		}

		private void AddMaxRows() {
			while (topRow - playerCoords.y < generateDistance) {
				AddRow(++topRow);
			}
		}

		private void AddMinZs(int z) {
			while (z - minZ < generateDistance) {
				AddZ(--minZ);
			}
		}
		
		private void AddMaxZs(int z) {
			while (maxZ - z < generateDistance) {
				AddZ(++maxZ);
			}
		}


		#region Map Properties Methods

		public bool IsWallAt(int column, int row, HexDirection direction) {
			int elevationA = GetElevationAt(column, row);

			int elevationB = GetElevationAt(HexUtils.MoveFrom(column, row, direction));

			int difference = Mathf.Abs(elevationB - elevationA);

			return difference > 2;

			//if (isWallTable == null) {
			//	return false;
			//}
			//return isWallTable.Get(column, row, direction);
		}

		public bool IsWallAt(Vector2Int pos, HexDirection direction) {
			return (IsWallAt(pos.x, pos.y, direction));
		}

		public int GetElevationAt(Vector2Int pos) {
			return GetElevationAt(pos.x, pos.y);
		}

		public int GetElevationAt( int column, int row ) {
			int elevation;
			if (elevationTable.TryGet(column,row, out elevation ) ) {
				return elevation;
			}
			var point = HexUtils.PositionFromCoordinates(column, row, 1f);
			float noise = HexUtils.GetHeightOnNoiseMap(Metrics.elevationNoiseMap,
				point.JustXZ() * Metrics.elevationNoiseMapResolution );
			elevation = Mathf.FloorToInt( noise * Metrics.maxElevation );
			//elevation = UnityEngine.Random.Range(0, Metrics.maxElevation );
			elevationTable.Set(column, row, elevation);
			return elevation;
		}

		public Vector3 AxialCoordsToWorldPosition(Vector2Int pos) {
			return HexUtils.PositionFromCoordinates(pos.x, pos.y, maker.metrics.tileSize);
		}

		public Vector3 AxialCoordsToWorldPositionWithHeight(Vector2Int pos) {
			Vector3 result = HexUtils.PositionFromCoordinates(pos.x, pos.y, maker.metrics.tileSize);
			return result + Vector3.up * maker.metrics.XZPositionToHeight(result,true);
		}

		public float XZPositionToHeight(Vector3 position, bool scaleByMapHeight = false) {
			return maker.metrics.XZPositionToHeight(position, scaleByMapHeight);
		}

		public HexTile GetTile( int column, int row ) {
			return tileTable.Get(column, row);
		}

		public HexTile GetTile( Vector2Int pos ) {
			return tileTable.Get(pos);
		}

		/// <summary>
		/// Takes the world position and returns the tile's axial coordinates that contains this position
		/// relative to the map's tile size and scale.
		/// </summary>
		/// <param name="position"></param>
		/// <returns></returns>
		public Vector2Int WorldPositionToAxialCoords(Vector3 position) {
			position = transform.InverseTransformPoint(position);
			return HexUtils.WorldPositionToAxialCoords(position, Metrics);
		}
		
		#endregion
		
	}
	
}