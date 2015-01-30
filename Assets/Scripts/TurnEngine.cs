using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading;

public class TurnEngine
{

	int maxDepth;
	Evaluator eval;
	Turn bestTurn;
	bool timeLimited = false;
	float maxTime;
	System.Random rando;
	public EngineStats Stats
	{
		get; private set;
	}
	bool collectStats = false;

	public delegate void TurnReady(Turn bestTurn);
	public event TurnReady TurnReadyEvent;
	

	public TurnEngine(Evaluator eval, int limit, bool timeLimited, bool collectStats)
	{
		this.timeLimited = timeLimited;
		this.collectStats = collectStats;
		if(timeLimited) {
			this.maxTime = limit;
			maxDepth = int.MaxValue;
		} else
			this.maxDepth = limit;
		if(collectStats) {
			Stats = new EngineStats();
		}
		this.eval = eval;
		InitEngine();
	}
	

	void InitEngine()
	{
		rando = new System.Random((int)DateTime.Now.Ticks);
	}

	public System.Collections.IEnumerator GetNextTurn(GameState state) 
	{
		bestTurn = null;
		Thread thread = new Thread(MinimaxCaller);
		thread.Start(state);
		while(thread.IsAlive) {
			yield return 0;
		}
		if(TurnReadyEvent != null)
			TurnReadyEvent(bestTurn);
	}

	public void MinimaxCaller(object state)
	{
		DateTime startTime = new DateTime(DateTime.Now.Ticks);
		bool exit = false;
		List<Turn> results = null;
		float resultsValue = eval.minValue;
		GameState root = (GameState)state;



		//precompute the first level so we don't have to every time
		List<Turn> rootTurns = new List<Turn>();
		foreach(Turn turn in root.GeneratePossibleTurns()) {
			rootTurns.Add(turn);
		}

		int depth;
		for(depth = 1; depth <= maxDepth && !exit; depth++) {
			List<Turn> potentialTurns = new List<Turn>();

			float bestValue = eval.minValue;
			foreach(Turn turn in rootTurns) {
				if(timeLimited && results != null && DateTime.Now.Subtract(startTime).Seconds >= maxTime) {
					exit = true;
					break;
				}


				GameState nextState = turn.ApplyTurn(root.Clone());
				float value = AlphaBeta(nextState,depth-1,eval.minValue,eval.maxValue,false);
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
			} else 
				//for debugging/logging purposes
				depth--;
			bestTurn = GetRandomElement<Turn>(results);
			Debug.Log ("Best value " + resultsValue + ", Ply: " + depth);
		}
		if(collectStats)
			Stats.Log(depth,DateTime.Now.Subtract(startTime).Seconds);
	}


	float AlphaBeta(GameState state, int depth, float alpha, float beta, bool ourTurn)
	{
		//if we already have a value for this state then we can just skip everything
		/*
		if(transTable.ContainsKey(state) && transTable[state].depth < depth) {
			Debug.Log ("Used transposition table!");
			return transTable[state].value;
		}
		*/
		if(depth == 0 || state.IsTerminal()) {
			return eval.Evaluate(state);
		}
		if(ourTurn) {
			float bestValue = eval.minValue;
			foreach(Turn turn in state.GeneratePossibleTurns()) {
				GameState nextState = turn.ApplyTurn(state.Clone());
				float value = AlphaBeta(nextState,depth-1,alpha,beta,false);
				if(value > bestValue) {

					bestValue = value;
				}
				value = Math.Max(value,bestValue);
				//AddTransposition(nextState,value,depth);
				alpha = Math.Max(alpha,value);
				if(beta <= alpha) {
					//Debug.Log ("Pruned a branch");
					break;
				}
					
			}
			return bestValue;
		} else {
			float worstValue = eval.maxValue;
			foreach(Turn turn in state.GeneratePossibleTurns()) {
				GameState nextState = turn.ApplyTurn(state.Clone());
				float value = AlphaBeta(nextState,depth-1,alpha,beta,true);
				if(value < worstValue) {
					worstValue = value;
				}
				value = Math.Min(value,worstValue);
				//AddTransposition(nextState,value,depth);
				beta = Math.Min(beta,value);
				if(beta <= alpha) {
					//Debug.Log ("Pruned a branch");
					break;
				}
			}
			return worstValue;
		}
	}

	static T GetRandomElement<T>(IList<T> list)
	{
		System.Random rando = new System.Random((int)DateTime.Now.Ticks);
		return list[rando.Next (list.Count)];
	}


	public EngineStats ResetStatisticsLog()
	{
		if(collectStats) {
			EngineStats old = Stats;
			Stats = new EngineStats();
			return old;
		} else
			return null;
	}

	public class Node
	{
		public GameState state;
		public Node parent;
		public int depth;
		public bool ourTurn;
		public float value;
		public Turn generatedBy;
		public Turn bestTurn;
		public float alpha;
		public float beta;

		public Node (GameState state, Node parent, Turn generatedBy, int depth, bool ourTurn, float alpha, float beta)
		{
			this.state = state;
			this.depth = depth;
			this.ourTurn = ourTurn;
			this.parent = parent;
			this.generatedBy = generatedBy;
			if(ourTurn)
				value = float.MinValue;
			else
				value = float.MaxValue;
			this.alpha = alpha;
			this.beta = beta;
		}
		
	}

}
