

public class ChessTurn : Turn
{

	int fromX,fromY,toX,toY;

	public ChessTurn (int fromX, int fromY, int toX, int toY)
	{
		this.fromX = fromX;
		this.fromY = fromY;
		this.toX = toX;
		this.toY = toY;
	}
	

	public override GameState ApplyTurn (GameState state)
	{
		ChessBoard board = (ChessBoard)state;
		if(board.board[fromX,fromY].HasPiece()) {
			board.board[toX,toY].piece.captured = true;
		}
		board.board[toX,toY] = board.board[fromX,fromX];

		return board;
	}


}
