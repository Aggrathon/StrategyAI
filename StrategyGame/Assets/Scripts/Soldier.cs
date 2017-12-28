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

	public ReplaceMaterial[] teamColors;
	
	NavMeshAgent agent;
	new Rigidbody rigidbody;
	List<float> state;

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
	}

	private void FixedUpdate()
	{
		rigidbody.MovePosition(rigidbody.position + agent.desiredVelocity*Time.fixedDeltaTime);
		if (agent.isStopped || agent.isPathStale)
		{
			//Look for enemy to shoot
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

	}

	public override void AgentOnDone()
	{

	}
}
