using System.Collections.Generic;

public class ChessBoard : GameState
{

	public BoardPosition[,] board;
	public int size = 8;
	public PieceColor playerColor;

	public ChessBoard(PieceColor playerColor)
	{
		board = new BoardPosition[size,size];
		this.playerColor = playerColor;
		InitPieces();
	}

	public ChessBoard(ChessBoard oldBoard)
	{
		board = new BoardPosition[size,size];
		playerColor = oldBoard.playerColor;
		size = oldBoard.size;
		for(int x = 0; x < size; x++) {
			for(int y = 0; y < size; y++) {
				board[x,y] = oldBoard.board[x,y];
			}
		}
	}

	void InitPieces()
	{
		for(int x = 0; x < size; x++) {
			for(int y = 0; y < size; y++) {
				board[x,y] = new BoardPosition(null);
			}
		}
		//do top and bottom rows
		int[] topAndBottomRows = new int[]{0,size-1};
		foreach(int y in topAndBottomRows) {
			PieceColor color;
			if(y == 0) {
				color = PieceColor.White;
			} else {
				color = PieceColor.Black;
			}
			board[0,y] = new BoardPosition(new ChessPiece(PieceType.Rook,color,false));
			board[1,y] = new BoardPosition(new ChessPiece(PieceType.Knight,color,false));
			board[2,y] = new BoardPosition(new ChessPiece(PieceType.Bishop,color,false));
			board[3,y] = new BoardPosition(new ChessPiece(PieceType.Queen,color,false));
			board[4,y] = new BoardPosition(new ChessPiece(PieceType.King,color,false));
			board[5,y] = new BoardPosition(new ChessPiece(PieceType.Bishop,color,false));
			board[6,y] = new BoardPosition(new ChessPiece(PieceType.Knight,color,false));
			board[7,y] = new BoardPosition(new ChessPiece(PieceType.Rook,color,false));
		}
		for(int x = 0; x < size; x++) {
			board[x,1] = new BoardPosition(new ChessPiece(PieceType.Pawn,PieceColor.White,false));
			board[x,size-2] = new BoardPosition(new ChessPiece(PieceType.Pawn,PieceColor.Black,false));
		}

	}

	public override List<Turn> GeneratePossibleTurns()
	{
		List<Turn> turns = new List<Turn>();
		for(int x = 0; x < size; x++) {
			for(int y = 0; y < size; y++) {
				if(board[x,y].HasPiece() && board[x,y].piece.color == playerColor) {
					List<ChessTurn> chessTurns = board[x,y].piece.GetPossibleMoves(this,x,y);
					foreach(ChessTurn ct in chessTurns)
						turns.Add((Turn)ct);
				}
			}
		}
		return turns;
	}

	public override bool IsTerminal ()
	{
		ChessPiece whiteKing = new ChessPiece(PieceType.King, PieceColor.White, true);
		ChessPiece blackKing = new ChessPiece(PieceType.King, PieceColor.Black, true);
		return ContainsPiece(whiteKing) || ContainsPiece(blackKing);
	}

	public override GameState Clone ()
	{
		return new ChessBoard(this);
	}

	bool ContainsPiece(ChessPiece piece)
	{
		for(int x = 0; x < size; x++) {
			for(int y = 0; y < size; y++) {
				if(board[x,y].HasPiece() && piece.Equals(board[x,y].piece))
					return true;
			}
		}
		return false;
	}

}
