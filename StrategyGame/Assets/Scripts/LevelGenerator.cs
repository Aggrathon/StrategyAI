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

    public Map Generate(Texture2D image, int width, int height, MLAcademy academy) {
		if (width < image.width)
			Debug.LogError("The map is wider than the level");
		if (height < image.height)
			Debug.LogError("The map is higher then the level");
		Map map = new Map(width, height);

		//Spawn Objects
		floor.localScale = new Vector3(width, 1, height);
		floor.position = new Vector3(width * 0.5f-0.5f, 0, height * 0.5f-0.5f); ;
		cameraMount.position = new Vector3(width * 0.5f - 0.5f, 0, height * 0.5f - 0.5f); ;
		Color32[] pixels = image.GetPixels32();
		SpawnLevel (pixels, image.width, image.height, map, (width-image.width)/2, (height-image.height)/2);

		//Calculate Navigation
		map.CalculateNavMesh();

		//Spawn Units
		SpawnUnits(pixels, image.width, image.height, academy, (width - image.width) / 2, (height - image.height) / 2);

		return map;
	}

	void SpawnLevel(Color32[] colors, int width, int height, Map map, int offsetX=0, int offsetY=0)
	{
		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {
				var item = colors [x + y * width];
				var position = new Vector3 (x+offsetX, 0, y+offsetY);
				if (item.r == item.b && item.r == item.g) {
					if (item.r == 255)
					{
						map.SetTile(x + offsetX, y + offsetY, false, 0);
						continue;
					}
					else
					{
						position.y = -(float)item.r / 255.0f;
						map.SetTile(x + offsetX, y + offsetY, false, 1.0f+position.y);
						ObjectPool.Spawn (wall, position, Quaternion.identity);
						continue;
					}
				}
				else if (item.r == 0 && item.b == 0)
				{
					map.SetTile(x + offsetX, y + offsetY, true, 0);
					ObjectPool.Spawn(goal, position, Quaternion.identity);
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
