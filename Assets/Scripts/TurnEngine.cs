//using UnityEngine;
using System.Collections.Generic;

public class TurnEngine 
{

	int maxDepth;
	Evaluator eval;

	public delegate void TurnReady(Turn bestTurn);
	public event TurnReady TurnReadyEvent;

	public TurnEngine (int maxDepth, Evaluator eval)
	{
		this.maxDepth = maxDepth;
		this.eval = eval;
	}

	public System.Collections.IEnumerator GetNextTurn(GameState state) 
	{
		Turn bestTurn = null;
		Minimax(state,maxDepth, true, ref bestTurn);
		if(TurnReadyEvent != null)
			TurnReadyEvent(bestTurn);
		yield return 0;
	}


	float Minimax(GameState state, int depth, bool ourTurn,  ref Turn bestTurn)
	{
		if(depth == 0 || state.IsTerminal()) {
			return eval.Evaluate(state);
		}
		if(ourTurn) {
			float bestValue = eval.minValue;
			List<Turn> turns = state.GeneratePossibleTurns();
			foreach(Turn turn in turns) {
				GameState nextState = turn.ApplyTurn(state.Clone());
				float value = Minimax(nextState,depth-1,false, ref bestTurn);
				if(value > bestValue) {
					bestValue = value;
					bestTurn = turn;
				}
			}
			return bestValue;
		} else {
			float worstValue = eval.maxValue;
			List<Turn> turns = state.GeneratePossibleTurns();
			foreach(Turn turn in turns) {
				GameState nextState = turn.ApplyTurn(state.Clone());
				float value = Minimax(nextState,depth-1,true, ref bestTurn);
				if(value < worstValue) {
					worstValue = value;
				}
			}
			return worstValue;
		}
	}

	public System.Collections.IEnumerator IterativeMinimax(GameState rootState)
	{
		int depth = maxDepth;
		Stack<Node> stack = new Stack<Node>();
		GameState rootClone = rootState.Clone();
		Node root = new Node(rootClone,null,null,0,true);
		stack.Push(root);
		while(stack.Count > 0) {
			Node node = stack.Pop();
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
					} else {
						if(value < cursor.value) {
							cursor.value = value;
							cursor.bestTurn = child.generatedBy;
						} else {
							value = cursor.value;
						}
					}
					child = cursor;
					cursor = cursor.parent;
					yield return 0;
				}
			} else {
				List<Turn> turns = node.state.GeneratePossibleTurns();
				foreach(Turn turn in turns) {
					GameState nextState = turn.ApplyTurn(node.state.Clone ());
					stack.Push(new Node(nextState,node,turn,depth+1,!node.ourTurn));
				}
			}
			yield return 0;
		}


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

		public Node (GameState state, Node parent, Turn generatedBy, int depth, bool ourTurn)
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
		}
		
	}

}
