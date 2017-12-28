using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LevelController : MonoBehaviour {

    public Texture2D map;
    public Transform floor;
	public LayerMask navMeshLayerMask;
	public Brain brain;
	[Header("Prefabs")]
    public GameObject wall;
	public GameObject soldier;

	public Vector3 offset { get; protected set; }

	List<GameObject> spawns;

	void Start () {
		spawns = new List<GameObject>();
        Generate();
	}

    public void Generate() {
        int width = map.width;
        int height = map.height;

		//Spawn Objects
		floor.localScale = new Vector3(width, 1, height);
		offset = new Vector3(-width * 0.5f+0.5f, 0, -height * 0.5f+0.5f);
        Color32[] pixels = map.GetPixels32();
		SpawnLevel (pixels, width, height);
		//Calculate Navigation
		var bounds = new Bounds(Vector3.zero, new Vector3(width, 1.5f, height));
		var sources = new List<NavMeshBuildSource>();
		var markups = new List<NavMeshBuildMarkup>();
		NavMeshBuilder.CollectSources(bounds, navMeshLayerMask.value, NavMeshCollectGeometry.PhysicsColliders, 0, markups, sources);
		var data = NavMeshBuilder.BuildNavMeshData(NavMesh.GetSettingsByIndex(0), sources, bounds, Vector3.zero, Quaternion.identity);
		if (data == null)
			Debug.LogError("Could not create the Nav Mesh");
		else
			NavMesh.AddNavMeshData(data);
		//Spawn Units
		SpawnUnits(pixels, width, height);
	}

	void SpawnLevel(Color32[] map, int width, int height)
	{
		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {
				var item = map [x + y * width];
				var position = new Vector3 (x, 0, y) + offset;
				if (item.r == item.b && item.r == item.g) {
					if (item.r == 255)
						continue;
					else {
						position.y = -(float)item.r / 255.0f;
						var go = ObjectPool.Spawn (wall, position, Quaternion.identity);
						go.isStatic = true;
						spawns.Add (go);
					}
				}
			}
		}
	}

	void SpawnUnits(Color32[] map, int width, int height)
	{
		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {
				var item = map [x + y * width];
				var position = new Vector3 (x, 0, y) + offset;
				if (item.r == 0 && item.g == 0 && item.b != 0) {
					GameObject go = ObjectPool.Spawn(soldier, position, Quaternion.LookRotation(-position, Vector3.up));
					spawns.Add (go);
					go.GetComponent<Soldier>().GiveBrain(brain);
				}
			}
		}
	}

	public void Despawn()
	{
		for (int i = 0; i < spawns.Count; i++)
		{
			spawns[i].SetActive(false);
		}
		spawns.Clear();
	}
}
