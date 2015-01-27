using UnityEngine;
using System;

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
		ChessPiece piece = board.board[fromX,fromY].piece;
		board.board[toX,toY] = board.board[fromX,fromY];
		board.board[fromX,fromY] = new BoardPosition(null);
		piece.firstMoveDone = true;
		if(board.playerColor == PieceColor.Black)
			board.playerColor = PieceColor.White;
		else
			board.playerColor = PieceColor.Black;
		return board;
	}


	public override string ToString ()
	{
		return "(" + fromX + "," + fromY + ")" + " to " + "(" + toX + "," + toY + ")";
	}


}
