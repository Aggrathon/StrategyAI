using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

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
	public Image healthBar;
	public Gradient healthColor;
	public float maxHealth = 100f;

	[Header("Shooting")]
	public GameObject bullet;
	public Transform shootingPoint;
	public float shootingInterval = 0.5f;
	public float shootingAngle = 10f;
	public float shootingRandomness = 0.05f;
	public float shootingRange = 25f;

	[Header("Movement")]
	public float angularSpeed = 200f;
	public float speed = 3f;

	
#pragma warning disable 0108
	Rigidbody rigidbody;
	float health;
	float shootTime;
	MLAcademy academy;
	[System.NonSerialized] public MLAcademy.Team team;
	Soldier target;
	List<float> state;
	List<Vector3> path;
	[System.NonSerialized] public bool goal;


#region initialize

	public override void InitializeAgent()
	{
		rigidbody = GetComponent<Rigidbody>();
		health = maxHealth;
		shootTime = Time.time;
		state = new List<float>(new float[] { 0, 0, 0, 0 });
		path = new List<Vector3>();
	}

	public override void AgentReset()
	{
		health = maxHealth;
		healthBar.fillAmount = health / maxHealth;
		healthBar.color = healthColor.Evaluate(1 - health / maxHealth);
		shootTime = Time.time;
		target = null;
		path.Clear();
		goal = false;
		done = false;
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
		team = academy.RegisterUnit(this);
		AgentReset();
	}

	public override void AgentOnDone()
	{
		academy.UnregisterUnit(this);
	}

#endregion

#region moving
	private void FixedUpdate()
	{
		if (done)
			return;
		if (path.Count > 0)
		{
			Vector3 dir = path[path.Count - 1] - rigidbody.position;
			float mag = dir.magnitude;
			if (path.Count != 1 && mag < 0.3)
			{
				path.RemoveAt(path.Count - 1);
				dir = path[path.Count - 1] - rigidbody.position;
				mag = dir.magnitude;
			}
			if (path.Count > 1 || mag > speed * Time.fixedDeltaTime)
			{
				Vector3 pos = rigidbody.position + dir * (speed * Time.fixedDeltaTime / mag);
				Quaternion rotation = Quaternion.RotateTowards(rigidbody.rotation, Quaternion.LookRotation(dir, Vector3.up), angularSpeed);
				rigidbody.MovePosition(pos);
				rigidbody.MoveRotation(rotation);
				return;
			}
		}
		if (target != null)
		{
			Shoot();
		}
		else if (brain.brainType == BrainType.Player)
		{
			FindClosestEnemy();
		}
	}

	public void SetDestination(Vector3 pos)
	{
		path.Clear();
		academy.map.GetPath(rigidbody.position, pos, ref path);
	}

	public void StopMoving()
	{
		path.Clear();
	}
#endregion

#region shooting

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
		Quaternion rotation = Quaternion.RotateTowards(rigidbody.rotation, rotateTarget, angularSpeed);
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
			AgentReset();
			rigidbody.MovePosition(new Vector3(1000, 1000, 1000));
			done = true;
			reward -= MLAcademy.REWARD_DIE;
			return MLAcademy.REWARD_KILL;
		}
		else
		{
			healthBar.fillAmount = health / maxHealth;
			healthBar.color = healthColor.Evaluate(1 - health / maxHealth);
			float rw = health / maxHealth * MLAcademy.REWARD_HIT;
			reward -= rw;
			return rw;
		}
	}

	float VectorToAngle(Vector3 vec)
	{
		float angle = Vector3.SignedAngle(Vector3.forward, vec, Vector3.up);
		return angle;
	}

	#endregion

#region action

	public override List<float> CollectState()
	{
		academy.Select(this);
		state[0] = health;
		state[1] = transform.position.x;
		state[2] = transform.position.z;
		state[3] = team.score;
		return state;
	}

	public override void AgentStep(float[] act)
	{
		if (done)
			return;
		if (goal)
			reward += MLAcademy.REWARD_GOAL;
		else
			reward -= MLAcademy.REWARD_CONSTANT_PENALTY;

		int action = Mathf.RoundToInt(act[0]);
		if (action > 7) //Target Command
		{
			float angle = (action - 8) * 360 / 8;
			SetTargetDirection(angle);
			if (target != null)
				path.Clear();
		}
		else if (action >= 0 ) //Move Command
		{
			Quaternion rotateTarget = Quaternion.Euler(0, action * 360 / 8, 0);
			Vector3 dir = rotateTarget * Vector3.forward * 0.5f;
			SetDestination(rigidbody.position + dir);
		}
		else if (brain.brainType != BrainType.Player)
		{
			Debug.LogError("Cannot give "+act[0]+" as an action when the brain is not human!");
			act[0] = Random.Range(0.0f, brain.brainParameters.actionSize);
			for (int i = 0; i < teamColors.Length; i++)
			{
				var mats = teamColors[i].renderer.materials;
				mats[teamColors[i].index].color = Color.black;
				teamColors[i].renderer.materials = mats;
			}
		}
	}

#endregion

}
