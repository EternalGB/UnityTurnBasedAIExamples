using UnityEngine;
using System.Collections.Generic;
using System;

public class Chess : MonoBehaviour
{

	public List<GameObject> whitePieces;
	public List<GameObject> blackPieces;
	Dictionary<PieceType,GameObject> whiteDict;
	Dictionary<PieceType,GameObject> blackDict;
	public GameObject boardSquare;
	public Color black, white;
	public Vector2 bottomLeft;
	public float squareSize;
	List<GameObject> lastPieces;
	ChessBoard gameBoard;

	TurnEngine whiteAI;
	TurnEngine blackAI;
	bool whiteTurn = true;
	bool waiting = false;
	bool minWait = false;
	float minWaitingTime = 2;

	void Start()
	{
		gameBoard = new ChessBoard(PieceColor.White);
		InitBoard(gameBoard);
		DrawBoard(gameBoard);
		whiteAI = new TurnEngine(4,new ChessEvaluator(PieceColor.White));
		blackAI = new TurnEngine(1,new ChessEvaluatorRandom(0,200));
		whiteAI.TurnReadyEvent += ReceiveTurn;
		blackAI.TurnReadyEvent += ReceiveTurn;
	}

	void Update()
	{
		if(!gameBoard.IsTerminal()) {
			if(!waiting && !minWait)
				PlayTurn();
		} else {
			Debug.Log ("Game over!");
		}
	}

	void PlayTurn()
	{
		//Debug.Log ("Starting Turn");
		if(whiteTurn) {
			gameBoard.playerColor = PieceColor.White;
			StartCoroutine(whiteAI.IterativeMinimax(gameBoard));
		} else {
			gameBoard.playerColor = PieceColor.Black;
			StartCoroutine(blackAI.IterativeMinimax(gameBoard));
		}
		whiteTurn = !whiteTurn;
		waiting = true;
		minWait = true;
		StartCoroutine(Timers.Countdown(minWaitingTime,() => minWait = false));
	}

	void ReceiveTurn(Turn turn)
	{
		string message = "";
		if(whiteTurn) {
			message += "Black moves ";
		} else {
			message += "White moves ";
		}
		message += turn.ToString();
		//Debug.Log(message);
		//Debug.Log (gameBoard.ToString());
		gameBoard = (ChessBoard)((ChessTurn)turn).ApplyTurn(gameBoard);
		//Debug.Log (gameBoard.ToString());
		DrawBoard(gameBoard);
		waiting = false;
	}

	void InitBoard(ChessBoard board)
	{
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
		whiteDict = new Dictionary<PieceType, GameObject>();
		blackDict = new Dictionary<PieceType, GameObject>();
		foreach(PieceType type in Enum.GetValues(typeof(PieceType))) {
			whiteDict.Add(type,FindPiece(whitePieces,type));
			blackDict.Add(type,FindPiece(blackPieces,type));
		}
		lastPieces = new List<GameObject>();
	}

	void DrawBoard(ChessBoard board)
	{
		foreach(GameObject piece in lastPieces)
			Destroy(piece);
		for(int x = 0; x < board.size; x++) {
			for(int y = 0; y < board.size; y++) {
				if(board.board[x,y].IsOccupied()) {
					ChessPiece piece = board.board[x,y].piece;
					if(piece.color == PieceColor.White)
						lastPieces.Add((GameObject)Instantiate(whiteDict[piece.type],GetRealPosition(x,y),Quaternion.identity));
					else
						lastPieces.Add((GameObject)Instantiate(blackDict[piece.type],GetRealPosition(x,y),Quaternion.identity));
				}
			}
		}
	}

	Vector2 GetRealPosition(int x, int y)
	{
		return bottomLeft + new Vector2(x*squareSize,y*squareSize);
	}

	GameObject FindPiece(List<GameObject> pieces, PieceType type)
	{
		string name = Enum.GetName(typeof(PieceType),type);
		foreach(GameObject piece in pieces)
			if(piece.name.Equals(name))
				return piece;
		return null;
	}

}
