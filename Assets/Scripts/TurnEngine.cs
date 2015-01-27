using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading;

public class TurnEngine 
{

	int maxDepth;
	Evaluator eval;
	System.Random rando;
	Turn bestTurn;

	public delegate void TurnReady(Turn bestTurn);
	public event TurnReady TurnReadyEvent;

	public TurnEngine (int maxDepth, Evaluator eval)
	{
		this.maxDepth = maxDepth;
		this.eval = eval;
		rando = new System.Random((int)System.DateTime.Now.Ticks);
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

		//Debug.Log ("Starting minimax");
		AlphaBeta((GameState)state,maxDepth,eval.minValue,eval.maxValue,true);

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

	float AlphaBeta(GameState state, int depth, float alpha, float beta, bool ourTurn)
	{
		//Debug.Log ("Depth " + depth + " alpha: " + alpha + ", beta: " + beta);
		if(depth == 0 || state.IsTerminal()) {
			return eval.Evaluate(state);
		}
		if(ourTurn) {
			float bestValue = eval.minValue;
			foreach(Turn turn in state.GeneratePossibleTurns()) {
				GameState nextState = turn.ApplyTurn(state.Clone());
				float value = AlphaBeta(nextState,depth-1,alpha,beta,false);
				if(value > bestValue || (value == bestValue && rando.NextDouble() < 0.5)) {
					bestValue = value;
					if(depth == maxDepth)
						bestTurn = turn;
				}
				value = Math.Max(value,bestValue);
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
				beta = Math.Min(beta,value);
				if(beta <= alpha)
					break;
			}
			return worstValue;
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
						cursor.alpha = Math.Max(cursor.alpha, value);
					} else {
						if(value < cursor.value) {
							cursor.value = value;
							cursor.bestTurn = child.generatedBy;
						} else {
							value = cursor.value;
						}
						cursor.beta = Math.Min(cursor.beta,value);
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
