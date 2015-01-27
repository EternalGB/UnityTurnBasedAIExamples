

public class BoardPosition
{

	public ChessPiece piece;

	public BoardPosition(ChessPiece piece)
	{
		this.piece = piece;
	}

	public bool IsOccupied()
	{
		return piece != null;
	}

	public BoardPosition Clone()
	{
		if(piece != null)
			return new BoardPosition(piece.Clone());
		else
			return new BoardPosition(null);
	}
}
