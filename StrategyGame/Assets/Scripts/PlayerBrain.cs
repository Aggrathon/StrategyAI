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

	Soldier selected = null;
	Brain brain;

	private void Start()
	{
		brain = GetComponent<Brain>();
		marker.SetActive(false);
	}

	private void Update()
	{
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
			if (!selected.gameObject.activeSelf)
			{
				marker.SetActive(false);
				selected = null;
			}
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
			if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100f))
			{
				Soldier sel = null;
				if (hit.rigidbody != null)
				{
					sel = hit.rigidbody.GetComponent<Soldier>();
					if (sel.brain != brain)
						sel = null;
				}
				if (sel != null)
				{
					marker.SetActive(true);
					if (selected == sel)
					{
						sel.StopMoving();
					}
					else
					{
						selected = sel;
					}
				}
				else if (selected != null)
				{
					selected.SetDestination(hit.point);
				}
			}
		}
	}
}
