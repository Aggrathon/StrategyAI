using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreUI : MonoBehaviour {

	public MLAcademy academy;

	float[] cache = new float[] { -1, -1 };

	void Update ()
	{
		SetScore(0, false);
		SetScore(1, true);
	}

	void SetScore(int index, bool reverse)
	{
		Transform tr = transform.GetChild(index);
		if (academy.teams.Count > index && academy.teams[index].units.Count > 0)
		{
			tr.gameObject.SetActive(true);
			tr = tr.GetChild(0);
			while (tr.childCount - 1 < academy.teams[index].units.Count)
				Instantiate(tr.GetChild(1), tr);
			for (int i = tr.childCount - 1; i > academy.teams[index].units.Count; i--)
				Destroy(tr.GetChild(i).gameObject);
			if (cache[index] != academy.teams[index].score)
			{
				cache[index] = academy.teams[index].score;
				if (reverse)
					(tr.GetChild(0) as RectTransform).anchorMin = new Vector2(1f - cache[index] * 0.9f - 0.1f, 0);
				else
					(tr.GetChild(0) as RectTransform).anchorMax = new Vector2(cache[index] * 0.9f + 0.1f, 1);
			}
		}
		else
		{
			tr.gameObject.SetActive(false);
		}
	}
}
