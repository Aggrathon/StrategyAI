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
		AIvsAI = 0,
		HumanvsAI = 1,
		HumanvsHuman = 2
	}

	[Header("Brains")]
	public Competitiors defaultCompetitors = Competitiors.HumanvsHuman;
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
	public int MaxSteps { get { return maxSteps; } }

	public override void InitializeAcademy()
	{
		teams = new List<Team>();
	}

	public override void AcademyReset()
	{
		generator.Despawn();
		//TODO resetParameters["Difficulty"]
		teams.Clear();
		switch((Competitiors)Utils.GetDictionaryIntDefault<string>(resetParameters, "Players", (int)defaultCompetitors))
		{
			case Competitiors.AIvsAI:
				teams.Add(new Team(externalBrains[0]));
				teams.Add(new Team(externalBrains[1]));
				break;
			case Competitiors.HumanvsAI:
				teams.Add(new Team(humanBrains[0]));
				teams.Add(new Team(externalBrains[0]));
				break;
			case Competitiors.HumanvsHuman:
				teams.Add(new Team(humanBrains[0]));
				teams.Add(new Team(humanBrains[1]));
				break;
			default:
				resetParameters["Players"] = (int)defaultCompetitors;
				AcademyReset();
				return;
		}
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
				if (teams[i].units[j].goals > 0)
				{
					teams[i].score += score;
					teams[i].units[j].reward += REWARD_GOAL;
				}
				if (teams[i].score > 1f)
				{
					//Won
					for (int k = 0; k < teams.Count; k++)
					{
						if (i == k)
							foreach (var item in teams[k].brain.agents)
								item.Value.reward += REWARD_VICTORY;
						else
							foreach (var item in teams[k].brain.agents)
								item.Value.reward -= REWARD_VICTORY;
					}
					done = true;
				}
			}
		}
	}

}
