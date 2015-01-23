using System.Collections.Generic;

public class TurnEngine 
{

	int maxDepth;
	Evaluator eval;

	public TurnEngine (int maxDepth, Evaluator eval)
	{
		this.maxDepth = maxDepth;
		this.eval = eval;
	}

	public Turn GetNextTurn(GameState state) 
	{
		Turn bestTurn = null;
		Minimax(state,maxDepth,true,ref bestTurn);
		return bestTurn;
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
				GameState nextState = turn.ApplyTurn(state);
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
				GameState nextState = turn.ApplyTurn(state);
				float value = Minimax(nextState,depth-1,true, ref bestTurn);
				if(value < worstValue) {
					worstValue = value;
				}
			}
			return worstValue;
		}
	}

}
