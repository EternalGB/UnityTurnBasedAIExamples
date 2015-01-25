using System.Collections.Generic;
using System;

public class ChessEvaluator : Evaluator
{

	PieceColor playerColor;
	PieceColor oppColor;
	Dictionary<PieceColor, Dictionary<PieceType, int>> pieceCounts;

	const float kingWeight = 200,
	queenWeight = 9,
	rookWeight = 5,
	bishopWeight = 3,
	knightWeight = 3,
	pawnWeight = 1;

	const int numPawns = 8, 
	numBishops = 2,
	numRooks = 2,
	numKnights = 2,
	numQueens = 1,
	numKings = 1;

	public ChessEvaluator(PieceColor playerColor)
	{
		pieceCounts = new Dictionary<PieceColor, Dictionary<PieceType, int>>();
		pieceCounts.Add(PieceColor.Black, new Dictionary<PieceType, int>());
		pieceCounts.Add(PieceColor.White, new Dictionary<PieceType, int>());
		foreach(PieceColor color in Enum.GetValues(typeof(PieceColor))) {
			foreach(PieceType type in Enum.GetValues(typeof(PieceType))) {
				int numToAdd = 1;
				switch (type) {
				case PieceType.Pawn:
					numToAdd = numPawns;
					break;
				case PieceType.Rook:
					numToAdd = numRooks;
					break;
				case PieceType.Bishop:
					numToAdd = numBishops;
					break;
				case PieceType.Knight:
					numToAdd = numKnights;
					break;
				case PieceType.Queen:
					numToAdd = numQueens;
					break;
				case PieceType.King:
					numToAdd = numKings;
					break;
				}
				pieceCounts[color].Add(type,numToAdd);
			}
		}
		this.playerColor = playerColor;
		if(playerColor == PieceColor.White)
			oppColor = PieceColor.Black;
		else
			oppColor = PieceColor.White;
	}

	public override float Evaluate (GameState state)
	{

		ChessBoard board = (ChessBoard)state;
		for(int x = 0; x < board.size; x++) {
			for(int y = 0; y < board.size; y++) {
				if(board.board[x,y].HasPiece()) {
					ChessPiece piece = board.board[x,y].piece;
					pieceCounts[piece.color][piece.type] += 1;
				}
			}
		}
		float value = 0;
		foreach(PieceType type in Enum.GetValues(typeof(PieceType))) {
			switch (type) {
			case PieceType.Pawn:
				value += pawnWeight*(pieceCounts[playerColor][type] - pieceCounts[oppColor][type]);
				break;
			case PieceType.Rook:
				value += rookWeight*(pieceCounts[playerColor][type] - pieceCounts[oppColor][type]);
				break;
			case PieceType.Bishop:
				value += bishopWeight*(pieceCounts[playerColor][type] - pieceCounts[oppColor][type]);
				break;
			case PieceType.Knight:
				value += knightWeight*(pieceCounts[playerColor][type] - pieceCounts[oppColor][type]);
				break;
			case PieceType.Queen:
				value += queenWeight*(pieceCounts[playerColor][type] - pieceCounts[oppColor][type]);
				break;
			case PieceType.King:
				value += kingWeight*(pieceCounts[playerColor][type] - pieceCounts[oppColor][type]);
				break;
			}
		}
		return value;
	}

}

