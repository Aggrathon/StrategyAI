using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallColor : MonoBehaviour {

	new public Renderer renderer;
	public Color highColor = new Color(0, 0, 0);
	public Color lowColor = new Color(0.7f, 0.7f, 0.7f);

	private void OnEnable()
	{
		if (renderer == null)
			renderer = GetComponentInChildren<Renderer>();
		var mpb = new MaterialPropertyBlock();
		mpb.SetColor("_Color", Color.Lerp(highColor, lowColor, -transform.position.y));
		renderer.SetPropertyBlock(mpb);
	}
}
