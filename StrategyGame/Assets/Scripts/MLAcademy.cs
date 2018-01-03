using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MLAcademy : Academy {

	public const float REWARD_VICTORY = 1.0f;
	public const float REWARD_HIT = 0.1f;
	public const float REWARD_DIE = 0.5f;
	public const float REWARD_KILL = 0.2f;
	public const float REWARD_GOAL = 0.005f;

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
		AiVsAi = 0,
		HumanVsAi = 1,
		HumanVsHuman = 2
	}

	[Header("Brains")]
	public Competitiors defaultCompetitors = Competitiors.HumanVsHuman;
	public Brain[] humanBrains;
	public Brain[] externalBrains;
	public Brain[] internalBrains;

	[Header("Level")]
	public Texture2D map;
	public LevelGenerator generator;
	public int width = 60;
	public int height = 30;
	public Transform aiMarker;
	public float goalTime = 15f;

	public List<Team> teams;
	public HashSet<int> goalCache;
	public int MaxSteps { get { return maxSteps; } }

	public override void InitializeAcademy()
	{
		teams = new List<Team>();
		goalCache = new HashSet<int>();
	}

	public override void AcademyReset()
	{
		generator.Despawn();
		//TODO resetParameters["Difficulty"]
		teams.Clear();
		switch((Competitiors)Utils.GetDictionaryIntDefault<string>(resetParameters, "Player", (int)defaultCompetitors))
		{
			case Competitiors.AiVsAi:
				teams.Add(new Team(externalBrains[0]));
				teams.Add(new Team(externalBrains[1]));
				break;
			case Competitiors.HumanVsAi:
				teams.Add(new Team(humanBrains[0]));
				teams.Add(new Team(externalBrains[0]));
				break;
			case Competitiors.HumanVsHuman:
				teams.Add(new Team(humanBrains[0]));
				teams.Add(new Team(humanBrains[1]));
				break;
			default:
				resetParameters["Players"] = (int)defaultCompetitors;
				AcademyReset();
				return;
		}
		goalCache.Clear();
		generator.Generate(map, width, height, this);
	}

	public void RegisterUnit(Soldier unit)
	{
		for (int i = 0; i < teams.Count; i++)
		{
			if (teams[i].brain == unit.brain)
			{
				teams[i].units.Add(unit);
				break;
			}
		}
	}

	public void UnregisterUnit(Soldier unit)
	{
		for (int i = 0; i < teams.Count; i++)
		{
			if (teams[i].brain == unit.brain)
			{
				teams[i].units.Remove(unit);
				if (teams[i].units.Count == 0)
				{
					//Lost
					for (int j = 0; j < teams.Count; j++)
					{
						if (i==j)
							foreach (var item in teams[j].brain.agents)
								item.Value.reward -= REWARD_VICTORY;
						else
							foreach (var item in teams[j].brain.agents)
								item.Value.reward += REWARD_VICTORY;
					}
					done = true;
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
		float score = Time.fixedDeltaTime / goalTime;
		for (int i = 0; i < teams.Count; i++)
		{
			for (int j = 0; j < teams[i].units.Count; j++)
			{
				teams[i].units[j].goal = goalCache.Contains(Utils.Float2Hash(teams[i].units[j].transform.position.x, teams[i].units[j].transform.position.z));
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
