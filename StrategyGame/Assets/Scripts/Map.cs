using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Map {

	[Serializable]
	public class Tile
	{
		public Vector3 position;
		public bool goal;
		public float wall;

		//Pathfinding
		private static Tile[] pfCache = new Tile[8];
		public int pfIndex;
		public float pfPriority;
		public float pfDistance;
		public int pfIteration;
		[NonSerialized]
		public Tile[] pfNeighbours;
		[NonSerialized]
		public Tile pfPrevious;

		public Tile(int x, int y)
		{
			position = new Vector3(x, 0, y);
			goal = false;
			wall = 1;
		}

		public void SetNeighbours(Tile n, Tile ne, Tile e, Tile se, Tile s, Tile sw, Tile w, Tile nw)
		{
			int index = 0;
			if (n.wall == 0)
				pfCache[index++] = n;
			if (n.wall == 0 && ne.wall == 0 && e.wall == 0)
				pfCache[index++] = ne;
			if (e.wall == 0)
				pfCache[index++] = e;
			if (s.wall == 0 && se.wall == 0 && e.wall == 0)
				pfCache[index++] = se;
			if (s.wall == 0)
				pfCache[index++] = s;
			if (s.wall == 0 && sw.wall == 0 && w.wall == 0)
				pfCache[index++] = sw;
			if (w.wall == 0)
				pfCache[index++] = w;
			if (n.wall == 0 && nw.wall == 0 && w.wall == 0)
				pfCache[index++] = nw;
			pfNeighbours = new Tile[index];
			Array.Copy(pfCache, pfNeighbours, index);
		}
	}

	int width;
	int height;
	int iteration;
	Tile[,] map;
	List<Tile> queue;

	public Map(int width, int height)
	{
		map = new Tile[width, height];
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				map[x, y] = new Tile(x, y);
			}
		}
		this.width = width;
		this.height = height;
		queue = new List<Tile>();
		iteration = 10;
	}

	public void SetTile(int x, int y, bool goal, float wall)
	{
		map[x, y].goal = goal;
		map[x, y].wall = wall;
	}

	public Tile GetTile(int x, int y)
	{
		x = Mathf.Clamp(x, 0, width-1);
		y = Mathf.Clamp(y, 0, height-1);
		return map[x, y];
	}

	public Tile GetTile(Vector3 pos)
	{
		return GetTile(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.z));
	}

	public void CalculateNavMesh()
	{
		for (int x = 1; x < width-1; x++)
		{
			for (int y = 1; y < height - 1; y++)
			{
				map[x, y].SetNeighbours(
					map[x, y + 1],
					map[x + 1, y + 1],
					map[x + 1, y],
					map[x + 1, y - 1],
					map[x, y - 1],
					map[x - 1, y - 1],
					map[x - 1, y],
					map[x - 1, y + 1]
					);
			}
		}
	}

	public void GetPath(Vector3 start, Vector3 end, ref List<Vector3> path)
	{
		Tile goal = GetTile(end);
		if (goal.wall != 0 || end.x > width || end.x < 0 || end.z > height || end.z < 0)
			return;
		end.y = 0;
		queue.Clear();
		iteration++;
		Tile t = GetTile(start);
		if (t == goal)
		{
			path.Add(end);
			return;
		}
		t.pfDistance = 0;
		t.pfPriority = Vector3.Distance(t.position, end);
		t.pfPrevious = null;
		HeapAdd(t);
		while (queue.Count > 0)
		{
			t = HeapRemove();
			if (t == goal)
			{
				path.Add(end);
				while (t.pfPrevious != null)
				{
					path.Add(t.position);
					t = t.pfPrevious;
				}
				return;
			}
			if (t.pfNeighbours == null)
				continue;
			for (int i = 0; i < t.pfNeighbours.Length; i++)
			{
				if (t.pfNeighbours[i].pfIteration != iteration)
				{
					t.pfNeighbours[i].pfDistance = t.pfDistance + Vector3.Distance(t.position, t.pfNeighbours[i].position);
					t.pfNeighbours[i].pfPriority = t.pfNeighbours[i].pfDistance + Vector3.Distance(t.pfNeighbours[i].position, end);
					t.pfNeighbours[i].pfPrevious = t;
					HeapAdd(t.pfNeighbours[i]);
				}
				else
				{
					float dist = t.pfDistance + Vector3.Distance(t.position, t.pfNeighbours[i].position);
					if (dist < t.pfNeighbours[i].pfDistance)
					{
						t.pfNeighbours[i].pfDistance = dist;
						t.pfNeighbours[i].pfPrevious = t;
						HeapUpdate(t.pfNeighbours[i], dist + Vector3.Distance(t.pfNeighbours[i].position, end));
					}
				}
			}
		}
	}

	void HeapUp(int index)
	{
		if (index == 0)
			return;
		int root = (index - 1) / 2;
		if (queue[index].pfPriority < queue[root].pfPriority)
		{
			HeapSwap(index, root);
			HeapDown(root);
		}
	}

	void HeapDown(int index)
	{
		int branch1 = index * 2 + 1;
		int branch2 = index * 2 + 2;
		if (branch1 >= queue.Count)
			return;
		if (queue[branch1].pfPriority < queue[index].pfPriority)
		{
			if (branch2 >= queue.Count || queue[branch1].pfPriority < queue[branch2].pfPriority)
			{
				HeapSwap(branch1, index);
				HeapDown(branch1);
				return;
			}
			else
			{
				HeapSwap(branch2, index);
				HeapDown(branch2);
				return;
			}
		}
		else if (branch2 < queue.Count && queue[branch2].pfPriority < queue[index].pfPriority)
		{
			HeapSwap(branch2, index);
			HeapDown(branch2);
			return;
		}
	}

	Tile HeapRemove()
	{
		Tile t = queue[0];
		if (queue.Count == 1)
		{
			queue.Clear();
			return t;
		}
		queue[0] = queue[queue.Count - 1];
		queue[0].pfIndex = 0;
		queue.RemoveAt(queue.Count - 1);
		HeapDown(0);
		return t;
	}

	void HeapAdd(Tile t)
	{
		t.pfIndex = queue.Count;
		queue.Add(t);
		HeapUp(t.pfIndex);
		t.pfIteration = iteration;
	}

	void HeapUpdate(Tile t, float priority)
	{
		if (t.pfPriority < priority)
			HeapDown(t.pfIndex);
		else
			HeapUp(t.pfIndex);
	}

	void HeapSwap(int a, int b)
	{
		var tmp = queue[a];
		queue[a] = queue[b];
		queue[b] = tmp;
		queue[a].pfIndex = a;
		queue[b].pfIndex = b;
	}
}
