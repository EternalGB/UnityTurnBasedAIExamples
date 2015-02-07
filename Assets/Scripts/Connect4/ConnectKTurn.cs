using GenericTurnBasedAI;

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
