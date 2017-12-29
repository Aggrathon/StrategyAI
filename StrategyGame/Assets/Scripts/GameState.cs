using System;
using System.Collections.Generic;
using UnityEngine;

public class GameState
{
	public struct UnitPosition
	{
		public Soldier unit;
		public int position;
		public int player;

		public void UpdatePosition(int newPos) { position = newPos; }
	}

	public int width;
	public int height;

	public Vector3 offset;

	public List<float> map;
	public List<UnitPosition> units;

	int selected;

	public GameState(int width, int height, Vector3 offset, int additionalData = 0)
	{
		map = new List<float>(width * height * 5 + additionalData + 3);
		for (int i = 0; i < width*height; i++)
		{
			map.Add(1); //Walls
			map.Add(0); //Goal
			map.Add(0); //PlayerOne
			map.Add(0); //PlayerTwo
			map.Add(0); //Selected
		}
		for (int i = 0; i < additionalData; i++)
		{
			map.Add(0);
		}
		map.Add(0); //Selected X
		map.Add(0); //Selected Y
		map.Add(0); //Selected Health

		selected = 4;
		this.width = width;
		this.height = height;
		this.offset = offset;
		units = new List<UnitPosition>();
	}

	public void Select(Soldier s)
	{
		map[selected] = 0;
		selected = CalculatePosition(s.transform.position) + 4;
		map[selected] = 1;
		map[map.Count - 3] = s.transform.position.x - offset.x;
		map[map.Count - 2] = s.transform.position.z - offset.z;
		map[map.Count - 1] = s.health / s.maxHealth;
	}

	public void SetTile(int x, int y, float wall=0, float goal=0)
	{
		int index = y * width * 5 + x * 5;
		map[index] = wall;
		map[index + 1] = goal;
	}

	public void AddUnit(Soldier unit, int player=0)
	{
		int index = CalculatePosition(unit.transform.position) + 2 + player;
		map[index] = 1;
		units.Add(new UnitPosition() { unit=unit, position=index, player=player });
	}

	public void UpdatePositions()
	{
		for (int i = 0; i < units.Count; i++)
		{
			map[units[i].position] = 0;
		}
		units.RemoveAll(u => u.unit.done);

		for (int i = 0; i < units.Count; i++)
		{
			int index = CalculatePosition(units[i].unit.transform.position) + 2 + units[i].player;
			map[units[i].position] = 0;
			map[index] = 0.5f + units[i].unit.health / units[i].unit.maxHealth * 0.5f;
			units[i].UpdatePosition(index);
		}
	}

	int CalculatePosition(Vector3 pos)
	{
		pos -= offset;
		int x = Mathf.RoundToInt(pos.x);
		int y = Mathf.RoundToInt(pos.z);
		return CalculatePosition(x, y);
	}

	int CalculatePosition(int x, int y)
	{
		return x * width * 5 + y * 5;
	}
}
