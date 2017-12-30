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
	MLAcademy academy;
	Soldier target;
	List<float> state;

	public override void InitializeAgent()
	{
		agent = GetComponent<NavMeshAgent>();
		rigidbody = GetComponent<Rigidbody>();
		agent.updatePosition = false;
		health = maxHealth;
		shootTime = Time.time;
		state = new List<float>(new float[] { 0, 0, 0 });
	}

	private void FixedUpdate()
	{
		if (agent.isStopped || agent.isPathStale || !agent.hasPath || agent.velocity.sqrMagnitude < 0.01f)
		{
			if (target != null)
				Shoot();
			else
			{
				target = FindClosestEnemy();
				if (target != null)
					Shoot();
				target = null;
			}
		}
		else
		{
			rigidbody.MovePosition(rigidbody.position + agent.velocity * Time.fixedDeltaTime);
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

	public void SetTarget(Soldier target)
	{
		this.target = target;
	}

	Soldier FindClosestEnemy()
	{
		float dist = shootingDistance;
		Soldier target = null;
		for (int i = 1; i < academy.units.Count; i++)
		{
			var unit = academy.units[i];
			if (unit.brain != brain)
			{
				float d = Vector3.Distance(transform.position, unit.transform.position);
				if (d < dist)
				{
					target = unit;
					dist = d;
				}
			}
		}
		return target;
	}

	void Shoot()
	{
		Vector3 dir = target.transform.position - transform.position;
		Quaternion rotateTarget = Quaternion.LookRotation(dir, new Vector3(0, 1, 0));
		Quaternion rotation = Quaternion.RotateTowards(transform.rotation, rotateTarget, agent.angularSpeed);
		rigidbody.MoveRotation(rotation);
		transform.rotation = rotation;
		if (shootTime+shootingInterval < Time.time && Quaternion.Angle(rotation, rotateTarget) < shootingAngle)
		{
			Quaternion rnd = Quaternion.Euler(
				(Random.value-0.5f) * shootingRandomness, 
				(Random.value-0.5f) * shootingRandomness * 3, 
				(Random.value-0.5f) * shootingRandomness);
			ObjectPool.Spawn(bullet, shootingPoint.position, rotateTarget * rnd);
			shootTime = Time.time;
		}
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
		target = null;
	}

	public override void AgentOnDone()
	{
		reward -= 1;
		academy.UnregisterUnit(this);
		gameObject.SetActive(false);
	}

	float VectorToAngle(Vector3 vec)
	{
		float angle = Vector3.SignedAngle(Vector3.forward, vec, Vector3.up);
		if (angle < 0) angle += 360;
		return angle;
	}
}
