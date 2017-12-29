using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LevelGenerator : MonoBehaviour {
	
    public Transform floor;
	public LayerMask navMeshLayerMask;
	public Material playerOneColor;
	public Material playerTwoColor;
	[Header("Prefabs")]
    public GameObject wall;
	public GameObject soldier;

	List<GameObject> spawns;

	void Awake () {
		spawns = new List<GameObject>();
	}

    public GameState Generate(Texture2D map, Brain playerOne, Brain playerTwo, int width, int height, int additionalData=0) {
		if (width < map.width)
			Debug.LogError("The map is wider than the level");
		if (height < map.height)
			Debug.LogError("The map is higher then the level");
		GameState state = new GameState(width, height, new Vector3(-width * 0.5f + 0.5f, 0, -height * 0.5f + 0.5f), additionalData);

		//Spawn Objects
		floor.localScale = new Vector3(width, 1, height);
        Color32[] pixels = map.GetPixels32();
		SpawnLevel (pixels, map.width, map.height, state, (width-map.width)/2, (height-map.height)/2);

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
		SpawnUnits(pixels, map.width, map.height, playerOne, playerTwo, state, (width - map.width) / 2, (height - map.height) / 2);

		return state;
	}

	void SpawnLevel(Color32[] map, int width, int height, GameState state, int offsetX=0, int offsetY=0)
	{
		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {
				var item = map [x + y * width];
				var position = new Vector3 (x+offsetX, 0, y+offsetY) + state.offset;
				if (item.r == item.b && item.r == item.g) {
					if (item.r == 255)
					{
						state.SetTile(x + offsetX, y + offsetY);
						continue;
					}
					else {
						position.y = -(float)item.r / 255.0f;
						var go = ObjectPool.Spawn (wall, position, Quaternion.identity);
						go.isStatic = true;
						spawns.Add (go);
						state.SetTile(x + offsetX, y + offsetY, wall:-position.y);
						continue;
					}
				}
			}
		}
	}

	void SpawnUnits(Color32[] map, int width, int height, Brain playerOne, Brain playerTwo, GameState state, int offsetX = 0, int offsetY = 0)
	{
		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {
				var item = map [x + y * width];
				if (item.r == 0 && item.g == 0 && item.b != 0)
				{
					var position = new Vector3(x + offsetX, 0, y + offsetY) + state.offset;
					GameObject go = ObjectPool.Spawn(soldier, position, Quaternion.LookRotation(-position, Vector3.up));
					spawns.Add(go);
					Soldier s = go.GetComponent<Soldier>();
					s.SetTeam(playerOne, playerOneColor, state);
					state.AddUnit(s, 0);
				}
				else if (item.r != 0 && item.g == 0 && item.b == 0)
				{
					var position = new Vector3(x + offsetX, 0, y + offsetY) + state.offset;
					GameObject go = ObjectPool.Spawn(soldier, position, Quaternion.LookRotation(-position, Vector3.up));
					spawns.Add(go);
					Soldier s = go.GetComponent<Soldier>();
					s.SetTeam(playerTwo, playerTwoColor, state);
					state.AddUnit(s, 1);
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
