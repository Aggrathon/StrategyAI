using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour {

	private void OnTriggerEnter(Collider other)
	{
		if (other.attachedRigidbody != null)
		{
			var s = other.attachedRigidbody.GetComponent<Soldier>();
			if (s != null)
				s.goals++;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.attachedRigidbody != null)
		{
			var s = other.attachedRigidbody.GetComponent<Soldier>();
			if (s != null)
				s.goals--;
		}
	}
}
