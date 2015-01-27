using System;

public class ChessEvaluatorRandom : Evaluator
{

	float min, max;
	Random rando;

	public ChessEvaluatorRandom(float min, float max)
	{
		this.min = min;
		this.max = max;
		rando = new Random((int)DateTime.Now.Ticks);
	}

	public override float Evaluate (GameState state)
	{
		return min + (float)rando.NextDouble()*(max-min);
	}

}
