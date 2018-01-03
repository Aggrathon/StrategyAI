using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LevelGenerator : MonoBehaviour {
	
    public Transform floor;
	public LayerMask navMeshLayerMask;
	public Material playerOneColor;
	public Material playerTwoColor;
	public Camera aiCamera;
	public Transform cameraMount;
	[Header("Prefabs")]
    public GameObject wall;
	public GameObject soldier;
	public GameObject goal;

	void Awake () {
	}

    public void Generate(Texture2D map, int width, int height, MLAcademy academy) {
		if (width < map.width)
			Debug.LogError("The map is wider than the level");
		if (height < map.height)
			Debug.LogError("The map is higher then the level");

		//Spawn Objects
		floor.localScale = new Vector3(width, 1, height);
		floor.position = new Vector3(width * 0.5f-0.5f, 0, height * 0.5f-0.5f); ;
		cameraMount.position = new Vector3(width * 0.5f - 0.5f, 0, height * 0.5f - 0.5f); ;
		Color32[] pixels = map.GetPixels32();
		SpawnLevel (pixels, map.width, map.height, academy.goalCache, (width-map.width)/2, (height-map.height)/2);

		//Calculate Navigation
		var bounds = new Bounds(cameraMount.position, new Vector3(width, 1.5f, height));
		var sources = new List<NavMeshBuildSource>();
		var markups = new List<NavMeshBuildMarkup>();
		NavMeshBuilder.CollectSources(bounds, navMeshLayerMask.value, NavMeshCollectGeometry.PhysicsColliders, 0, markups, sources);
		var data = NavMeshBuilder.BuildNavMeshData(NavMesh.GetSettingsByIndex(0), sources, bounds, Vector3.zero, Quaternion.identity);
		if (data == null)
			Debug.LogError("Could not create the Nav Mesh");
		else
			NavMesh.AddNavMeshData(data);

		//Spawn Units
		SpawnUnits(pixels, map.width, map.height, academy, (width - map.width) / 2, (height - map.height) / 2);
		
	}

	void SpawnLevel(Color32[] map, int width, int height, HashSet<int> goalCache, int offsetX=0, int offsetY=0)
	{
		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {
				var item = map [x + y * width];
				var position = new Vector3 (x+offsetX, 0, y+offsetY);
				if (item.r == item.b && item.r == item.g) {
					if (item.r == 255)
					{
						continue;
					}
					else {
						position.y = -(float)item.r / 255.0f;
						ObjectPool.Spawn (wall, position, Quaternion.identity);
						continue;
					}
				}
				else if (item.r == 0 && item.b == 0)
				{
					ObjectPool.Spawn(goal, position, Quaternion.identity);
					goalCache.Add(Utils.Int2Hash(x+offsetX, y+offsetY));
				}
			}
		}
	}

	void SpawnUnits(Color32[] map, int width, int height, MLAcademy academy, int offsetX = 0, int offsetY = 0)
	{
		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {
				var item = map [x + y * width];
				if (item.r == 0 && item.g == 0 && item.b != 0)
				{
					var position = new Vector3(x + offsetX, 0, y + offsetY);
					GameObject go = ObjectPool.Spawn(soldier, position, Quaternion.LookRotation(floor.position-position, Vector3.up));
					Soldier s = go.GetComponent<Soldier>();
					s.SetTeam(academy.teams[0].brain, playerOneColor, academy, aiCamera);
				}
				else if (item.r != 0 && item.g == 0 && item.b == 0)
				{
					var position = new Vector3(x + offsetX, 0, y + offsetY);
					GameObject go = ObjectPool.Spawn(soldier, position, Quaternion.LookRotation(floor.position-position, Vector3.up));
					Soldier s = go.GetComponent<Soldier>();
					s.SetTeam(academy.teams[1].brain, playerTwoColor, academy, aiCamera);
				}
			}
		}
	}

	public void Despawn()
	{
		ObjectPool.DespawnAll();
	}
}
