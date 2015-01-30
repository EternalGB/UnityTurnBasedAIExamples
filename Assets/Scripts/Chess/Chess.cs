using UnityEngine;
using System.Collections.Generic;
using System;
using GenericTurnBasedAI;

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
	public GameObject restartButton;
	List<GameObject> lastPieces;
	ChessBoard gameBoard;

	TurnEngine whiteAI;
	TurnEngine blackAI;
	bool whiteTurn = true;
	bool waiting = false;
	bool minWait = false;
	public float minWaitingTime = 2;
	[Range(1,15)]
	public int whiteThinkingTime = 10;
	[Range(1,15)]
	public int blackThinkingTime = 10;

	void Start()
	{
		whiteAI = new TurnEngine(new ChessEvaluator(PieceColor.White),whiteThinkingTime,true,true);
		blackAI = new TurnEngine(new ChessEvaluator(PieceColor.Black),blackThinkingTime,true,true);
		whiteAI.TurnReadyEvent += ReceiveTurn;
		blackAI.TurnReadyEvent += ReceiveTurn;

		gameBoard = new ChessBoard(PieceColor.White);
		InitBoardDisplay(gameBoard);
		DrawBoard(gameBoard);
		PlayTurn();
	}

	void Update()
	{
		if(!gameBoard.IsTerminal()) {
			if(!waiting && !minWait)
				PlayTurn();
		} else {
			restartButton.SetActive(true);
		}
	}

	void PlayTurn()
	{
		if(whiteTurn) {
			StartCoroutine(whiteAI.GetNextTurn(gameBoard));
		} else {
			StartCoroutine(blackAI.GetNextTurn(gameBoard));
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
		Debug.Log(message);

		gameBoard = (ChessBoard)((ChessTurn)turn).ApplyTurn(gameBoard);
		DrawBoard(gameBoard);
		waiting = false;
	}

	public void Restart()
	{
		gameBoard = new ChessBoard(PieceColor.White);
		DrawBoard(gameBoard);
		PlayTurn();
		restartButton.SetActive(false);
	}

	void InitBoardDisplay(ChessBoard board)
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
				if(board.IsOccupied(x,y)) {
					ChessPiece piece = board.board[x,y];
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

