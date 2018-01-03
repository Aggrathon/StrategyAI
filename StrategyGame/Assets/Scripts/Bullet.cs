using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
#pragma warning disable 0108
	Rigidbody rigidbody;
	TrailRenderer trail;

	public float speed = 10f;
	public float damage = 25.2f;
	//public float safePeriod = 0.2f;
	public float liveTime = 5f;

	float time;
	[System.NonSerialized] public Soldier shooter;

	private void Awake()
	{
		rigidbody = GetComponent<Rigidbody>();
		trail = GetComponentInChildren<TrailRenderer>();
	}

	private void OnEnable()
	{
		time = Time.time;
		trail.Clear();
	}

	private void FixedUpdate()
	{
		if (time + liveTime < Time.time)
			gameObject.SetActive(false);
		rigidbody.MovePosition(rigidbody.position + transform.forward * speed * Time.fixedDeltaTime);
	}

	private void OnTriggerEnter(Collider other)
	{
		//if (time + safePeriod < Time.time)
		//{
		if (other.attachedRigidbody != null)
		{
			var s = other.attachedRigidbody.GetComponent<Soldier>();
			if (s != null)
			{
				if (s == shooter)
					return;
				else if (s.brain == shooter.brain)
					shooter.reward -= s.DoDamage(damage);
				else
					shooter.reward += s.DoDamage(damage);
			}
		}
		gameObject.SetActive(false);
		//}
	}
}
