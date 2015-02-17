using UnityEngine;
using System.Collections.Generic;
using UniversalTurnBasedAI;

/// <summary>
/// Game controller for Connect K - a generalisation of Connect 4 where
/// board width and height and number in a row (k) can be defined. This
/// is just the Unity wrapper that handles all the GameObjects and coordinates
/// the playing of turns.
/// </summary>
public class ConnectK : MonoBehaviour 
{
	public GameObject gridSquare, winSquare;
	public float gridSize;
	public GameObject p1Piece, p2Piece;
	Vector2 bottomLeft;

	public int width, height, matches;

	ConnectKBoard board;
	List<GameObject> lastPieces;

	public TurnAgent p1;
	public TurnAgent p2;

	void Start()
	{
		//create our internal board representation
		board = new ConnectKBoard(width,height,matches);
		InitBoard();
		lastPieces = new List<GameObject>();

		//initialise our turn agents
		p1.Init(board);
		p2.Init(board);

		//register to receive and process turns
		p1.TurnReadyEvent += ReceiveTurn;
		p2.TurnReadyEvent += ReceiveTurn;
		//start the turn loop
		PlayTurn();
	}

	void PlayTurn()
	{
		//Generate the next player's turn
		if(board.player == ConnectKPiece.P1) {
			p1.GenerateNextTurn(board);
		} else {
			p2.GenerateNextTurn(board);
		}
	}

	void ReceiveTurn(ITurn turn)
	{
		//Apply the turn and redraw the board
		board = turn.ApplyTurn(board) as ConnectKBoard;
		DrawBoard();
		//check to see if the game is over
		if(board.IsTerminal()) {
			ConnectKPiece winner;
			if(board.player == ConnectKPiece.P1)
				winner = ConnectKPiece.P2;
			else
				winner = ConnectKPiece.P1;
			Line winLine = board.GetWinningLine();
			if(winLine != null)
				DrawVictory(winLine, winner);
			Debug.Log ("Game over");
		} else
			PlayTurn();
	}

	void InitBoard()
	{
		bottomLeft = -(new Vector2(width/2f - gridSize/2f,height/2f - gridSize/2f));
		for(int i = 0; i < width; i++) {
			for(int j = 0; j < height; j++) {
				Vector2 pos = GetRealPosition(i,j);
				GameObject square = (GameObject)Instantiate(gridSquare,pos,Quaternion.identity);
			}
		}
	}

	void DrawBoard()
	{
		foreach(GameObject piece in lastPieces)
			Destroy(piece);
		lastPieces.Clear();
		for(int x = 0; x < board.nCols; x++) {
			for(int y = 0; y < board.nRows; y++) {
				if(board.GetPiece(x,y) == ConnectKPiece.P1) {
					lastPieces.Add((GameObject)Instantiate(p1Piece, GetRealPosition(x,y), Quaternion.identity));
				} else if(board.GetPiece(x,y) == ConnectKPiece.P2){
					lastPieces.Add((GameObject)Instantiate(p2Piece, GetRealPosition(x,y), Quaternion.identity));
				}
			}
		}
	}

	void DrawVictory(Line line, ConnectKPiece winner)
	{
		ConnectKPiece lastPiece = board.GetPiece(line.start);
		int x = line.start.x + line.dir.x;
		int y = line.start.y + line.dir.y;
		int count = 0;
		IntVector2 winStart = line.start;
		if(lastPiece == winner) {
			count = 1;
		}
		while(x >= 0 && x < board.nCols && y >= 0 && y < board.nRows) {
			if(board.GetPiece(x,y) == winner) {
				if(lastPiece != winner)
					winStart = new IntVector2(x,y);
				count++;
			} else {
				if(count >= board.k)
					break;
				count = 0;
			}
			lastPiece = board.GetPiece(x,y);
			x += line.dir.x;
			y += line.dir.y;
		}
		if(count >= board.k) {
			x = winStart.x;
			y = winStart.y;
			for(int i = 0; i < count; i++) {
				Instantiate(winSquare, GetRealPosition(x,y), Quaternion.identity);
				x += line.dir.x;
				y += line.dir.y;
			}
		}
	}

	Vector2 GetRealPosition(int x, int y)
	{
		return bottomLeft + new Vector2(x*gridSize,y*gridSize);
	}

}
