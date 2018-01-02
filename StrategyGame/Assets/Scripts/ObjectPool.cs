using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour {

	protected static ObjectPool instance;

	Dictionary<GameObject, Queue<PoolObject>> pool;

	private void Awake()
	{
		pool = new Dictionary<GameObject, Queue<PoolObject>>();
		instance = this;
	}

	private void OnDestroy()
	{
		if (instance == this)
			instance = null;
	}

	public static GameObject Spawn(GameObject obj, Vector3 position, Quaternion rotation)
	{
		if (instance == null)
		{
			GameObject go = new GameObject("ObjectPool");
			instance = go.AddComponent<ObjectPool>();
		}
		else
		{
			Queue<PoolObject> queue;
			if (instance.pool.TryGetValue(obj, out queue))
			{
				if (queue.Count > 0)
				{
					GameObject go = queue.Dequeue().gameObject;
					go.transform.position = position;
					go.transform.rotation = rotation;
					go.SetActive(true);
					return go;
				}
			}
		}
		GameObject go2 = Instantiate(obj, position, rotation, instance.transform);
		go2.AddComponent<PoolObject>().prefab = obj;
		return go2;
	}

	public static void Despawn(PoolObject obj)
	{
		if (instance == null || obj.prefab == null)
		{
			Destroy(obj.gameObject);
		}
		else
		{
			Queue<PoolObject> q;
			if (instance.pool.TryGetValue(obj.prefab, out q)) {
				q.Enqueue(obj);
			}
			else
			{
				q = new Queue<PoolObject>();
				q.Enqueue(obj);
				instance.pool.Add(obj.prefab, q);
			}
		}
	}

	public static void DespawnAll()
	{
		if (instance == null)
		{
			GameObject go = new GameObject("ObjectPool");
			instance = go.AddComponent<ObjectPool>();
			return;
		}
		for (int i = 0; i < instance.transform.childCount; i++)
		{
			instance.transform.GetChild(i).gameObject.SetActive(false);
		}
	}

	public class PoolObject : MonoBehaviour
	{
		public GameObject prefab;

		private void OnDisable()
		{
			ObjectPool.Despawn(this);
		}
	}
}
