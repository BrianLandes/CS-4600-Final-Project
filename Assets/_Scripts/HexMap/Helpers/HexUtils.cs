using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexMap {

	
	public static class HexUtils {

		public static readonly float TWO_PI = Mathf.PI * 2.0f;
		public static readonly float ONE_THIRD_PI = Mathf.PI / 3f;
		public static readonly float ONE_SIXTH_PI = Mathf.PI / 6f;
		public static readonly float ONE_TWELFTH_PI = Mathf.PI / 12f;
		public static readonly float ROOT_THREE_OVER_TWO = Mathf.Sqrt(3) / 2f;

		/// <summary>
		/// Pointy-top, radians, corner zero is 4-o'clock, moves CCW
		/// </summary>
		public static float CornerAngle(int index) {
			return ONE_THIRD_PI * index - ONE_SIXTH_PI;
		}

		/// <summary>
		/// Returns the unit circle position of the hex corner on the x-z plane. Centered at zero
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public static Vector3 CornerPosition(int index ) {
			float theta = CornerAngle(index);
			return new Vector3(Mathf.Cos(theta), 0, Mathf.Sin(theta));
		}

		/// <summary>
		/// Returns the median point on the edge in the given direction
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public static Vector3 CenterOfSide( HexDirection direction ) {
			float theta = CornerAngle(direction.Int()) + ONE_SIXTH_PI;
			return new Vector3(Mathf.Cos(theta), 0, Mathf.Sin(theta)) * ROOT_THREE_OVER_TWO;
		}

		/// <summary>
		/// Returns the uv coordinates for the hex corner
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public static Vector2 CornerUV(int index) {
			float theta = CornerAngle(index);
			return new Vector2(Mathf.Cos(theta) * 0.5f + 0.5f, Mathf.Sin(theta) * 0.5f + 0.5f);
		}

		/// <summary>
		/// The edge to edge distance of the left flat side and the right flat side.
		/// </summary>
		/// <param name="size"></param>
		/// <returns></returns>
		public static float Width(float size) {
			return Mathf.Sqrt(3) * size;
		}

		/// <summary>
		/// The corner to corner distance from the top point to the bottom point
		/// </summary>
		/// <param name="size"></param>
		/// <returns></returns>
		public static float Height(float size ) {
			return 2f * size;
		}

		public static float SideLength( float size ) {
			return size;
		}

		/// <summary>
		/// Returns the number of tiles within a hex-shaped area with the given radius.
		/// A radius of 0 is 1 tile and radius of 1 is 1 tile with 1 ring of tiles around it.
		/// </summary>
		/// <param name="radius"></param>
		/// <returns></returns>
		public static int Area( int radius ) {
			if ( radius == 0 ) {
				return 1;
			}
			return (int)Mathf.Pow(2, radius) * 3 + Area( radius -1 );
		}

		/// <summary>
		/// Returns a random float value between -1 and 1.
		/// </summary>
		/// <returns></returns>
		public static float RandomOneOne() {
			return UnityEngine.Random.value * 2.0f - 1.0f;
		}

		/// <summary>
		/// Returns the axial coordinates of a random tile within the given hex-shaped area, centered at the given column and row.
		/// </summary>
		/// <param name="column"></param>
		/// <param name="row"></param>
		/// <param name="radius"></param>
		/// <returns></returns>
		public static Vector2Int RandomPointInArea( int column, int row, int radius ) {
			int x;
			int y;
			// Choose random tiles until we find one within the hex area
			do {
				int deltaX = Mathf.RoundToInt(RandomOneOne() * radius);
				x = column + deltaX;
				int deltaY = Mathf.RoundToInt(RandomOneOne() * radius);
				y = row + deltaY;
			} while (Distance(column, row, x, y) > radius);

			return new Vector2Int(x, y);
		}

		/// <summary>
		/// Returns a Unity world position based on the given axial coordinates on the x-z plane
		/// </summary>
		/// <param name="column"></param>
		/// <param name="row"></param>
		/// <returns></returns>
		public static Vector3 PositionFromCoordinates( int column, int row, float size ) {
			float height = Height(size);
			float width = Width(size);
			//int oddColumn = column & 1;
			float x = row * width * 0.5f + column * width;
			float z = row * height * 0.75f; // The vertical distance between adjacent hexagon centers is h * 3/4.
			return new Vector3(x, 0, z);
		}

		public static Vector3 PositionFromCoordinates( Vector2Int pos, float size = 1 ) {
			return PositionFromCoordinates(pos.x, pos.y, size);
		}

		/// <summary>
		/// Takes a Hexagon mesh generated from MeshBuilder and maps the uv coordinates for each tile to a piece of a larger texture.
		/// </summary>
		/// <param name="mesh"></param>
		/// <param name="scale"></param>
		/// <param name="position"></param>
		public static void MapUVs( ref Mesh mesh, Vector2 offset, float scale) {
			List<Vector2> uvs = ListPool<Vector2>.Get();

			offset = offset * scale;

			Vector2 center = new Vector2(0.0f, 0.0f) * scale + offset;
			
			for (int i = 0; i < 6; i++) {

				uvs.Add(center);

				float theta = CornerAngle(i+1);
				uvs.Add(new Vector2(Mathf.Cos(theta), Mathf.Sin(theta)) * scale + offset);

				theta = CornerAngle(i);
				uvs.Add(new Vector2(Mathf.Cos(theta), Mathf.Sin(theta)) * scale + offset);
				
			}

			mesh.SetUVs(0, uvs);
			ListPool<Vector2>.Add(uvs);
		}
		
		public static Vector2Int UVCoordsToPixelCoords(Texture2D texture, Vector2 uvCoords) {

			int x = Mathf.RoundToInt( (uvCoords.x * (float)texture.width) % (float)texture.width );
			int y = Mathf.RoundToInt( (uvCoords.y * (float)texture.height) % (float)texture.height );
			return new Vector2Int(x, y);
		}

		public static float GetHeightOnNoiseMap(Texture2D texture, Vector2 uvCoords) {
			Vector2Int point = UVCoordsToPixelCoords(texture, uvCoords);
			return texture.GetPixel(point.x, point.y).r;
		}

		/// <summary>
		/// The number of tiles from the origin to the given axial coordinates
		/// </summary>
		public static int Distance(int column, int row ) {
			int z = -column - row; // axial coordinates are simply Cube coordinates where x + y + z = 0, so we leave of the z, but we need it here
							// The distance from one tile to another is simply the largest abs component of the difference of the two vectors.
			return Mathf.Max(Mathf.Abs(column), Mathf.Abs(row), Mathf.Abs(z));
		}

		/// <summary>
		/// The number of tiles for one axial coordinate to another.
		/// </summary>
		/// <param name="c1"></param>
		/// <param name="r1"></param>
		/// <param name="c2"></param>
		/// <param name="r2"></param>
		/// <returns></returns>
		public static int Distance( int c1, int r1, int c2, int r2 ) {
			int c = c2 - c1;
			int r = r2 - r1;
			int z = -c - r; // axial coordinates are simply Cube coordinates where x + y + z = 0, so we leave of the z, but we need it here
			// The distance from one tile to another is simply the largest abs component of the difference of the two vectors.
			return Mathf.Max(Mathf.Abs(c), Mathf.Abs(r), Mathf.Abs(z));
		}

		public static int Distance(Vector2Int a, Vector2Int b) {
			int c = a.x - b.x;
			int r = a.y - b.y;
			int z = -c - r; // axial coordinates are simply Cube coordinates where x + y + z = 0, so we leave of the z, but we need it here
							// The distance from one tile to another is simply the largest abs component of the difference of the two vectors.
			return Mathf.Max(Mathf.Abs(c), Mathf.Abs(r), Mathf.Abs(z));
		}

		/// <summary>
		/// Returns the tile's coordinates if we were to move from the given axial coordinates in the given direction 1 tile.
		/// </summary>
		/// <param name="column"></param>
		/// <param name="row"></param>
		/// <param name="direction"></param>
		/// <returns></returns>
		public static Vector2Int MoveFrom(int column, int row, HexDirection direction, int spaces = 1 ) {
			switch (direction) {
				case HexDirection.E: return new Vector2Int(column + spaces, row);
				case HexDirection.NE: return new Vector2Int(column, row + spaces); 
				case HexDirection.NW: return new Vector2Int(column - spaces, row + spaces);
				case HexDirection.W: return new Vector2Int(column - spaces, row);
				case HexDirection.SW: return new Vector2Int(column, row - spaces);
				case HexDirection.SE: return new Vector2Int(column + spaces, row - spaces);
				default:
					throw new System.Exception("Unknown Direction");
			}
		}

		public static Vector2Int MoveFrom( Vector2Int pos, HexDirection direction, int spaces = 1 ) {
			return MoveFrom(pos.x, pos.y, direction, spaces);
		}

		/// <summary>
		/// Takes an axial coordinate change (+-1 or 0, +-1 or 0) and converts it into a HexDirection
		/// </summary>
		/// <param name="colMove"></param>
		/// <param name="rowMove"></param>
		/// <returns></returns>
		public static HexDirection VectorToDirection( int colMove, int rowMove ) {
			if ( colMove == +1 ) {
				switch( rowMove ) {
					case -1: return HexDirection.SE;
					case 0: return HexDirection.E;
				}
			} else
			if ( colMove == 0 ) {
				switch (rowMove) {
					case -1: return HexDirection.SW;
					case +1: return HexDirection.NE;
				}
			} else
			if (colMove == -1) {
				switch (rowMove) {
					case 0: return HexDirection.W;
					case +1: return HexDirection.NW;
				}
			}
			throw new System.Exception("Unknown Direction");
		}

		public static IEnumerable<Vector2Int> ForEachTileWithin( int centerColumn, int centerRow, int radius ) {
			for (int c = -radius; c <= radius; c++) {
				for (int r = -radius; r <= radius; r++) {
					// There ends up being a few tiles that are iterated over but not within the circle
					if (HexUtils.Distance(c, r) <= radius) {
						yield return new Vector2Int(c + centerColumn, r + centerRow);
					}
				}
			}
		}

		public static int Z( int column, int row ) {
			return -column - row; // axial coordinates are simply Cube coordinates where x + y + z = 0, so we leave of the z, but we need it here
		}
		
		public static int RowFromZ( int column, int z ) {
			return -column - z;
		}

		public static Vector2Int WorldPositionToAxialCoords(Vector3 position, HexMetrics metrics ) {
			//position = transform.InverseTransformPoint(position);

			float q = ((float)Mathf.Sqrt(3) / 3f * position.x - 1f / 3f * position.z) / metrics.tileSize;
			float r = (2f / 3f * position.z) / metrics.tileSize;

			Vector3 cubeCoords = new Vector3(q, -q-r, r);
			Vector3Int cubeCoordsRounded = FloatCubeCoordsToIntCubeCoords(cubeCoords);
			return new Vector2Int(cubeCoordsRounded.x, cubeCoordsRounded.z);
		}
		
		public static Vector3Int FloatCubeCoordsToIntCubeCoords( Vector3 point ) {
			var rx = Mathf.Round(point.x);
			var ry = Mathf.Round(point.y);
			var rz = Mathf.Round(point.z);
			
			var x_diff = Mathf.Abs(rx - point.x);
			var y_diff = Mathf.Abs(ry - point.y);
			var z_diff = Mathf.Abs(rz - point.z);
			
			if (x_diff > y_diff && x_diff > z_diff) {
				rx = -ry - rz;
			} else if (y_diff > z_diff) {
				ry = -rx - rz;
			} else {
				rz = -rx - ry;
			}
			
			return new Vector3Int((int)rx, (int)ry, (int)rz);
		}
	}
	
}
