using System;

namespace UniversalTurnBasedAI
{

	/// <summary>
	/// An Evaluator that returns random values for every state. 
	/// Can be useful to test other evaluation functions. Any evaluation function
	/// should be at least as good as selecting moves randomly.
	/// </summary>
	public class EvaluatorRandom : IEvaluator
	{
		
		float min, max;
		Random rando;

		/// <summary>
		/// Initializes a new instance of the <see cref="UniversalTurnBasedAI.EvaluatorRandom"/> class.
		/// </summary>
		/// <param name="min">The minimum value to generate</param>
		/// <param name="max">The maximum value to generate</param>
		public EvaluatorRandom(float min, float max)
		{
			this.min = min;
			this.max = max;
			rando = new Random((int)DateTime.Now.Ticks);
		}

		public float GetMinValue()
		{
			return min;
		}

		public float GetMaxValue()
		{
			return max;
		}

		public float Evaluate (IGameState state)
		{
			return min + (float)rando.NextDouble()*(max-min);
		} 
	}
}


