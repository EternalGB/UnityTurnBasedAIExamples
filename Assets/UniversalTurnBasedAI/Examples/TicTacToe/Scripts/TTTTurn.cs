using UniversalTurnBasedAI;

/// <summary>
/// A Tic Tac Toe turn
/// </summary>
public class TTTTurn : ITurn
{

	int x;
	TTTBoard.TTTPiece player;

	public TTTTurn(int x, TTTBoard.TTTPiece player)
	{
		this.x = x;
		this.player = player;
	}

	public IGameState ApplyTurn (IGameState state)
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
