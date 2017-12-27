using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
public class Soldier : MonoBehaviour {

	bool selected = false;
	NavMeshAgent agent;
	//new Rigidbody rigidbody;

	private void Awake()
	{
		agent = GetComponent<NavMeshAgent>();
		//rigidbody = GetComponent<Rigidbody>();
	}
	

	private void Update()
	{
		if (Input.GetMouseButtonUp(0))
		{
			RaycastHit hit;
			if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100f))
			{
				if (hit.rigidbody != null && hit.rigidbody.gameObject == gameObject)
				{
					Debug.Log(name + " is selected");
					selected = true;
				}
				else if (selected)
				{
					if (hit.rigidbody != null)
					{
						selected = false;
					}
					else if (agent.SetDestination(hit.point))
					{
						Debug.Log("Path set");
						selected = false;
					}
					else
					{
						Debug.Log("Path not find");
					}
				}
			}
		}
	}
}
