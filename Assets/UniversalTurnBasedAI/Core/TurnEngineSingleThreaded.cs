using System;
using System.Collections.Generic;
using System.Threading;

namespace UniversalTurnBasedAI
{

	/// <summary>
	/// A single threaded implementation of <see cref="TurnEngine"/>. Uses an implementation of
	/// the Minimax algorithm with Alpha-Beta pruning.
	/// 
	/// <seealso cref="TurnEngine"/> 
	/// <seealso cref="TurnEngineMultiThreaded"/>
	/// </summary>
	public class TurnEngineSingleThreaded : TurnEngine
	{

		/// <summary>
		/// Initializes a new instance of the <see cref="UniversalTurnBasedAI.TurnEngineSingleThreaded"/> class with
		/// a time limit. Once the time limit has been reached the best turn found so far will be <paramref name="bestTurn"/>
		/// </summary>
		/// <param name="eval">The Evaluator</param>
		/// <param name="timeLimit">The time limit, must be greater than 0</param>
		/// <param name="collectStats">If set to <c>true</c> collect stats.</param>
		public TurnEngineSingleThreaded (IEvaluator eval, float timeLimit, bool collectStats)
		{
			InitEngine(eval,timeLimit,int.MaxValue,true,collectStats);
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="UniversalTurnBasedAI.TurnEngineSingleThreaded"/> class with
		/// a depth limit. Searches the entire GameState tree up to the specific depth. The best turn found after the
		/// search will be <paramref name="bestTurn"/>.
		/// </summary>
		/// <param name="eval">Eval.</param>
		/// <param name="depthLimit">Depth limit, must be at least 1</param>
		/// <param name="collectStats">If set to <c>true</c> collect stats.</param>
		public TurnEngineSingleThreaded (IEvaluator eval, int depthLimit, bool collectStats)
		{
			InitEngine(eval,float.MaxValue,depthLimit,false,collectStats);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="UniversalTurnBasedAI.TurnEngineSingleThreaded"/> class with
		/// both a time and depth limit.
		/// </summary>
		/// <param name="eval">The <see cref="IEvaluator"/> to use</param>
		/// <param name="timeLimit">Time limit in seconds. Must be greater than 0</param>
		/// <param name="depthLimit">Depth limit or maximum "ply". Must be at least 1</param>
		/// <param name="collectStats">If set to <c>true</c> collect stats.</param>
		public TurnEngineSingleThreaded (IEvaluator eval, float timeLimit, int depthLimit, bool collectStats)
		{
			InitEngine(eval,timeLimit,depthLimit,true,collectStats);
		}

		/// <summary>
		/// A wrapper for the Minimax algorithm. Initialises the first branch of turns so that
		/// they can be given values and the best possible returned. Always generates at least one
		/// possible turns so that at least some sensible result can be returned. When the search is
		/// completed or timed out <see cref="bestTurn"/> will be assigned to the best found turn.
		/// </summary>
		/// <param name="state">The starting state</param>
		protected override void TurnSearchDelegate(object state)
		{
			

			bool exit = false;
			List<ITurn> results = null;
			float resultsValue = eval.GetMinValue();
			IGameState root = (IGameState)state;
			
			
			
			//precompute the first level so we don't have to every time
			List<ITurn> rootTurns = new List<ITurn>();
			foreach(ITurn turn in root.GeneratePossibleTurns()) {
				rootTurns.Add(turn);
				if(Exit) {
					exit = true;
					break;
				}
			}
			//this is so we can bail out without evaluating any turns
			results = rootTurns;
			if(exit) {
				bestTurn = GetRandomElement<ITurn>(results);
				return;
			}
			
			int depth;
			for(depth = 1; depth <= maxDepth && !exit; depth++) {
				
				List<ITurn> potentialTurns = new List<ITurn>();
				
				float bestValue = eval.GetMinValue();
				foreach(ITurn turn in rootTurns) {
					if(Exit) {
						exit = true;
						break;
					}
					
					
					IGameState nextState = turn.ApplyTurn(root.Clone());
					float value = AlphaBeta(nextState,eval,depth-1,eval.GetMinValue(),eval.GetMaxValue(),false);
					if(value >= bestValue) {
						if(value > bestValue) {
							bestValue = value;
							potentialTurns.Clear();
						}
						potentialTurns.Add(turn);
					}
					
				}
				//only overwrite the results if we haven't aborted mid search
				if(!exit) {
					results = potentialTurns;
					resultsValue = bestValue;
				} else if(timeLimited)
					//for debugging/logging purposes
					depth--;
				bestTurn = GetRandomElement<ITurn>(results);
			}
			if(collectStats)
				Stats.Log(depth,(float)DateTime.Now.Subtract(startTime).TotalSeconds);
		}

		/// <summary>
		/// An implementation of the Minimax algorithm with Alpha-Beta pruning
		/// </summary>
		/// <returns>The beta.</returns>
		/// <param name="state">Starting state</param>
		/// <param name="eval">Used to evaluate leaf states</param>
		/// <param name="depth">The current depth (counting down)</param>
		/// <param name="alpha">The current upper bound of values found</param>
		/// <param name="beta">The current lower bound of values found</param>
		/// <param name="ourTurn">Whether or not it is the searching player's turn</param>
		float AlphaBeta(IGameState state, IEvaluator eval, int depth, float alpha, float beta, bool ourTurn)
		{
			if(depth == 0 || state.IsTerminal()) {
				return eval.Evaluate(state);
			}
			if(ourTurn) {
				float bestValue = eval.GetMinValue();
				foreach(ITurn turn in state.GeneratePossibleTurns()) {
					if(Exit)
						break;
					IGameState nextState = turn.ApplyTurn(state.Clone());
					float value = AlphaBeta(nextState,eval,depth-1,alpha,beta,false);
					if(value > bestValue) {
						
						bestValue = value;
					}
					alpha = Math.Max(alpha,bestValue);
					if(beta <= alpha) {
						break;
					}
					
				}
				return bestValue;
			} else {
				float worstValue = eval.GetMaxValue();
				foreach(ITurn turn in state.GeneratePossibleTurns()) {
					if(Exit)
						break;
					IGameState nextState = turn.ApplyTurn(state.Clone());
					float value = AlphaBeta(nextState,eval,depth-1,alpha,beta,true);
					if(value < worstValue) {
						worstValue = value;
					}
					beta = Math.Min(beta,worstValue);
					if(beta <= alpha) {
						break;
					}
				}
				return worstValue;
			}
		}
		
		
	}
	
}
