using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableInEditor : MonoBehaviour {

#if UNITY_EDITOR
	private void Awake()
	{
		gameObject.SetActive(false);
	}
#endif
}
