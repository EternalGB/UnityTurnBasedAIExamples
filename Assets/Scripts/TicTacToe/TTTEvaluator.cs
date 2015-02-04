using GenericTurnBasedAI;

public class TTTEvaluator : Evaluator
{

	TTTBoard.TTTPiece player;

	public TTTEvaluator(TTTBoard.TTTPiece player)
	{
		this.player = player;
	}

	public override float Evaluate (GameState state)
	{
		TTTBoard board = (TTTBoard)state;
		if(board.Winner(player))
			return 100;
		else if(board.Loser(player))
			return -100;
		else
			return 0;
	}

	public override Evaluator Clone()
	{
		return new TTTEvaluator(player);
	}

}
