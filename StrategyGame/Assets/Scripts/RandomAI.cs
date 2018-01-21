using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Brain))]
public class RandomAI : MonoBehaviour, Decision {

	Brain brain;

	private void Awake()
	{
		brain = GetComponent<Brain>();
	}

	public float[] Decide(List<float> state, List<Camera> observation, float reward, bool done, float[] memory)
	{
		return new float[] { Random.Range(0, brain.brainParameters.actionSize) };
	}

	public float[] MakeMemory(List<float> state, List<Camera> observation, float reward, bool done, float[] memory)
	{
		return memory;
	}
}
