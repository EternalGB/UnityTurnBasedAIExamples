using UnityEngine;
using System.Collections.Generic;

public class TicTacToe : MonoBehaviour
{

	public GameObject X, O;
	public float gridSize;
	TTTBoard gameBoard;
	TurnEngine XAI;
	TurnEngine OAI;

	List<GameObject> lastPieces;
	Vector2 bottomLeft;

	bool XTurn = true;
	bool waiting = false;
	bool minWait = false;
	float minWaitingTime = 2;

	void Start()
	{
		gameBoard = new TTTBoard(TTTBoard.TTTPiece.X);

		lastPieces = new List<GameObject>();
		bottomLeft = -((gameBoard.Size- gridSize)/2)*Vector2.one;


		XAI = new TurnEngine(new TTTEvaluator(TTTBoard.TTTPiece.X),10,false,true);
		OAI = new TurnEngine(new TTTEvaluator(TTTBoard.TTTPiece.O),10,false,true);
		XAI.TurnReadyEvent += ReceiveTurn;
		OAI.TurnReadyEvent += ReceiveTurn;
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
		if(XTurn) {
			StartCoroutine(XAI.GetNextTurn(gameBoard));
		} else {
			StartCoroutine(OAI.GetNextTurn(gameBoard));
		}
		XTurn = !XTurn;
		waiting = true;
		minWait = true;
		StartCoroutine(Timers.Countdown(minWaitingTime,() => minWait = false));
	}

	void ReceiveTurn(Turn turn)
	{
		//Debug.Log (gameBoard.ToString());
		gameBoard = (TTTBoard)((TTTTurn)turn).ApplyTurn(gameBoard);
		//Debug.Log (gameBoard.ToString());
		DrawBoard();
		waiting = false;
	}

	void DrawBoard()
	{
		foreach(GameObject piece in lastPieces)
			Destroy(piece);
		for(int i = 0; i < gameBoard.Size; i++) {
			Vector2 nextPos = bottomLeft + new Vector2((i%3)*gridSize, (i/3)*gridSize);
			if(gameBoard.GetPiece(i) == TTTBoard.TTTPiece.X) {
				lastPieces.Add((GameObject)Instantiate(X,nextPos,Quaternion.identity));
			} else if (gameBoard.GetPiece(i) == TTTBoard.TTTPiece.O) {
				lastPieces.Add((GameObject)Instantiate(O,nextPos,Quaternion.identity));
			}
		}
	}


}

