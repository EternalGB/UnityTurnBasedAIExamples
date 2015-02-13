using UniversalTurnBasedAI;

/// <summary>
/// Our representation of a Turn in Connect K. Records the column
/// to place the piece and what piece is being placed. Calls 
/// <see cref="ConnectKBoard.AddPiece"/> to perform the move.
/// </summary>
public class ConnectKTurn : Turn
{

	int column;
	ConnectKPiece piece;

	public ConnectKTurn (int column, ConnectKPiece piece)
	{
		this.column = column;
		this.piece = piece;
	}

	public override GameState ApplyTurn (GameState state)
	{
		ConnectKBoard board = state as ConnectKBoard;
		board = board.Clone() as ConnectKBoard;
		board.AddPiece(piece,column);
		if(board.player == ConnectKPiece.P1)
			board.player = ConnectKPiece.P2;
		else
			board.player = ConnectKPiece.P1;
		return board;
	}

}
