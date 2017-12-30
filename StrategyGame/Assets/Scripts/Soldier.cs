using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
public class Soldier : Agent {

	[System.Serializable]
	public struct ReplaceMaterial
	{
		public Renderer renderer;
		public int index;
	}
	public struct Target
	{
		public Soldier unit;
		public float priority;
		public Vector3 target;
		public Target UpdatePriority(float pri) { this.priority = pri; return this; }
		public Target UpdatePriority(float pri, Vector3 tar) { this.priority = pri; this.target = tar; return this; }
	}

	[Header("Soldier")]
	public ReplaceMaterial[] teamColors;
	public GameObject bullet;
	public Transform shootingPoint;
	public float shootingInterval = 0.5f;
	public float shootingAngle = 10f;
	public float shootingRandomness = 0.05f;
	public float maxHealth = 100f;
	public float shootingDistance = 30f;

	
	NavMeshAgent agent;
	new Rigidbody rigidbody;
	[System.NonSerialized] public float health;
	float shootTime;
	GameState state;
	List<Target> enemies;

	public override void InitializeAgent()
	{
		agent = GetComponent<NavMeshAgent>();
		rigidbody = GetComponent<Rigidbody>();
		agent.updatePosition = false;
		health = maxHealth;
		shootTime = Time.time;
		enemies = new List<Target>();
	}

	private void FixedUpdate()
	{
		if (agent.isStopped || agent.isPathStale || !agent.hasPath)
		{
			ShootClosestEnemy();
		}
		else
		{
			rigidbody.MovePosition(rigidbody.position + agent.desiredVelocity * Time.fixedDeltaTime);
		}
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

	public void StopMoving()
	{
		agent.isStopped = true;
	}

	void ShootClosestEnemy()
	{
		if (enemies.Count == 0)
		{
			for (int i = 0; i < state.units.Count; i++)
			{
				if (state.units[i].unit.brain != brain)
					enemies.Add(new Target() { unit = state.units[i].unit, priority = Vector3.Distance(transform.position, state.units[i].unit.transform.position) / shootingDistance });
			}
			for (int i = 1; i < enemies.Count; i++)
			{
				if (enemies[i].priority < enemies[0].priority)
				{
					var tmp = enemies[i];
					enemies[i] = enemies[0];
					enemies[0] = tmp;
				}
			}
			return;
		}
		enemies[0] = enemies[0].UpdatePriority(2f);
		for (int i = 0; i < enemies.Count; i++)
		{
			if (enemies[i].unit.done)
			{
				enemies[i] = enemies[enemies.Count - 1];
				enemies.RemoveAt(enemies.Count - 1);
			}
			Vector3 pos = enemies[0].unit.transform.position;
			Vector3 dir = pos - transform.position;
			float dist = dir.magnitude / shootingDistance;
			dir = dir.normalized;
			if (dist < 1 && dist < enemies[0].priority)
			{
				float hits = 0f;
				Vector3 target = new Vector3();
				pos.y += 0.5f;
				Vector3 left = Vector3.Cross(dir, Vector3.up).normalized;
				RaycastHit hit;
				Vector3 start = shootingPoint.position + dir * 0.7f;
				for (int x = -1; x < 2; x++)
				{
					for (int y = -1; y < 2; y++)
					{
						if (Physics.Raycast(start, -start + pos + x * left * 0.2f + Vector3.up * 0.4f * y, out hit, shootingDistance))
						{
							if (hit.rigidbody != null && hit.rigidbody.gameObject == enemies[i].unit.gameObject)
							{
								hits++;
								target += hit.point;
							}
						}
					}
				}
				if (hits == 0)
					continue;
				float vis = 1f - hits / 9f;
				vis *= vis;
				enemies[i] = enemies[i].UpdatePriority(dist + vis, target / hits);
				if (enemies[i].priority < enemies[0].priority && i != 0)
				{
					var tmp = enemies[i];
					enemies[i] = enemies[0];
					enemies[0] = tmp;
				}
			}
		}
		if (enemies[0].priority < 2)
		{
			Shoot(enemies[0].target);
		}
	}

	public void Shoot(Vector3 target)
	{
		Vector3 dir = target - shootingPoint.position;
		dir.y = 0;
		if (dir.sqrMagnitude < 0.1f)
			return;
		Quaternion rotateTarget = Quaternion.LookRotation(dir, new Vector3(0, 1, 0));
		Quaternion rotation = Quaternion.RotateTowards(transform.rotation, rotateTarget, agent.angularSpeed);
		rigidbody.MoveRotation(rotation);
		transform.rotation = rotation;
		if (shootTime+shootingInterval < Time.time && Quaternion.Angle(rotation, rotateTarget) < shootingAngle)
		{
			dir = shootingPoint.forward * dir.magnitude  + new Vector3(0, target.y - shootingPoint.position.y, 0);
			Quaternion rot = Quaternion.Lerp(Quaternion.LookRotation(dir, Vector3.up), Random.rotation, shootingRandomness)*Quaternion.Euler(0, Random.value*shootingRandomness*200, 0);
			ObjectPool.Spawn(bullet, shootingPoint.position, rot);
			shootTime = Time.time;
		}
	}

	public void SetTeam(Brain brain, Material color, GameState state)
	{
		GiveBrain(brain);
		for (int i = 0; i < teamColors.Length; i++)
		{
			var mats = teamColors[i].renderer.materials;
			mats[teamColors[i].index] = color;
			teamColors[i].renderer.materials = mats;
		}
		this.state = state;
	}


	public override List<float> CollectState()
	{
		state.Select(this);
		return new List<float>(state.map);
	}

	public override void AgentStep(float[] act)
	{
		int action = Mathf.RoundToInt(act[0]);
		if (action > 7)
			StopMoving();
		else if (action > 0)
		{
			SetDestination(transform.position + Quaternion.Euler(0, action * 360 / 8, 0) * Vector3.forward*0.5f);
		}
		//reward -= 0.01f;
	}

	public override void AgentReset()
	{
		health = maxHealth;
		shootTime = Time.time;
		reward = 0;
		enemies.Clear();
	}

	public override void AgentOnDone()
	{
		reward -= 1;
	}

	float VectorToAngle(Vector3 vec)
	{
		float angle = Vector3.SignedAngle(Vector3.forward, vec, Vector3.up);
		if (angle < 0) angle += 360;
		return angle;
	}
}
