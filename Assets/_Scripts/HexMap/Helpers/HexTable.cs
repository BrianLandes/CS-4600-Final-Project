using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexMap {
	/// <summary>
	/// Data structure class for storing HexTiles or gameobjects using their axial coordinates (column, row).
	/// Allows inserting, deleting, searching, and indexing using a specific column and row, or by just column or by just row.
	/// </summary>
	//[Serializable]
	public class HexTable<T> {
		/// <summary>
		/// A dictionary containing all the tiles in this table where axis 0 is the column and axis 1 is the row.
		/// </summary>
		//[SerializeField]
		private Dictionary<int, Dictionary<int, T>> columnDict = new Dictionary<int, Dictionary<int, T>>();

		/// <summary>
		/// A redundant dictionary containing all the tiles in this table where axis 0 is the row and axis 1 is the column.
		/// </summary>
		//[SerializeField]
		private Dictionary<int, Dictionary<int, T>> rowDict = new Dictionary<int, Dictionary<int, T>>();

		/// <summary>
		/// A redundant dictionary containing all the tiles in this table where axis 0 is the z/cube coord and axis 1 is the column (could've been row but it doesn't matter)
		/// </summary>
		//[SerializeField]
		private Dictionary<int, Dictionary<int, T>> zDict = new Dictionary<int, Dictionary<int, T>>();

		public int Count { get; private set; }

		/// <summary>
		/// Adds a tile to the table.
		/// </summary>
		/// <param name="tile"></param>
		public void Set( int column, int row, T value ) {
			// Add the tile to the columnDict
			Dictionary<int, T> columnBucket = GetColumnBucket(column);
			columnBucket[row] = value;
			// Add the tile to the rowDict
			Dictionary<int, T> rowBucket = GetRowBucket(row);
			rowBucket[column] = value;
			// Add the tile to the zDict
			Dictionary<int, T> zBucket = GetZBucket(HexUtils.Z(column, row));
			zBucket[column] = value;

			Count++;
		}

		public void Set( Vector2Int pos, T value ) {
			Set(pos.x, pos.y, value);
		}
		
		public bool Contains(int column, int row) {
			Dictionary<int, T> columnBucket = GetColumnBucket(column);
			return columnBucket.ContainsKey(row);
		}

		public void Remove(Vector2Int pos) {
			Remove(pos.x, pos.y);
		}

		public void Remove(int column, int row) {
			// Remove the tile from the columnDict
			Dictionary<int, T> columnBucket = GetColumnBucket(column);
			if ( columnBucket.Remove(row) ) {
				// Remove the tile from the rowDict
				Dictionary<int, T> rowBucket = GetRowBucket(row);
				rowBucket.Remove(column);
				// Remove the tile from the zDict
				Dictionary<int, T> zBucket = GetZBucket(HexUtils.Z(column, row));
				zBucket.Remove(column);

				Count--;
			}
			
		}

		/// <summary>
		/// Removes all entries that have a row component equal to the given row.
		/// Returns each of the values in an IEnumerable so that further operations can
		/// be applied on each object (ie: destroying) without having to iterate through them a second time.
		/// Note: for this reason, the function must be called in a foreach and fully iterated to complete its function.
		/// For an immediate row removal use RemoveRowImmediate.
		/// </summary>
		/// <param name="row"></param>
		/// <returns></returns>
		public IEnumerable<T> RemoveRow(int row) {
			Dictionary<int, T> rowBucket = GetRowBucket(row);
			List<int> keys = new List<int>(rowBucket.Keys);
			foreach (int key in keys) {

				yield return rowBucket[key];

				Remove(key, row);
			}
		}

		/// <summary>
		/// Removes all entries that have a row component equal to the given row.
		/// Returns each of the values in an IEnumerable so that further operations can
		/// be applied on each object (ie: destroying) without having to iterate through them a second time.
		/// </summary>
		/// <param name="row"></param>
		/// <returns></returns>
		public IEnumerable<T> RemoveColumn(int column) {
			Dictionary<int, T> columnBucket = GetColumnBucket(column);
			List<int> keys = new List<int>(columnBucket.Keys);
			foreach (int key in keys) {

				yield return columnBucket[key];

				Remove(column, key);
			}
		}

		/// <summary>
		/// Removes all entries that have a z component equal to the given z.
		/// Returns each of the values in an IEnumerable so that further operations can
		/// be applied on each object (ie: destroying) without having to iterate through them a second time.
		/// </summary>
		/// <param name="row"></param>
		/// <returns></returns>
		public IEnumerable<T> RemoveZ(int z) {
			Dictionary<int, T> zBucket = GetZBucket(z);
			List<int> keys = new List<int>(zBucket.Keys);
			foreach (int key in keys) {

				yield return zBucket[key];

				Remove(key, HexUtils.RowFromZ(key, z));
			}
		}

		public void RemoveRowImmediate(int row ) {
			foreach( var value in RemoveRow(row) ) {
				// TODO: do some copypasta instead of IEnumerable here
			}
		}

		/// <summary>
		/// Removes all entries that have a row component equal to the given row.
		/// </summary>
		/// <param name="column"></param>
		public void RemoveColumnImmediate(int column) {
			foreach (var value in RemoveColumn(column)) {
				// TODO: do some copypasta instead of IEnumerable here
			}
		}

		public void Clear() {
			columnDict.Clear();
			rowDict.Clear();
			zDict.Clear();
			Count = 0;
		}

		public bool TryGet( Vector2Int pos, out T tile) {
			return TryGet(pos.x, pos.y, out tile);
		}

		public bool TryGet(int column, int row, out T tile ) {
			// We only need to check one, doesn't matter which, might as well be columnDict
			Dictionary<int, T> columnBucket = GetColumnBucket(column);
			return columnBucket.TryGetValue(row, out tile);
		}

		/// <summary>
		/// Simple getter for values. Returns null or the default of T if the entry does not exist.
		/// </summary>
		/// <param name="column"></param>
		/// <param name="row"></param>
		/// <returns></returns>
		public T Get( int column, int row ) {
			Dictionary<int, T> columnBucket = GetColumnBucket(column);
			T value;
			if ( columnBucket.TryGetValue(row, out value) ) {
				return value;
			}
			return default(T);
		}

		public T Get( Vector2Int pos ) {
			return Get(pos.x, pos.y);
		}

		public IEnumerable<T> GetAllTilesInRow( int row ) {
			Dictionary<int, T> rowBucket = GetRowBucket(row);
			foreach( T tile in rowBucket.Values ) {
				yield return tile;
			}
		}

		public IEnumerable<T> GetAllTilesInColumn(int column) {
			Dictionary<int, T> columnBucket = GetColumnBucket(column);
			foreach (T tile in columnBucket.Values) {
				yield return tile;
			}
		}

		/// <summary>
		/// Get's all the tiles with the given z coordinate.
		/// </summary>
		/// <param name="z"></param>
		/// <returns></returns>
		public IEnumerable<T> GetAllTilesInZ(int z) {
			Dictionary<int, T> zBucket = GetZBucket(z);
			foreach (T tile in zBucket.Values) {
				yield return tile;
			}
		}

		/// <summary>
		/// Returns any and all neighbors adjacent to the given coordinates.
		/// </summary>
		/// <param name="column"></param>
		/// <param name="row"></param>
		/// <returns></returns>
		public IEnumerable<NeighborResult<T>> GetAllNeighbors(int column, int row ) {
			T neighbor;
			foreach( HexDirection direction in HexDirectionUtils.All()) {
				if ( TryGet( HexUtils.MoveFrom(column,row,direction), out neighbor ) ) {
					yield return new NeighborResult<T>(direction, neighbor);
				}
			}
		}

		/// <summary>
		/// Goes into the columnDict and returns the bucket associated with the given column.
		/// If a bucket does not exist yet one is created.
		/// </summary>
		/// <param name="column"></param>
		/// <returns></returns>
		private Dictionary<int, T> GetColumnBucket(int column) {
			return GetBucket(column, columnDict);
		}

		/// <summary>
		/// Goes into the rowDict and returns the bucket associated with the given row.
		/// If a bucket does not exist yet one is created.
		/// </summary>
		/// <param name="column"></param>
		/// <returns></returns>
		private Dictionary<int, T> GetRowBucket(int row) {
			return GetBucket(row, rowDict);
		}

		private Dictionary<int, T> GetZBucket(int z) {
			return GetBucket(z, zDict);
		}

		private Dictionary<int, T> GetBucket(int key, Dictionary<int, Dictionary<int, T>> dict ) {
			Dictionary<int, T> result;
			if (!dict.TryGetValue(key, out result)) {
				result = new Dictionary<int, T>();
				dict[key] = result;
			}
			return result;
		}

		
	}

	public struct NeighborResult<T> {
		/// <summary>
		/// The direction moved to go FROM the origin, to this resulting tile.
		/// </summary>
		public HexDirection direction;

		public T neighbor;

		public NeighborResult( HexDirection direction, T neighbor ) {
			this.direction = direction;
			this.neighbor = neighbor;
		}
	}
}