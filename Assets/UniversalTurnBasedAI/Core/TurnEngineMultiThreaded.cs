using System;
using System.Collections.Generic;
using System.Threading;

namespace UniversalTurnBasedAI
{

	/// <summary>
	/// A multi-threaded implementation of <see cref="TurnEngine"/>. Uses the same search algorithm as <see cref="TurnEngineSingleThreaded"/>
	/// but runs each initial branch in a separate thread.
	/// 
	/// This implementation may not be significantly faster than using <see cref="TurnEngineSingleThreaded"/> due to the overhead
	/// of managing multiple threads. May see an improvement if your GameState search tree is extremely wide i.e. in each state
	/// there is a very large number of possible moves to make.
	/// 
	/// <seealso cref="TurnEngine"/>
	/// <seealso cref="TurnEngineSingleThreaded"/>
	/// </summary>
	public class TurnEngineMultiThreaded : TurnEngine
	{

		List<ManualResetEvent> lastDoneEvents;
		List<MinimaxWorker> lastThreadWorkers;

		/// <summary>
		/// Initializes a new instance of the <see cref="UniversalTurnBasedAI.TurnEngineMultiThreaded"/> class with
		/// a time limit. Once the time limit has been reached the best turn found so far will be <paramref name="bestTurn"/>
		/// </summary>
		/// <param name="eval">The Evaluator</param>
		/// <param name="timeLimit">The time limit, must be greater than 0</param>
		/// <param name="collectStats">If set to <c>true</c> collect stats.</param>
		public TurnEngineMultiThreaded (IEvaluator eval, float timeLimit, bool collectStats)
		{
			InitEngine(eval,timeLimit,int.MaxValue,true,collectStats);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="UniversalTurnBasedAI.TurnEngineMultiThreaded"/> class with
		/// a depth limit. Searches the entire GameState tree up to the specific depth. The best turn found after the
		/// search will be <paramref name="bestTurn"/>.
		/// </summary>
		/// <param name="eval">Eval.</param>
		/// <param name="depthLimit">Depth limit, must be at least 1</param>
		/// <param name="collectStats">If set to <c>true</c> collect stats.</param>
		public TurnEngineMultiThreaded (IEvaluator eval, int depthLimit, bool collectStats)
		{
			InitEngine(eval,float.MaxValue,depthLimit,false,collectStats);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="UniversalTurnBasedAI.TurnEngineMultiThreaded"/> class.
		/// </summary>
		/// <param name="eval">The <see cref="IEvaluator"/> to use</param>
		/// <param name="timeLimit">Time limit in seconds. Must be greater than 0</param>
		/// <param name="depthLimit">Depth limit or maximum "ply". Must be at least 1</param>
		/// <param name="collectStats">If set to <c>true</c> collect stats.</param>
		public TurnEngineMultiThreaded(IEvaluator eval, float timeLimit, int depthLimit, bool collectStats)
		{
			InitEngine(eval,timeLimit, depthLimit, true,collectStats);
		}
		
		/// <summary>
		/// A wrapper for the Minimax algorithm. Initialises the first branch of turns so that
		/// they can be given values and the best possible returned. Always generates at least one
		/// possible turns so that at least some sensible result can be returned. When the search is
		/// completed or timed out <see cref="bestTurn"/> will be assigned to the best found turn.
		/// 
		/// For each initial turn this created a new <see cref="MinimaxWorker"/> is created and
		/// added to a <see cref="ThreadPool"/>. Then it waits for each thread to finish or a 
		/// time out.
		/// <seealso cref="MinimaxWorker"/>
		/// </summary>
		/// <param name="state">The starting state</param>
		protected override void TurnSearchDelegate(object state)
		{
			DateTime startTime = new DateTime(DateTime.Now.Ticks);
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

				List<ManualResetEvent> doneEvents = new List<ManualResetEvent>();
				List<MinimaxWorker> threadWorkers = new List<MinimaxWorker>();

				int timeOut;
				if(timeLimited)
					timeOut = (int)(maxTime*1000);
				else
					timeOut = Timeout.Infinite;

				foreach(ITurn turn in rootTurns) {

					ManualResetEvent waitHandle = new ManualResetEvent(false);
					MinimaxWorker nextWorker = new MinimaxWorker(root.Clone(), turn, eval, depth, false, waitHandle);
					threadWorkers.Add(nextWorker);
					doneEvents.Add(waitHandle);
					ThreadPool.QueueUserWorkItem((object threadState) => ExecuteAndCatch(nextWorker.EvaluateState, turn, ExceptionHandler));
				}

				lastThreadWorkers = threadWorkers;
				lastDoneEvents = doneEvents;

				float bestValue = eval.GetMinValue();
				if(WaitHandle.WaitAll(doneEvents.ToArray(),timeOut) && !stopped) {
					foreach(MinimaxWorker mm in threadWorkers) {
						if(mm.Value >= bestValue) {
							if(mm.Value > bestValue) {
								bestValue = mm.Value;
								potentialTurns.Clear();
							}

							potentialTurns.Add(mm.firstTurn);
						}
					}
				} else {
					foreach(MinimaxWorker mm in threadWorkers) {
						mm.Stop();
					}
					exit = true;
				}


				//only overwrite the results if we haven't aborted mid search
				if(!exit) {
					results = potentialTurns;
					resultsValue = bestValue;
				} else if(timeLimited)
					//for debugging/logging purposes
					depth--;
				bestTurn = GetRandomElement<ITurn>(results);
				lastThreadWorkers = null;
				lastDoneEvents = null;
			}
			if(collectStats)
				Stats.Log(depth,(float)DateTime.Now.Subtract(startTime).TotalSeconds);
		}



		public override void Stop ()
		{
			base.Stop ();
			if(lastThreadWorkers != null && lastDoneEvents != null) {
				foreach(ManualResetEvent mre in lastDoneEvents) {
					mre.Set();
				}
				foreach(MinimaxWorker mm in lastThreadWorkers) {
					mm.Stop();
				}

			}
		}
		
	}
	
}
