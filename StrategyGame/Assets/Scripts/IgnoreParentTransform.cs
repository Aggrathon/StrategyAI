using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IgnoreParentTransform : MonoBehaviour {
	
	Quaternion rotation;
	Vector3 offset;

	private void Awake()
	{
		rotation = transform.localRotation;
		offset = transform.localPosition;
	}

	void Update () {
		transform.rotation = rotation;
		transform.position = transform.parent.position + offset;
	}
}
