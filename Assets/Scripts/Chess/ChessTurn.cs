using UnityEngine;
using System;
using System.Collections.Generic;

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
		board.board[toX,toY] = board.board[fromX,fromY];
		board.board[fromX,fromY] = ChessPiece.Empty;
		ChessPiece piece = board.board[toX,toY];

		//check for existing enPassant
		if(board.enPassant != null) {
			foreach(int[] enPassant in board.enPassant) {
				if(enPassant[0] == fromX && enPassant[1] == fromY) {
					//did this pawn move forwards or backwards
					if(fromY < toY) {
						board.board[toX,toY-1] = ChessPiece.Empty;
					} else {
						board.board[toX,toY+1] = ChessPiece.Empty;
					}
				}
			}

		}

		board.enPassant = null;
		if(piece != null) {
			piece.firstMoveDone = true;

			if(piece.type == PieceType.Pawn) {
				//if there is a pawn at the bottom or top of the board, promote to queen
				if(toY == 0 || toY == board.size-1) 
					piece.type = PieceType.Queen;
				//check for newly created en passant
				if(Math.Abs(fromY - toY) == 2) {
					int left = toX-1;
					int right = toX+1;
					board.enPassant = new List<int[]>();
					if(left >= 0 && left < board.size && board.IsOccupied(left,toY) && board.board[left,toY].type == PieceType.Pawn)
						board.enPassant.Add(new int[]{left,toY});
					if(right >= 0 && right < board.size && board.IsOccupied(right,toY) && board.board[right,toY].type == PieceType.Pawn)
						board.enPassant.Add(new int[]{right,toY});
				}
			}
		}



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
