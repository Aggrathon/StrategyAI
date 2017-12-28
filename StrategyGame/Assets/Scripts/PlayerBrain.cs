using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerBrain : MonoBehaviour {

	public GameObject marker;

	NavMeshAgent selected = null;

	private void Start()
	{
		marker.SetActive(false);
	}

	private void Update()
	{
		if (Input.GetMouseButtonUp(0))
		{
			RaycastHit hit;
			if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100f))
			{
				if (hit.rigidbody != null)
				{
					selected = hit.rigidbody.GetComponent<NavMeshAgent>();
					marker.SetActive(true);
				}
				else if (selected != null)
				{
					if (selected.SetDestination(hit.point))
					{
						Debug.Log("Path set");
						selected = null;
						marker.SetActive(false);
					}
					else
					{
						Debug.Log("Path not found");
					}
				}
			}
		}
		if (selected)
		{
			marker.transform.position = new Vector3(selected.transform.position.x, marker.transform.position.y, selected.transform.position.z);
		}
	}
}
