using UniversalTurnBasedAI;

/// <summary>
/// Tic Tac Toe evaluator. We don't bother evaluating partial states as we are just
/// going to search the whole tree every time anyway
/// </summary>
public class TTTEvaluator : IEvaluator
{

	TTTBoard.TTTPiece player;
	float minValue = -100;
	float maxValue = 100;

	public TTTEvaluator(TTTBoard.TTTPiece player)
	{
		this.player = player;
	}

	public float GetMinValue()
	{
		return minValue;
	}

	public float GetMaxValue()
	{
		return maxValue;
	}

	public float Evaluate (IGameState state)
	{
		TTTBoard board = (TTTBoard)state;
		if(board.Winner(player))
			return maxValue;
		else if(board.Loser(player))
			return minValue;
		else
			return 0;
	}
	

}
