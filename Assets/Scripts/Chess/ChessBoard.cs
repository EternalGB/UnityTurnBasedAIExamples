
using System.Collections.Generic;

public class ChessBoard : GameState
{

	public BoardPosition[,] board;
	public int size = 8;
	public PieceColor playerColor;

	public ChessBoard()
	{
		board = new BoardPosition[size,size];
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
		ChessPiece whiteKing = new ChessPiece(PieceType.King, PieceColor.White, false);
		ChessPiece blackKing = new ChessPiece(PieceType.King, PieceColor.Black, false);
		return !ContainsPiece(whiteKing) || !ContainsPiece(blackKing);
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
