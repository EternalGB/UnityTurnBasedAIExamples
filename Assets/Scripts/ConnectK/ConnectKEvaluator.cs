using UnityEngine;
using System.Collections.Generic;
using UniversalTurnBasedAI;
using System;

/// <summary>
/// Connect K evaluator. Uses a weighted sum approach, counting the number of pieces that
/// the player could potential connect on each row and the maximum number of connected pieces
/// on each line where there is still room to play a piece. Then subtracts the sum of the
/// opponent's count.
/// 
/// If the player wins on the evaluated board, gives <see cref="maxValue"/>, if the opponent 
/// wins gives <see cref="minValue"/>.
/// </summary>
public class ConnectKEvaluator : Evaluator
{

	const float 
		connectableWeight = 1f,
		maxChainWeight = 1;	

	ConnectKPiece player;
	ConnectKPiece opponent;
	List<Line> allLines;
	int k;

	public ConnectKEvaluator(ConnectKPiece player, int width, int height, int k)
	{
		allLines = ConnectKBoard.GetLines(width,height,k);
		this.player = player;
		if(player == ConnectKPiece.P1)
			opponent = ConnectKPiece.P2;
		else
			opponent = ConnectKPiece.P1;
		this.k = k;
		maxValue = width*height*k;
		minValue = -maxValue;
	}

	public override float Evaluate (IGameState state)
	{
		ConnectKBoard board = state as ConnectKBoard;
		Dictionary<Line, float> playerCount;
		Dictionary<Line, float> oppCount;
		if(player == ConnectKPiece.P1) {
			playerCount = board.p1Count;
			oppCount = board.p2Count;
		} else {
			playerCount = board.p2Count;
			oppCount = board.p1Count;
		}

		float score = 0;
		foreach(Line line in allLines) {
			if(playerCount[line] > 0) {
				int connectable = GetConnectablePieces(player,line,board);
				int maxChain = GetMaxChainLength(player,line,board);
				if(maxChain >= k) {
					return maxValue;
				} else {
					score += connectable*connectableWeight + maxChainWeight*maxChain;
				}
			}
			if(oppCount[line] > 0) {
				int connectable = GetConnectablePieces(opponent,line,board);
				int maxChain = GetMaxChainLength(opponent,line,board);
				if(maxChain >= k) {
					return minValue;
				} else {
					score -= connectable*connectableWeight + maxChainWeight*maxChain;
				}
			}
		}
		return score;
	}

	//returns the length of the chain of type piece. If there is not an open space
	//at either end of the chain, returns 0
	int GetMaxChainLength(ConnectKPiece piece, Line line, ConnectKBoard board)
	{
		int x = line.start.x;
		int y = line.start.y;
		int max = 0;
		int count = 0;
		bool lastChainStartOpen = false;
		while(x >= 0 && x < board.nCols && y >= 0 && y < board.nRows) {
			if(board.GetPiece(x,y) == piece) {
				count++;
				if(count == 1) {
					int startX = x - line.dir.x;
					int startY = y - line.dir.y;
					if(board.IsEmpty(startX,startY))
						lastChainStartOpen = true;
					else
						lastChainStartOpen = false;
				}
			} else if(board.GetPiece(x,y) == ConnectKPiece.None || lastChainStartOpen) {
				max = Math.Max(max,count);
				count = 0;
			} else {
				count = 0;
			}
			//covers the case that we have filled the whole line with only our piece and won
			if(count >= k)
				return count;
			x += line.dir.x;
			y += line.dir.y;
		}
		return max;
	}

	//measures the total number of pieces that are connectable on this line
	//a piece is connectable as long as there is no opponent piece in the way
	int GetConnectablePieces(ConnectKPiece piece, Line line, ConnectKBoard board)
	{
		int x = line.start.x;
		int y = line.start.y;
		int total = 0;
		int count = 0;
		while(x >= 0 && x < board.nCols && y >= 0 && y < board.nRows) {
			if(board.GetPiece (x,y) == piece) {
				count++;
			} else if(board.GetPiece(x,y) != ConnectKPiece.None) {
				total += count;
				count = 0;
			}
			x += line.dir.x;
			y += line.dir.y;
		}
		return total;
	}

}

