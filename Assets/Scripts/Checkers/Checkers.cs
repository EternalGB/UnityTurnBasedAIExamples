using UnityEngine;
using System.Collections.Generic;

public class Checkers : MonoBehaviour
{

	CheckersBoard board;

	public GameObject whitePiece, blackPiece;
	public GameObject boardSquare;
	public Color black, white;
	Vector2 bottomLeft;
	public float squareSize;
	List<GameObject> lastPieces;

	void Start()
	{
		board = new CheckersBoard(10);
		InitBoard(board);
		lastPieces = new List<GameObject>();
		DrawBoard(board);
	}

	void InitBoard(CheckersBoard board)
	{
		bottomLeft = -((board.size- squareSize)/2)*Vector2.one;
		for(int x = 0; x < board.size; x++) {
			for(int y = 0; y < board.size; y++) {
				GameObject square = (GameObject)Instantiate(boardSquare,GetRealPosition(x,y),Quaternion.identity);
				SpriteRenderer sr = square.GetComponent<SpriteRenderer>();
				if(x%2 == 0 && y%2 == 0)
					sr.color = black;
				if(x%2 == 0 && y%2 != 0)
					sr.color = white;
				if(x%2 != 0 && y%2 == 0)
					sr.color = white;
				if(x%2 != 0 && y%2 != 0)
					sr.color = black;
			}
		}
	}

	void DrawBoard(CheckersBoard board)
	{
		foreach(GameObject piece in lastPieces)
			Destroy(piece);
		for(int x = 0; x < board.size; x++) {
			for(int y = 0; y < board.size; y++) {
				if(board.GetPiece(x,y) == CheckersBoard.Piece.Black)
					lastPieces.Add((GameObject)Instantiate(blackPiece,GetRealPosition(x,y),Quaternion.identity));
				if(board.GetPiece(x,y) == CheckersBoard.Piece.White)
					lastPieces.Add((GameObject)Instantiate(whitePiece,GetRealPosition(x,y),Quaternion.identity));
			}
		}
	}

	Vector2 GetRealPosition(int x, int y)
	{
		return bottomLeft + new Vector2(x*squareSize,y*squareSize);
	}

}

