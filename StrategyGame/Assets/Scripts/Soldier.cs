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
	[SerializeField] float maxHealth = 100f;

	
	NavMeshAgent agent;
	new Rigidbody rigidbody;
	List<float> state;
	[System.NonSerialized] public float health;
	float shootTime;

	public override void InitializeAgent()
	{
		agent = GetComponent<NavMeshAgent>();
		rigidbody = GetComponent<Rigidbody>();
		agent.updatePosition = false;
		state = new List<float>(12);
		for (int i = 0; i < 12; i++)
		{
			state.Add(0);
		}
		health = maxHealth;
		shootTime = Time.time;
	}

	private void FixedUpdate()
	{
		rigidbody.MovePosition(rigidbody.position + agent.desiredVelocity*Time.fixedDeltaTime);
		if (agent.isStopped || agent.isPathStale || !agent.hasPath)
		{
			//Look for enemy to shoot
			Shoot(transform.position + shootingPoint.forward * 100f);
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

	public void Shoot(Vector3 target)
	{
		Quaternion rotateTarget = Quaternion.LookRotation(new Vector3(target.x - transform.position.x, 0, target.z - transform.position.z), Vector3.up);
		rigidbody.MoveRotation(Quaternion.RotateTowards(rigidbody.rotation, rotateTarget, agent.angularSpeed));
		if (shootTime+shootingInterval < Time.time && Quaternion.Angle(rigidbody.rotation, rotateTarget) < shootingAngle)
		{
			Vector3 dir = transform.forward * Mathf.Sqrt((target.x - shootingPoint.position.x) * (target.x - shootingPoint.position.x) + (target.y - shootingPoint.position.y)) + new Vector3(0, target.y - shootingPoint.position.y, 0);
			Quaternion rot = Quaternion.Lerp(Quaternion.LookRotation(dir, Vector3.up), Random.rotation, shootingRandomness);
			ObjectPool.Spawn(bullet, shootingPoint.position, rot);
			shootTime = Time.time;
		}
	}

	public void SetTeam(Brain brain, Material color)
	{
		GiveBrain(brain);
		for (int i = 0; i < teamColors.Length; i++)
		{
			var mats = teamColors[i].renderer.materials;
			mats[teamColors[i].index] = color;
			teamColors[i].renderer.materials = mats;
		}
	}


	public override List<float> CollectState()
	{
		return state;
	}

	public override void AgentStep(float[] act)
	{

	}

	public override void AgentReset()
	{
		health = maxHealth;
		shootTime = Time.time;
	}

	public override void AgentOnDone()
	{

	}
}
