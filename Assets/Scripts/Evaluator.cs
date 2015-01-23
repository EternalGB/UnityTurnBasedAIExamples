using UnityEngine;
using System.Collections;

public abstract class Evaluator 
{

	public float minValue, maxValue;

	public abstract float Evaluate(GameState state);

}

