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
	GameObject[,] squares;
	public Color black, white, red, green;
	public Vector2 bottomLeft;
	public float squareSize;
	public GameObject restartButton;
	List<GameObject> lastPieces;
	ChessBoard gameBoard;

	TurnEngine whiteAI;
	TurnEngine blackAI;
	bool WhiteTurn
	{
		get
		{
			if(gameBoard != null)
				return gameBoard.playerColor == PieceColor.White;
			else
				return false;
		}
	}
	bool waiting = false;
	bool minWait = false;
	public float minWaitingTime = 2;

	[Range(1,15)]
	public int whiteThinkingTime = 10;
	[Range(1,15)]
	public int blackThinkingTime = 10;

	void Start()
	{

		whiteAI = new TurnEngineSingleThreaded(new ChessEvaluator(PieceColor.White),whiteThinkingTime,true, true);
		blackAI = new TurnEngineMultiThreaded(new ChessEvaluator(PieceColor.Black),blackThinkingTime,true,4,4,true);
		whiteAI.TurnReadyEvent += ReceiveTurn;
		blackAI.TurnReadyEvent += ReceiveTurn;

		gameBoard = new ChessBoard(PieceColor.White);
		InitBoardDisplay(gameBoard);
		DrawBoard(gameBoard, null);
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
		if(WhiteTurn) {
			StartCoroutine(whiteAI.GetNextTurn(gameBoard));
		} else {
			StartCoroutine(blackAI.GetNextTurn(gameBoard));
		}

		waiting = true;
		minWait = true;
		Invoke("CancelMinWait",minWaitingTime);
	}

	void ReceiveTurn(Turn turn)
	{
		string statMessage = "";
		if(WhiteTurn) {
			statMessage += "White Stats: ";
			statMessage += whiteAI.Stats.ToString();
		} else {
			statMessage += "Black Stats: ";
			statMessage += blackAI.Stats.ToString();
		}
		Debug.Log (statMessage);

		string message = "";
		if(WhiteTurn) {
			message += "White moves ";
		} else {
			message += "Black moves ";
		}
		message += turn.ToString();
		Debug.Log(message);

		gameBoard = (ChessBoard)(turn as ChessTurn).ApplyTurn(gameBoard);
		//whiteTurn = !whiteTurn;
		DrawBoard(gameBoard, turn as ChessTurn);
		waiting = false;
	}

	void CancelMinWait()
	{
		minWait = false;
	}

	public void Restart()
	{
		//whiteTurn = true;
		gameBoard = new ChessBoard(PieceColor.White);
		DrawBoard(gameBoard, null);
		PlayTurn();
		restartButton.SetActive(false);
	}

	void OnDisable()
	{
		if(whiteAI != null)
			whiteAI.Stop();
		if(blackAI != null)
			blackAI.Stop();
	}

	void InitBoardDisplay(ChessBoard board)
	{
		squares = new GameObject[board.size,board.size];
		for(int x = 0; x < board.size; x++) {
			for(int y = 0; y < board.size; y++) {
				GameObject square = (GameObject)Instantiate(boardSquare,GetRealPosition(x,y),Quaternion.identity);
				square.GetComponent<SpriteRenderer>().color = GetSquareColor(x,y);

				squares[x,y] = square;
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

	void DrawBoard(ChessBoard board, ChessTurn lastTurn)
	{
		foreach(GameObject piece in lastPieces)
			Destroy(piece);
		for(int x = 0; x < board.size; x++) {
			for(int y = 0; y < board.size; y++) {
				squares[x,y].GetComponent<SpriteRenderer>().color = GetSquareColor(x,y);
				if(board.IsOccupied(x,y)) {
					ChessPiece piece = board.board[x,y];
					if(piece.color == PieceColor.White)
						lastPieces.Add((GameObject)Instantiate(whiteDict[piece.type],GetRealPosition(x,y),Quaternion.identity));
					else
						lastPieces.Add((GameObject)Instantiate(blackDict[piece.type],GetRealPosition(x,y),Quaternion.identity));
				}

			}
		}
		if(lastTurn != null) {
			squares[lastTurn.fromX, lastTurn.fromY].GetComponent<SpriteRenderer>().color = red;
			squares[lastTurn.toX, lastTurn.toY].GetComponent<SpriteRenderer>().color = green;
		}
	}

	Vector2 GetRealPosition(int x, int y)
	{
		return bottomLeft + new Vector2(x*squareSize,y*squareSize);
	}

	Color GetSquareColor(int x, int y)
	{
		if(x%2 == 0 && y%2 == 0)
			return black;
		if(x%2 == 0 && y%2 != 0)
			return white;
		if(x%2 != 0 && y%2 == 0)
			return white;
		if(x%2 != 0 && y%2 != 0)
			return black;
		return Color.white;
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

