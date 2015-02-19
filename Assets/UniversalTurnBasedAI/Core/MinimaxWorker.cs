using System;
using System.Threading;

namespace UniversalTurnBasedAI
{

	/// <summary>
	/// Holds all of the initialising information required for a Minimax search so it
	/// can more easily be passed to a ThreadPool
	/// </summary>
	public class MinimaxWorker
	{

		IGameState rootState;
		/// <summary>
		/// The first turn to use in the search
		/// this is the turn retrieved for returning
		/// </summary>
		public ITurn firstTurn;
		int maxDepth;
		bool ourTurn;
		IEvaluator eval;
		EventWaitHandle waitHandle;
		/// <summary>
		/// The value of <paramref name="firstTurn"/>
		/// </summary>
		/// <value>The value.</value>
		public float Value
		{
			get; private set;
		}
		bool stopped;

		/// <summary>
		/// Initializes a new instance of the <see cref="UniversalTurnBasedAI.MinimaxWorker"/> class.
		/// </summary>
		/// <param name="rootState">The starting state</param>
		/// <param name="firstTurn">The turn to apply to the starting state to generate this worker's branch</param>
		/// <param name="eval">The Evaluator</param>
		/// <param name="maxDepth">Max depth.</param>
		/// <param name="ourTurn">Whether or not it is the searching player's turn</param>
		/// <param name="waitHandle">Signals the ThreadPool that the search is complete</param>
		public MinimaxWorker (IGameState rootState, ITurn firstTurn, IEvaluator eval, int maxDepth, bool ourTurn, EventWaitHandle waitHandle)
		{
			this.rootState = rootState;
			this.firstTurn = firstTurn;
			this.maxDepth = maxDepth;
			this.ourTurn = ourTurn;
			this.eval = eval;
			this.waitHandle = waitHandle;
			stopped = true;
		}
		

		public void EvaluateState(object threadState)
		{
			IGameState state = firstTurn.ApplyTurn(rootState.Clone());
			stopped = false;
			Value = AlphaBeta(state,eval,maxDepth,eval.GetMinValue(),eval.GetMaxValue(),ourTurn);
			waitHandle.Set();
		}

		public float AlphaBeta(IGameState state, IEvaluator eval, int depth, float alpha, float beta, bool ourTurn)
		{
			if(depth == 0 || state.IsTerminal()) {
				return eval.Evaluate(state);
			}
			if(ourTurn) {
				float bestValue = eval.GetMinValue();
				foreach(ITurn turn in state.GeneratePossibleTurns()) {
					if(stopped)
						return eval.GetMinValue();
					IGameState nextState = turn.ApplyTurn(state.Clone());
					float value = AlphaBeta(nextState,eval,depth-1,alpha,beta,false);
					if(value > bestValue) {
						
						bestValue = value;
					}
					value = Math.Max(value,bestValue);
					alpha = Math.Max(alpha,value);
					if(beta <= alpha) {
						break;
					}
					
				}
				return bestValue;
			} else {
				float worstValue = eval.GetMaxValue();
				foreach(ITurn turn in state.GeneratePossibleTurns()) {
					if(stopped)
						return eval.GetMinValue();
					IGameState nextState = turn.ApplyTurn(state.Clone());
					float value = AlphaBeta(nextState,eval,depth-1,alpha,beta,true);
					if(value < worstValue) {
						worstValue = value;
					}
					value = Math.Min(value,worstValue);
					beta = Math.Min(beta,value);
					if(beta <= alpha) {
						break;
					}
				}
				return worstValue;
			}
		}

		public void Stop()
		{
			stopped = true;
		}
	}


}


