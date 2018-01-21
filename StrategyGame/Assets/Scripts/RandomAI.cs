using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Brain))]
public class RandomAI : MonoBehaviour, Decision {

	Brain brain;
	float[] actions;

	private void Awake()
	{
		brain = GetComponent<Brain>();
		actions = new float[1];
	}

	public float[] Decide(List<float> state, List<Camera> observation, float reward, bool done, float[] memory)
	{
		actions[0] = Random.Range(0.0f, brain.brainParameters.actionSize);
		return actions;
	}

	public float[] MakeMemory(List<float> state, List<Camera> observation, float reward, bool done, float[] memory)
	{
		return memory;
	}
}
