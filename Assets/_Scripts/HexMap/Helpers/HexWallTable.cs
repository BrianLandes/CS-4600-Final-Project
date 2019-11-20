using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexMap {
	/// <summary>
	/// Contains a 3 axii table mapping the places between the hexagons.
	/// Getting the one side of a hexagon and getting the opposite side of the neighboring hexagon return the same value.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[Serializable]
	public class HexWallTable<T> :  ISerializationCallbackReceiver {

		/// <summary>
		/// A dictionary containing all the tiles in this table where axis 0 is the column and axis 1 is the row and axis 2 is the direction.
		/// The axis 2 holds exactly 3 values: half the sides that this hex connects to.
		/// </summary>
		private Dictionary<int, Dictionary<int, T[]>> columnDict = new Dictionary<int, Dictionary<int, T[]>>();

		/// <summary>
		/// A redundant dictionary containing all the tiles in this table where axis 0 is the row and axis 1 is the column.
		/// </summary>
		private Dictionary<int, Dictionary<int, T[]>> rowDict = new Dictionary<int, Dictionary<int, T[]>>();

		/// <summary>
		/// A redundant dictionary containing all the tiles in this table where axis 0 is the z/cube coord and the value is a list of all tiles with that z coord.
		/// </summary>
		private Dictionary<int, List<T[]>> zDict = new Dictionary<int, List<T[]>>();

		public int Count { get; private set; }

		/// <summary>
		/// Records the coordinates that have recently had changes.
		/// Vector3 where the z is the direction and we use that to find the two affected tiles
		/// HashSet because we don't care about duplicates.
		/// </summary>
		private HashSet<Vector3Int> changedCoords = new HashSet<Vector3Int>();

		#region Serialization

		[SerializeField] List<Vector3Int> _Keys;
		[SerializeField] List<T> _Values;

		public void OnBeforeSerialize() {
			_Keys = new List<Vector3Int>();
			_Values = new List<T>();

			// Go through one of the axes and create a axial coord + direction and save it to a list
			foreach (var bucketPair in columnDict) {
				int col = bucketPair.Key;
				foreach (var pair in bucketPair.Value) {
					int row = pair.Key;
					int direction = 0;
					foreach( var value in pair.Value ) {
						_Keys.Add(new Vector3Int(col, row, direction));
						_Values.Add(value);
						direction++;
					}
				}
			}
		}

		public void OnAfterDeserialize() {
			Clear();

			for ( int i = 0; i < Math.Min(_Keys.Count, _Values.Count);i ++ ) {
				Vector3Int coords = _Keys[i];
				T value = _Values[i];
				Set(coords.x, coords.y, (HexDirection)coords.z, value);
			}
		}

		#endregion

		/// <summary>
		/// Adds a value to the table
		/// </summary>
		/// <param name="tile"></param>
		public void Set(int column, int row, HexDirection direction, T value) {
			Vector3Int index = Convert(column, row, direction);

			// Add the value to the columnDict
			Dictionary<int, T[]> columnBucket = GetColumnBucket(index.x);
			T[] axis2;
			if ( !columnBucket.TryGetValue(index.y, out axis2)) {
				axis2 = new T[3];
				columnBucket[index.y] = axis2;
			}
			axis2[index.z] = value;

			// Add the tile to the rowDict
			Dictionary<int, T[]> rowBucket = GetRowBucket(index.y);
			rowBucket[index.x] = axis2;

			Count++;

			// record that we changed those coordinates
			changedCoords.Add(index);
		}

		public void Set( Vector2Int pos, HexDirection direction, T value ) {
			Set(pos.x, pos.y, direction, value);
		}

		public void BeginChangeCheck() {
			changedCoords.Clear();
		}

		/// <summary>
		/// Returns the coords for tiles that have been affected by changes.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<Vector2Int> EndChangeCheck() {
			HashSet<Vector2Int> tileCoords = new HashSet<Vector2Int>();
			
			foreach( var coord in changedCoords ) {
				// take the coord + direction and add both tiles that touch that edge
				// first one's easy
				tileCoords.Add(new Vector2Int(coord.x, coord.y));
				// second one is move 1 tile in the given direction
				HexDirection direction = (HexDirection)coord.z;
				Vector2Int move = HexUtils.MoveFrom(coord.x, coord.y, direction);
				tileCoords.Add(move);
			}

			foreach (var coord in tileCoords) {
				yield return coord;
			}
		}

		//public void Remove(HexTile tile) {
		//	Remove(tile.column, tile.row, tile);
		//}

		//public bool Contains(int column, int row) {
		//	Dictionary<int, HexTile> columnBucket = GetColumnBucket(column);
		//	return columnBucket.ContainsKey(row);
		//}

		//public void Remove(int column, int row, HexTile tile = null) {
		//	// Remove the tile from the columnDict
		//	Dictionary<int, HexTile> columnBucket = GetColumnBucket(column);
		//	columnBucket.Remove(row);
		//	// Remove the tile from the rowDict
		//	Dictionary<int, HexTile> rowBucket = GetRowBucket(row);
		//	rowBucket.Remove(column);
		//	// Remove the tile from the zDict
		//	List<HexTile> zBucket = GetZBucket(HexUtils.Z(column, row));
		//	if (tile == null) {
		//		foreach (var t in zBucket) {
		//			if (t.column == column && t.row == row) {
		//				tile = t;
		//				continue;
		//			}
		//		}
		//	}
		//	if (tile != null) {
		//		zBucket.Remove(tile);
		//	}


		//	Count--;
		//}

		public void Clear() {
			columnDict.Clear();
			rowDict.Clear();
			Count = 0;
			changedCoords.Clear();
		}

		public T Get(Vector2Int pos, HexDirection direction) {
			return Get(pos.x, pos.y, direction);
		}

		public T Get(int column, int row, HexDirection direction) {
			Vector3Int index = Convert(column, row, direction);
			// We only need to check one, doesn't matter which, might as well be columnDict
			Dictionary<int, T[]> columnBucket = GetColumnBucket(index.x);
			T[] result;
			if (columnBucket.TryGetValue(index.y, out result)) {
				return result[index.z];
			}
			return default(T);
		}

		public bool TryGet( Vector2Int pos, HexDirection direction , out T result ) {
			return TryGet(pos.x, pos.y, direction, out result);
		}

		public bool TryGet( int column, int row, HexDirection direction, out T result ) {
			Vector3Int index = Convert(column, row, direction);
			// We only need to check one, doesn't matter which, might as well be columnDict
			Dictionary<int, T[]> columnBucket = GetColumnBucket(index.x);
			T[] directionBucket;
			if (columnBucket.TryGetValue(index.y, out directionBucket)) {
				result = directionBucket[index.z];
				return true;
			}
			result = default(T);
			return false;
		}

		//public IEnumerable<HexTile> GetAllTilesInRow(int row) {
		//	Dictionary<int, HexTile> rowBucket = GetRowBucket(row);
		//	foreach (HexTile tile in rowBucket.Values) {
		//		yield return tile;
		//	}
		//}

		//public IEnumerable<HexTile> GetAllTilesInColumn(int column) {
		//	Dictionary<int, HexTile> columnBucket = GetColumnBucket(column);
		//	foreach (HexTile tile in columnBucket.Values) {
		//		yield return tile;
		//	}
		//}

		///// <summary>
		///// Get's all the tiles with the given z coordinate.
		///// Most process intense GetAll function since the tiles aren't stored by their z anywhere.
		///// </summary>
		///// <param name="z"></param>
		///// <returns></returns>
		//public IEnumerable<HexTile> GetAllTilesInZ(int z) {
		//	List<HexTile> zBucket = GetZBucket(z);
		//	foreach (HexTile tile in zBucket) {
		//		yield return tile;
		//	}
		//}

		///// <summary>
		///// Returns any and all neighbors adjacent to the given coordinates.
		///// </summary>
		///// <param name="column"></param>
		///// <param name="row"></param>
		///// <returns></returns>
		//public IEnumerable<NeighborResult> GetAllNeighbors(int column, int row) {
		//	HexTile neighbor;
		//	foreach (HexDirection direction in HexDirectionUtils.All()) {
		//		if (TryGet(HexUtils.MoveFrom(column, row, direction), out neighbor)) {
		//			yield return new NeighborResult(direction, neighbor);
		//		}
		//	}
		//}

		/// <summary>
		/// Goes into the columnDict and returns the bucket associated with the given column.
		/// If a bucket does not exist yet one is created.
		/// </summary>
		/// <param name="column"></param>
		/// <returns></returns>
		private Dictionary<int, T[]> GetColumnBucket(int column) {
			return GetBucket(column, columnDict);
		}

		/// <summary>
		/// Goes into the rowDict and returns the bucket associated with the given row.
		/// If a bucket does not exist yet one is created.
		/// </summary>
		/// <param name="column"></param>
		/// <returns></returns>
		private Dictionary<int, T[]> GetRowBucket(int row) {
			return GetBucket(row, rowDict);
		}

		private Dictionary<int, T[]> GetBucket(int key, Dictionary<int, Dictionary<int, T[]>> dict) {
			Dictionary<int, T[]> result;
			if (!dict.TryGetValue(key, out result)) {
				result = new Dictionary<int, T[]>();
				dict[key] = result;
			}
			return result;
		}

		//private List<HexTile> GetZBucket(int z) {
		//	List<HexTile> result;
		//	if (!zDict.TryGetValue(z, out result)) {
		//		result = new List<HexTile>();
		//		zDict[z] = result;
		//	}
		//	return result;
		//}

		/// <summary>
		/// Since the axis 2 only holds 3 values in order to avoid redundancy, the hex at the given coordinates only
		/// contains half the information. If the given direction is not one of the first three, we have to go to 
		/// one of the three neighboring hexagons in order to find the common value.
		/// </summary>
		/// <param name="column"></param>
		/// <param name="row"></param>
		/// <param name="direction"></param>
		/// <returns></returns>
		public static Vector3Int Convert( int column, int row, HexDirection direction ) {
			if ( (int)direction >=3 ) {
				Vector2Int move = HexUtils.MoveFrom(column, row, direction);
				return new Vector3Int(move.x, move.y, (int)direction.Opposite());
			} else {
				return new Vector3Int(column, row, (int)direction);
			}
		}
		
	}
}