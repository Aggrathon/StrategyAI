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

	public override void InitializeAcademy()
	{

	}

	public override void AcademyReset()
	{
		generator.Despawn();
		generator.Generate(map, playerOne, playerTwo);
	}

	public override void AcademyStep()
	{


	}

}
