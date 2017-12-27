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

	public class PoolObject : MonoBehaviour
	{
		public GameObject prefab;
		public void Despawn()
		{
			ObjectPool.Despawn(this);
		}

		private void OnDisable()
		{
			Despawn();
		}
	}
}
