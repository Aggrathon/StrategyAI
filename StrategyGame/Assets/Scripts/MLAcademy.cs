using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MLAcademy : Academy {


	[Header("Brains")]
	public Brain playerOne;
	public Brain playerTwo;
	public Brain[] humanBrains;
	public Brain[] externalBrains;
	public Brain[] internalBrains;

	[Header("Level")]
	public Texture2D map;
	public LevelGenerator generator;
	public int width = 60;
	public int height = 30;

	GameState state;

	public override void InitializeAcademy()
	{

	}

	public override void AcademyReset()
	{
		generator.Despawn();
		//Select: resetParameters["Difficulty"]
		float players = 2f;
		if(!resetParameters.TryGetValue("Players", out players))
		{
			players = 2f;
		}
		switch(Mathf.RoundToInt(players))
		{
			case 0:
				playerOne = externalBrains[0];
				playerTwo = externalBrains[1];
				break;
			case 1:
				playerOne = humanBrains[0];
				playerTwo = externalBrains[0];
				break;
			case 2:
				playerOne = humanBrains[0];
				playerTwo = humanBrains[1];
				break;
		}
		state = generator.Generate(map, playerOne, playerTwo, width, height);
		playerOne.brainParameters.stateSize = state.map.Capacity;
		playerTwo.brainParameters.stateSize = state.map.Capacity;
	}

	public override void AcademyStep()
	{
		state.UpdatePositions();
	}

}
