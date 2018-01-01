using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MLAcademy : Academy {

	public const float REWARD_VICTORY = 1.0f;
	public const float REWARD_HIT = 0.1f;
	public const float REWARD_DIE = 0.5f;
	public const float REWARD_KILL = 0.2f;
	public const float REWARD_GOAL = 0.005f;

	[Header("Brains")]
	public Brain playerOne;
	public Brain playerTwo;
	public Brain[] humanBrains;
	public Brain[] externalBrains;
	public Brain[] internalBrains;

	[Header("Level")]
	public Texture2D map;
	public LevelGenerator generator;
	public int width = 60;
	public int height = 30;
	public Transform aiMarker;

	[System.NonSerialized] public List<Soldier> units;

	float playerOneScore;
	float playerTwoScore;

	public override void InitializeAcademy()
	{
		units = new List<Soldier>();
	}

	public override void AcademyReset()
	{
		generator.Despawn();
		//TODO resetParameters["Difficulty"]
		float players = 2f;
		if(!resetParameters.TryGetValue("Players", out players))
		{
			players = 2f;
		}
		switch(Mathf.RoundToInt(players))
		{
			case 0:
				playerOne = externalBrains[0];
				playerTwo = externalBrains[1];
				break;
			case 1:
				playerOne = humanBrains[0];
				playerTwo = externalBrains[0];
				break;
			case 2:
				playerOne = humanBrains[0];
				playerTwo = humanBrains[1];
				break;
		}
		units.Clear();
		generator.Generate(map, width, height, this);
		playerOneScore = 0;
		playerTwoScore = 0;
	}

	public void RegisterUnit(Soldier unit)
	{
		units.Add(unit);
	}

	public void UnregisterUnit(Soldier unit)
	{
		units.Remove(unit);
		if (units.Count == 0)
			done = true;
		//Find Winner
		bool both = false;
		for (int i = 1; i < units.Count; i++)
		{
			both = both || units[i].brain != units[0].brain;
		}
		if (!both)
		{
			done = true;
			//Give victory rewards
			if (units[0].brain == playerOne)
			{
				foreach (var item in playerOne.agents)
				{
					item.Value.reward += REWARD_VICTORY;
				}
				foreach (var item in playerTwo.agents)
				{
					item.Value.reward -= REWARD_VICTORY;
				}
			}
			else
			{
				foreach (var item in playerOne.agents)
				{
					item.Value.reward -= REWARD_VICTORY;
				}
				foreach (var item in playerTwo.agents)
				{
					item.Value.reward += REWARD_VICTORY;
				}
			}
		}
	}

	public void Select(Soldier selected)
	{
		aiMarker.transform.position = new Vector3(selected.transform.position.x, aiMarker.transform.position.y, selected.transform.position.z);
	}

	public override void AcademyStep()
	{
		for (int i = 0; i < units.Count; i++)
		{
			if (units[i].goals > 0)
			{
				if (units[i].brain == playerOne)
					playerOneScore += Time.fixedDeltaTime;
				else
					playerTwoScore += Time.fixedDeltaTime;
				units[i].reward += REWARD_GOAL;
			}
		}
		//TODO show and check scoring
	}

}
