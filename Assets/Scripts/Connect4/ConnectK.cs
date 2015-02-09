using UnityEngine;
using System.Collections.Generic;
using GenericTurnBasedAI;

public class ConnectK : MonoBehaviour 
{
	public GameObject gridSquare;
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
		board = new ConnectKBoard(width,height,matches);
		InitBoard();
		lastPieces = new List<GameObject>();

		p1.Init(board);
		p2.Init(board);

		p1.TurnReadyEvent += ReceiveTurn;
		p2.TurnReadyEvent += ReceiveTurn;
		PlayTurn();
	}

	void PlayTurn()
	{
		if(board.player == ConnectKPiece.P1) {
			p1.GenerateNextTurn(board);
		} else {
			p2.GenerateNextTurn(board);
		}
	}

	void ReceiveTurn(Turn turn)
	{
		board = turn.ApplyTurn(board) as ConnectKBoard;
		PlayTurn();
	}

	void InitBoard()
	{
		//gridSize = gridSquare.renderer.bounds.size.x;
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
				GameObject piece;
				if(board.GetPiece(x,y) == ConnectKPiece.P1) {
					piece = (GameObject)Instantiate(p1Piece, GetRealPosition(x,y), Quaternion.identity);
				} else {
					piece = (GameObject)Instantiate(p2Piece, GetRealPosition(x,y), Quaternion.identity);
				}
				lastPieces.Add(piece);
			}
		}
	}

	Vector2 GetRealPosition(int x, int y)
	{
		return bottomLeft + new Vector2(x*gridSize,y*gridSize);
	}

}
