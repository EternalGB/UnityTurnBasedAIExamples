using UnityEngine;
using System.Collections;

public abstract class Evaluator 
{

	public float minValue = float.MinValue;
	public float maxValue = float.MaxValue;

	public abstract float Evaluate(GameState state);

}

