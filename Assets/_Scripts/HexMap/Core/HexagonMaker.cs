//using System;
//using Obi;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.AI;

namespace HexMap {
	/// <summary>
	/// Creates a single pointy-topped hex tile.
	/// https://www.redblobgames.com/grids/hexagons/
	/// 
	/// </summary>
	public class HexagonMaker : MonoBehaviour {

		public HexMetrics metrics;

		public Material material;
		
		private HexMap hexMap;

		private MeshBuilder meshBuilder;

		private GameObject tilesFolder;

		private Stack<GameObject> hexTilePool = new Stack<GameObject>();

		private void Start() {
			meshBuilder = new MeshBuilder();
			hexMap = GetComponent<HexMap>();
			tilesFolder = ObjectFactory.Folder("Tiles", transform);
		}
		public void ClearAllTiles() {
			
			CommonUtils.DestroyChildren(transform);
			//tileTable.Clear();
			//hexWallTable.Clear();
			//isWallTable.Clear();
		}

		public void GenerateTilesAt( int column, int row, int radius ) {
			hexMap = GetComponent<HexMap>();
			meshBuilder = new MeshBuilder();

			tilesFolder = ObjectFactory.Folder("Tiles", transform);
			foreach ( var pos in HexUtils.ForEachTileWithin(column, row, radius) ) {
				HexTile hexTile = NewHexTile(pos.x, pos.y );
			}
			
		}

		public void ReturnTileToPool(HexTile tile ) {
			hexTilePool.Push(tile.gameObject);
			tile.gameObject.SetActive(false);
		}

		public HexTile GenerateTile(int column, int row) {
			//if (Application.isPlaying && hexTilePool.Count > 0) {
			//	var reuseTile = hexTilePool.Pop();
			//	return RecycleTile(reuseTile, column, row);
			//}
			//else {
			return NewHexTile(column, row);
			//}
		}

		//private HexTile RecycleTile(GameObject tile, int column, int row) {
		//	Vector3 pos = HexUtils.PositionFromCoordinates(column, row, metrics.tileSize);
		//	tile.gameObject.SetActive(true);
		//	Mesh hexMesh;
		//	MeshFilter filter = tile.GetComponent<MeshFilter>();
		//	hexMesh = meshBuilder.BaseHexagon(metrics, hexMap, column, row, filter.mesh);
		//	filter.mesh = hexMesh;

		//	MeshRenderer renderer = tile.GetComponent<MeshRenderer>();
		//	if (material != null) {
		//		renderer.material = material;
		//	}
		//	MeshCollider collider = tile.GetComponent<MeshCollider>();
		//	collider.sharedMesh = hexMesh;
		//	tile.layer = gameObject.layer;

		//	HexTile hexTile = tile.GetComponent<HexTile>();
		//	hexTile.column = column;
		//	hexTile.row = row;
		//	hexTile.metrics = metrics;
		//	hexTile.map = hexMap;
		//	hexTile.NavMeshBuilt = false;

		//	tile.transform.localPosition = pos;

		//	//hexTile.staticObstacles.AddRange(obstacles);

		//	//hexTile.BuildNavMesh();

		//	return hexTile;
		//}

		private HexTile NewHexTile( Vector2Int pos ) {
			return NewHexTile(pos.x, pos.y);
		}

		private HexTile NewHexTile(int column, int row ) {

			Vector3 pos = HexUtils.PositionFromCoordinates(column, row, metrics.tileSize);



			//if ( IsWallAt( column, row ) ) {
			//	hexMesh = MountainHexagon(tileSize, column, row );
			//} else {
			//	hexMesh = Hexagon(tileSize, pos);
			//}

			string name = string.Format("Tile ({0},{1})", column, row);
			GameObject tile = ObjectFactory.Folder(name, tilesFolder.transform);
			tile.layer = gameObject.layer;
			tile.transform.localPosition = pos;

			HexTile hexTile = tile.AddComponent<HexTile>();
			
			hexTile.column = column;
			hexTile.row = row;
			hexTile.metrics = metrics;
			hexTile.map = hexMap;

			// Ground Mesh
			{
				Mesh hexMesh;
				hexMesh = meshBuilder.ElevatedTileHexagon(metrics, hexMap, column, row);
				GameObject groundMesh = NewMeshedObject("Ground Mesh", tile.transform, hexMesh, material);
				groundMesh.layer = gameObject.layer;
				hexTile.reusableMeshes.Add(hexMesh);
				groundMesh.transform.localPosition = Vector3.zero;
				//var onlyExists = groundMesh.AddComponent<ExistsOnlyWhenInSight>();
			}

			//// Ground Mesh
			//{
			//	Mesh hexMesh;
			//	hexMesh = meshBuilder.WallsHexagon(metrics, hexMap, column, row);
			//	GameObject groundMesh = NewMeshedObject("Legacy Mesh", tile.transform, hexMesh, material);
			//	groundMesh.layer = gameObject.layer;
			//	hexTile.reusableMeshes.Add(hexMesh);
			//	groundMesh.transform.localPosition = Vector3.zero;
			//	//var onlyExists = groundMesh.AddComponent<ExistsOnlyWhenInSight>();
			//}

			//// Wall Meshes
			//foreach (var corner in HexCornerUtils.AllCorners() ) {
			//	Mesh wallMesh;
			//	wallMesh = meshBuilder.WallMesh(metrics, hexMap, column, row, corner);
			//	GameObject wallMeshObject = NewMeshedObject("Wall Mesh " + corner.ToString(),
			//		tile.transform, wallMesh, wallMaterial);
			//	wallMeshObject.layer = gameObject.layer;
			//	hexTile.reusableMeshes.Add(wallMesh);
			//	wallMeshObject.transform.localPosition = Vector3.zero;
			//}


			//hexTile.staticObstacles.AddRange(obstacles);

			//hexTile.BuildNavMesh();

			return hexTile;
		}
		

		/// <summary>
		/// Takes the world position and returns the tile's axial coordinates that contains this position
		/// relative to the map's tile size and scale.
		/// </summary>
		/// <param name="position"></param>
		/// <returns></returns>
		public Vector2Int WorldPositionToAxialCoords(Vector3 position) {
			position = transform.InverseTransformPoint(position);
			return HexUtils.WorldPositionToAxialCoords(position, metrics);
		}

		public Vector3 AxialCoordsToWorldPosition(Vector2Int pos ) {
			return HexUtils.PositionFromCoordinates(pos.x, pos.y, metrics.tileSize);
		}
		
		private GameObject NewMeshedObject(string name, Transform parent, Mesh mesh, Material material = null) {
			GameObject obj = new GameObject(name);
			obj.transform.SetParent(parent);

			MeshFilter filter = obj.AddComponent<MeshFilter>();
			filter.mesh = mesh;
			MeshRenderer renderer = obj.AddComponent<MeshRenderer>();
			if (material != null) {
				renderer.material = material;
			}
			MeshCollider collider = obj.AddComponent<MeshCollider>();
			collider.sharedMesh = mesh;

			//ObiRigidbody obiBody = obj.AddComponent<ObiRigidbody>();

			//ObiCollider obiCollider = obj.AddComponent<ObiCollider>();

			return obj;
		}
	}
}