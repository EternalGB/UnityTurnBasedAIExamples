

public class BoardPosition
{

	public ChessPiece piece;

	public BoardPosition(ChessPiece piece)
	{
		this.piece = piece;
	}

	public bool HasPiece()
	{
		return piece != null;
	}

}
