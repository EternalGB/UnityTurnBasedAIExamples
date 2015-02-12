using UniversalTurnBasedAI;

public class TTTTurn : Turn
{

	int x;
	TTTBoard.TTTPiece player;

	public TTTTurn(int x, TTTBoard.TTTPiece player)
	{
		this.x = x;
		this.player = player;
	}

	public override GameState ApplyTurn (GameState state)
	{
		TTTBoard board = (TTTBoard)state;
		board.SetPiece(x,player);
		if(board.player == TTTBoard.TTTPiece.X)
			board.player = TTTBoard.TTTPiece.O;
		else
			board.player = TTTBoard.TTTPiece.X;
		return board;
	}
	
}
