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
	Dictionary<GameState, TransTableEntry> transTable;
	System.Random rando;
	

	public delegate void TurnReady(Turn bestTurn);
	public event TurnReady TurnReadyEvent;
	

	public TurnEngine(Evaluator eval, int limit, bool timeLimited = false)
	{
		this.timeLimited = timeLimited;
		if(timeLimited) {
			this.maxTime = limit;
			maxDepth = int.MaxValue;
		} else
			this.maxDepth = limit;
		this.eval = eval;
		InitEngine();
	}

	void InitEngine()
	{
		rando = new System.Random((int)DateTime.Now.Ticks);
		transTable = new Dictionary<GameState, TransTableEntry>();
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

		for(int depth = 1; depth <= maxDepth && !exit; depth++) {
			List<Turn> potentialTurns = new List<Turn>();

			float bestValue = eval.minValue;
			foreach(Turn turn in rootTurns) {
				if(timeLimited && results != null && DateTime.Now.Subtract(startTime).Seconds >= maxTime) {
					Debug.Log ("Time exceeded, aborting");
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
				//for debugging purposes
				depth--;
			bestTurn = GetRandomElement<Turn>(results);
			Debug.Log ("Best value " + resultsValue + ", Ply: " + depth);
		}
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
				if(beta <= alpha)
					break;
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
				if(beta <= alpha)
					break;
			}
			return worstValue;
		}
	}

	float Minimax(GameState state, int depth, bool ourTurn)
	{
		if(depth == 0 || state.IsTerminal()) {
			return eval.Evaluate(state);
		}
		if(ourTurn) {
			float bestValue = eval.minValue;
			foreach(Turn turn in state.GeneratePossibleTurns()) {
				
				GameState nextState = turn.ApplyTurn(state.Clone());
				float value = Minimax(nextState,depth-1,false);
				if(value > bestValue ) {
					bestValue = value;
					bestTurn = turn;
				} 
			}
			return bestValue;
		} else {
			float worstValue = eval.maxValue;
			foreach(Turn turn in state.GeneratePossibleTurns()) {
				GameState nextState = turn.ApplyTurn(state.Clone());
				float value = Minimax(nextState,depth-1,true);
				if(value < worstValue) {
					worstValue = value;
				}
			}
			return worstValue;
		}
	}

	void AddTransposition(GameState nextState, float value, int depth)
	{
		if(!transTable.ContainsKey(nextState)) {
			transTable.Add(nextState,new TransTableEntry(depth,value));
		} else if(transTable.ContainsKey(nextState) && transTable[nextState].depth < depth) {
			transTable[nextState].value = value;
			transTable[nextState].depth = depth;
		}
	}

	public System.Collections.IEnumerator IterativeMinimax(GameState rootState)
	{
		Stack<Node> stack = new Stack<Node>();
		GameState rootClone = rootState.Clone();
		Node root = new Node(rootClone,null,null,0,true, eval.minValue, eval.maxValue);
		stack.Push(root);
		int counter = 0;
		int maxDepthReached = 0;
		while(stack.Count > 0) {
			counter++;
			Node node = stack.Pop();
			//Debug.Log ("At depth " + node.depth);
			if(node.depth > maxDepthReached)
				maxDepthReached = node.depth;
			//iterate back up
			if(node.depth >= maxDepth || node.state.IsTerminal()) {
				Node cursor = node.parent;
				Node child = node;
				float value = eval.Evaluate(node.state);
				while(cursor != null) {
					if(cursor.ourTurn) {
						if(value > cursor.value) {
							cursor.value = value;
							cursor.bestTurn = child.generatedBy;
						} else {
							value = cursor.value;
						}
						cursor.alpha = Mathf.Max(cursor.alpha, value);
					} else {
						if(value < cursor.value) {
							cursor.value = value;
							cursor.bestTurn = child.generatedBy;
						} else {
							value = cursor.value;
						}
						cursor.beta = Mathf.Min(cursor.beta,value);
					}
					if(cursor.beta <= cursor.alpha) {
						//delete everything at the same depth?

					}
					child = cursor;
					cursor = cursor.parent;
					yield return 0;
				}
			} else {
				//Debug.Log ("Adding " + turns.Count + " turns");
				foreach(Turn turn in node.state.GeneratePossibleTurns()) {
					GameState nextState = turn.ApplyTurn(node.state.Clone ());
					stack.Push(new Node(nextState,node,turn,node.depth+1,!node.ourTurn, eval.minValue, eval.maxValue));
				}
			}
			yield return 0;
		}

		Debug.Log ("Examined " + counter + " nodes, max depth = " + maxDepthReached + ", best turn is " + root.bestTurn.ToString());
		if(TurnReadyEvent != null)
			TurnReadyEvent(root.bestTurn);
	}

	static T GetRandomElement<T>(IList<T> list)
	{
		System.Random rando = new System.Random((int)DateTime.Now.Ticks);
		return list[rando.Next (list.Count)];
	}


	public class TransTableEntry
	{
		public int depth;
		public float value;

		public TransTableEntry (int depth, float value)
		{
			this.value = value;
			this.depth = depth;
		}

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
