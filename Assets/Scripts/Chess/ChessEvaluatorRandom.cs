using UnityEngine;

public class ChessEvaluatorRandom : Evaluator
{

	float min, max;

	public ChessEvaluatorRandom(float min, float max)
	{
		this.min = min;
		this.max = max;
	}

	public override float Evaluate (GameState state)
	{
		return Random.Range(min,max);
	}

}
