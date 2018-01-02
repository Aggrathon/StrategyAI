using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreUI : MonoBehaviour {

	public MLAcademy academy;
	public Image timeImage;
	public RectTransform playerOneScore;
	public RectTransform playerTwoScore;

	float[] cache = new float[] { -1, -1 };

	void Update ()
	{
		SetScore(playerOneScore, 0, false);
		SetScore(playerTwoScore, 1, true);
		timeImage.fillAmount = 1.01f-(float)academy.currentStep/(float)academy.MaxSteps;
	}

	void SetScore(Transform tr, int index, bool reverse)
	{
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
