using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Brain))]
public class PlayerBrain : MonoBehaviour {

	public enum InputType
	{
		mouse,
		numpad,
		controller
	}

	public InputType inputMethod = InputType.mouse;
	public GameObject marker;
	public LayerMask raycastMask;

	Soldier selected = null;
	Brain brain;

	private void Start()
	{
		brain = GetComponent<Brain>();
		marker.SetActive(false);
	}

	private void Update()
	{
		if (selected != null)
		{
			if (selected.done || !selected.gameObject.activeSelf)
			{
				marker.SetActive(false);
				selected = null;
			}
		}
		switch (inputMethod)
		{
			case InputType.mouse:
				HandleMouseInput();
				break;
			case InputType.numpad:
				HandleNumpadInput();
				break;
			case InputType.controller:
				HandleControllerInput();
				break;
		}
		if (selected)
		{
			marker.transform.position = new Vector3(selected.transform.position.x, marker.transform.position.y, selected.transform.position.z);
		}
	}

	void HandleNumpadInput()
	{
		enabled = false;
		Debug.LogWarning("Numpad input is not implemented");
	}

	void HandleControllerInput()
	{
		enabled = false;
		Debug.LogWarning("Controller input is not implemented");
	}

	void HandleMouseInput()
	{
		if (Input.GetMouseButtonUp(0))
		{
			RaycastHit hit;
			if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100f, raycastMask.value))
			{
				if (hit.rigidbody == null)
				{
					if(selected != null)
						selected.SetDestination(hit.point);
					return;
				}
				Soldier sel = hit.rigidbody.GetComponent<Soldier>();
				if (sel != null) {
					if (sel.brain != brain)
					{
						if (selected != null)
							selected.SetTarget(sel);
					}
					else
					{
						if (selected == sel)
						{
							sel.StopMoving();
						}
						else
						{
							Select(sel);
						}
					}
				}
				else
				{
					selected.SetDestination(hit.point);
				}
			}
		}
	}

	void Select(Soldier sel)
	{
		selected = sel;
		marker.transform.position = new Vector3(selected.transform.position.x, marker.transform.position.y, selected.transform.position.z);
		marker.gameObject.SetActive(true);
	}
}
