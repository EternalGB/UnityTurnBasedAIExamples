using UnityEngine;
using System.Collections.Generic;
using System;
using GenericTurnBasedAI;

public class ChessEvaluator : Evaluator
{

	public PieceColor playerColor;
	public PieceColor oppColor;
	public Dictionary<PieceColor, Dictionary<PieceType, int>> pieceCounts;

	const float kingWeight = 200,
	queenWeight = 9,
	rookWeight = 5,
	bishopWeight = 3,
	knightWeight = 3,
	pawnWeight = 1,
	mobilityWeight = 0.1f;


	public ChessEvaluator(PieceColor playerColor)
	{
		pieceCounts = new Dictionary<PieceColor, Dictionary<PieceType, int>>();
		pieceCounts.Add(PieceColor.Black, new Dictionary<PieceType, int>());
		pieceCounts.Add(PieceColor.White, new Dictionary<PieceType, int>());
		foreach(PieceColor color in Enum.GetValues(typeof(PieceColor))) {
			foreach(PieceType type in Enum.GetValues(typeof(PieceType))) {
				if(type != PieceType.None)
					pieceCounts[color].Add(type,0);
			}
		}
		this.playerColor = playerColor;
		if(playerColor == PieceColor.White)
			oppColor = PieceColor.Black;
		else
			oppColor = PieceColor.White;

	}

	public ChessEvaluator(ChessEvaluator oldEval)
	{
		playerColor = oldEval.playerColor;
		oppColor = oldEval.oppColor;
		pieceCounts = new Dictionary<PieceColor, Dictionary<PieceType, int>>(oldEval.pieceCounts);
	}

	public override float Evaluate (GameState state)
	{

		ChessBoard board = (ChessBoard)state;
		foreach(PieceColor color in Enum.GetValues(typeof(PieceColor))) {
			foreach(PieceType type in Enum.GetValues(typeof(PieceType))) {
				if(type != PieceType.None)
					pieceCounts[color][type] = 0;
			}
		}
		float playerMobility = 0;
		float oppMobility = 0;
		for(int x = 0; x < board.size; x++) {
			for(int y = 0; y < board.size; y++) {
				if(board.IsOccupied(x,y)) {
					ChessPiece piece = board.board[x,y];
					pieceCounts[piece.color][piece.type] += 1;
					if(piece.color == playerColor)
						playerMobility += piece.GetPossibleMoves(board,x,y).Count;
					else
						oppMobility += piece.GetPossibleMoves(board,x,y).Count;
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
			default:
				break;
			}
		}
		value += mobilityWeight*(playerMobility - oppMobility);
		return value;
	}

	public override Evaluator Clone()
	{
		return new ChessEvaluator(this);
	}

}

