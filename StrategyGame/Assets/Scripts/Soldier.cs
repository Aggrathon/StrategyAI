using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
public class Soldier : Agent {

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
