using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
public class Soldier : Agent {

	[System.Serializable]
	public struct ReplaceMaterial
	{
		public Renderer renderer;
		public int index;
	}

	[Header("Soldier")]
	public ReplaceMaterial[] teamColors;
	public GameObject bullet;
	public Transform shootingPoint;
	public float shootingInterval = 0.5f;
	public float shootingAngle = 10f;
	public float shootingRandomness = 0.05f;
	public float shootingRange = 25f;
	public Image healthBar;
	public Gradient healthColor;
	public float maxHealth = 100f;


	NavMeshAgent agent;
#pragma warning disable 0108
	Rigidbody rigidbody;
	float health;
	float shootTime;
	MLAcademy academy;
	Soldier target;
	List<float> state;
	[System.NonSerialized] public bool goal;

	public override void InitializeAgent()
	{
		agent = GetComponent<NavMeshAgent>();
		rigidbody = GetComponent<Rigidbody>();
		agent.updatePosition = false;
		health = maxHealth;
		shootTime = Time.time;
		state = new List<float>(new float[] { 0, 0, 0 });
	}
	
	public bool SetDestination(Vector3 pos)
	{
		if (agent.SetDestination(pos))
		{
			agent.isStopped = false;
			return true;
		}
		return false;
	}

	public void SetTargetDirection(float angle)
	{
		float closest = 2;
		foreach (var team in academy.teams)
		{
			if (team.brain != brain) {
				foreach (var unit in team.units)
				{
					Vector3 dir = unit.transform.position - transform.position;
					float dist = dir.magnitude / shootingRange;
					if (dist < 1 && dist < closest)
					{
						float da = Mathf.Max(Mathf.DeltaAngle(angle, VectorToAngle(dir)) - 20, 0) / 60;
						if (da < 1 && da + dist < closest)
						{
							closest = da + dist;
							target = unit;
						}
					}
				}
			}
		}
	}

	public void StopMoving()
	{
		agent.isStopped = true;
	}

	public void SetTarget(Soldier target)
	{
		Vector3 dir = target.transform.position - transform.position;
		float len = dir.magnitude;
		if(len > shootingRange)
		{
			SetDestination(target.transform.position - dir.normalized * (shootingRange - 5));
		}
		this.target = target;
	}

	void FindClosestEnemy()
	{
		float dist = shootingRange;
		foreach (var team in academy.teams)
		{
			if (team.brain != brain)
			{
				foreach (var unit in team.units)
				{
					float d = Vector3.Distance(transform.position, unit.transform.position);
					if (d < dist)
					{
						target = unit;
						dist = d;
					}
				}
			}
		}
	}

	void Shoot()
	{
		if (target.done || Vector3.Distance(transform.position, target.transform.position) > shootingRange)
		{
			target = null;
			return;
		}
		Vector3 dir = target.transform.position - transform.position;
		Quaternion rotateTarget = Quaternion.LookRotation(dir, new Vector3(0, 1, 0));
		Quaternion rotation = Quaternion.RotateTowards(rigidbody.rotation, rotateTarget, agent.angularSpeed);
		rigidbody.MoveRotation(rotation);
		transform.rotation = rotation;
		if (shootTime+shootingInterval < Time.time && Quaternion.Angle(rotation, rotateTarget) < shootingAngle)
		{
			Quaternion rnd = Quaternion.Euler(
				(Random.value-0.5f) * shootingRandomness, 
				(Random.value-0.5f) * shootingRandomness * 3, 
				(Random.value-0.5f) * shootingRandomness);
			ObjectPool.Spawn(bullet, shootingPoint.position, rotateTarget * rnd).GetComponent<Bullet>().shooter = this;
			shootTime = Time.time;
		}
	}


	public float DoDamage(float value)
	{
		health -= value;
		if (health < 0)
		{
			done = true;
			reward -= MLAcademy.REWARD_DIE;
			return MLAcademy.REWARD_KILL;
		}
		healthBar.fillAmount = health / maxHealth;
		healthBar.color = healthColor.Evaluate(1-health / maxHealth);
		float rw = health / maxHealth * MLAcademy.REWARD_HIT;
		reward -= rw;
		return rw;
	}

	public void SetTeam(Brain brain, Material color, MLAcademy academy, Camera camera)
	{
		GiveBrain(brain);
		for (int i = 0; i < teamColors.Length; i++)
		{
			var mats = teamColors[i].renderer.materials;
			mats[teamColors[i].index] = color;
			teamColors[i].renderer.materials = mats;
		}
		this.academy = academy;
		observations = new List<Camera>(new Camera[] { camera });
		academy.RegisterUnit(this);
		agent.isStopped = true;
	}


	public override List<float> CollectState()
	{
		academy.Select(this);
		state[0] = health;
		state[1] = transform.position.x;
		state[2] = transform.position.z;
		return state;
	}

	public override void AgentStep(float[] act)
	{
		if (done)
			return;
		if (goal)
			reward += MLAcademy.REWARD_GOAL;

		int action = Mathf.RoundToInt(act[0]);

		if (action > 7) //Target Command
		{
			float angle = (action - 8) * 360 / 8;
			SetTargetDirection(angle);
			agent.isStopped = true;
		}
		else if (action > 0 ) //Move Command
		{
			//Own calculations needed for fastforward training with few normal updates that the navigation usually uses
			agent.isStopped = false;
			Quaternion rotateTarget = Quaternion.Euler(0, action * 360 / 8, 0);
			float dist = agent.speed * Time.fixedDeltaTime;
			Vector3 pos = rigidbody.position + rotateTarget*Vector3.forward*dist;
			NavMeshHit hit;
			if (!NavMesh.Raycast(rigidbody.position, pos, out hit, NavMesh.AllAreas)) //Allows for multiple agent on the same position
			{
				rigidbody.MovePosition(pos);
				agent.nextPosition = pos;
				Quaternion rotation = Quaternion.RotateTowards(rigidbody.rotation, rotateTarget, agent.angularSpeed);
				rigidbody.MoveRotation(rotation);
			}
		}

		if (agent.isStopped || agent.isPathStale || !agent.hasPath)
		{
			if (target != null)
			{
				Shoot();
			}
			else if (action < 1) //No Action Command
			{
				FindClosestEnemy();
			}
		}
		else
		{
			Vector3 pos = rigidbody.position + agent.velocity * Time.fixedDeltaTime;
			rigidbody.MovePosition(pos);
			agent.nextPosition = pos;
		}
	}

	public override void AgentReset()
	{
		health = maxHealth;
		DoDamage(0);
		shootTime = Time.time;
		reward = 0;
		target = null;
		agent.Warp(transform.position);
		agent.isStopped = true;
		agent.ResetPath();
		goal = false;
	}

	public override void AgentOnDone()
	{
		academy.UnregisterUnit(this);
		transform.position = new Vector3(0, 100, 0);
	}

	float VectorToAngle(Vector3 vec)
	{
		float angle = Vector3.SignedAngle(Vector3.forward, vec, Vector3.up);
		return angle;
	}
}
