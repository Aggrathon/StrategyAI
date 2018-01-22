using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MLAcademy : Academy {

	public const float REWARD_VICTORY = 1.0f;
	public const float REWARD_HIT = 0.1f;
	public const float REWARD_DIE = 0.5f;
	public const float REWARD_KILL = 0.2f;
	public const float REWARD_GOAL = 0.005f;
	public const float REWARD_CONSTANT_PENALTY = 0.0001f;

	[System.Serializable]
	public class Team
	{
		public List<Soldier> units;
		public float score;
		public Brain brain;
		public Team(Brain b)
		{
			score = 0;
			brain = b;
			units = new List<Soldier>();
		}
	}

	public enum Competitiors
	{
		AI1 = 0,
		AI2 = 1,
		Human1 = 2,
		Human2 = 3,
		Random1 = 4,
		Random2 = 5,
	}

	[Header("Brains")]
	public Competitiors defaultPlayerOne = Competitiors.Human1;
	public Competitiors defaultPlayerTwo = Competitiors.Random1;
	public Brain[] humanBrains;
	public Brain[] externalBrains;
	public Brain[] internalBrains;
	public Brain[] randomBrains;

	[Header("Level")]
	public MapList[] levels;
	public int defaultLevel = 3;
	public LevelGenerator generator;
	public int width = 60;
	public int height = 30;
	public Transform aiMarker;
	public float goalTime = 15f;

	[System.NonSerialized] public List<Team> teams;
	[System.NonSerialized] public Map map;
	public int MaxSteps { get { return maxSteps; } }

	public override void InitializeAcademy()
	{
		teams = new List<Team>();
		resetParameters["PlayerOne"] = (int)defaultPlayerOne;
		resetParameters["PlayerTwo"] = (int)defaultPlayerTwo;
		resetParameters["Difficulty"] = defaultLevel;
	}

	public override void AcademyReset()
	{
		generator.Despawn();
		teams.Clear();
		switch ((Competitiors)Utils.GetDictionaryIntDefault<string>(resetParameters, "PlayerOne", (int)defaultPlayerOne))
		{
			case Competitiors.AI1:
				teams.Add(new Team(externalBrains[0]));
				break;
			case Competitiors.AI2:
				teams.Add(new Team(externalBrains[1]));
				break;
			case Competitiors.Human1:
				teams.Add(new Team(humanBrains[0]));
				break;
			case Competitiors.Human2:
				teams.Add(new Team(humanBrains[1]));
				break;
			case Competitiors.Random1:
				teams.Add(new Team(randomBrains[0]));
				break;
			case Competitiors.Random2:
				teams.Add(new Team(randomBrains[1]));
				break;
			default:
				resetParameters["PlayerOne"] = (int)defaultPlayerOne;
				AcademyReset();
				return;
		}
		switch ((Competitiors)Utils.GetDictionaryIntDefault<string>(resetParameters, "PlayerTwo", (int)defaultPlayerTwo))
		{
			case Competitiors.AI1:
				teams.Add(new Team(externalBrains[0]));
				break;
			case Competitiors.AI2:
				teams.Add(new Team(externalBrains[1]));
				break;
			case Competitiors.Human1:
				teams.Add(new Team(humanBrains[0]));
				break;
			case Competitiors.Human2:
				teams.Add(new Team(humanBrains[1]));
				break;
			case Competitiors.Random1:
				teams.Add(new Team(randomBrains[0]));
				break;
			case Competitiors.Random2:
				teams.Add(new Team(randomBrains[1]));
				break;
			default:
				resetParameters["PlayerTwo"] = (int)defaultPlayerTwo;
				AcademyReset();
				return;
		}
		var level = levels[Mathf.Clamp(Utils.GetDictionaryIntDefault<string>(resetParameters, "Difficulty", defaultLevel), 0, levels.Length-1)].GetMap();
		map = generator.Generate(level, width, height, this);
	}

	public Team RegisterUnit(Soldier unit)
	{
		for (int i = 0; i < teams.Count; i++)
		{
			if (teams[i].brain == unit.brain)
			{
				teams[i].units.Add(unit);
				return teams[i];
			}
		}
		return null;
	}

	public void UnregisterUnit(Soldier unit)
	{
		unit.team.units.Remove(unit);
		if (unit.team.units.Count == 0)
		{
			//Lost
			for (int j = 0; j < teams.Count; j++)
			{
				if (unit.team != teams[j])
					Winner(teams[j]);
			}
		}
	}

	public void Select(Soldier selected)
	{
		aiMarker.transform.position = new Vector3(selected.transform.position.x, aiMarker.transform.position.y, selected.transform.position.z);
	}

	public override void AcademyStep()
	{
		float score = Time.fixedDeltaTime / goalTime;
		for (int i = 0; i < teams.Count; i++)
		{
			for (int j = 0; j < teams[i].units.Count; j++)
			{
				teams[i].units[j].goal = map.GetTile(teams[i].units[j].transform.position).goal;
			}
			for (int j = 0; j < teams[i].units.Count; j++)
			{
				if (teams[i].units[j].goal)
				{
					teams[i].score += score;
					if (teams[i].score > 1f)
					{
						Winner(teams[i]);
					}
					break;
				}
			}
		}
	}

	void Winner(Team team)
	{
		for (int i = 0; i < teams.Count; i++)
		{
			if (teams[i] == team)
				foreach (var item in teams[i].brain.agents)
					item.Value.reward += REWARD_VICTORY;
			else
				foreach (var item in teams[i].brain.agents)
					item.Value.reward -= REWARD_VICTORY;
		}
		done = true;
	}

}
