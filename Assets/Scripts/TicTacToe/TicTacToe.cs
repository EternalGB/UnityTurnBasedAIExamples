using UnityEngine;
using System.Collections.Generic;
using UniversalTurnBasedAI;

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
	public float minWaitingTime = 2;

	void Start()
	{
		lastPieces = new List<GameObject>();
		Restart();

		bottomLeft = -((gameBoard.Size- gridSize)/2)*Vector2.one;


		XAI = new TurnEngineMultiThreaded(new TTTEvaluator(TTTBoard.TTTPiece.X),10, false, false);
		OAI = new TurnEngineSingleThreaded(new TTTEvaluator(TTTBoard.TTTPiece.O),10, false, false);
		XAI.TurnReadyEvent += ReceiveTurn;
		OAI.TurnReadyEvent += ReceiveTurn;


	}

	void Update()
	{

		if(!gameBoard.IsTerminal()) {
			if(!waiting && !minWait)
				PlayTurn();
		} else {
			Restart();
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
		waiting = true;
		minWait = true;
		Invoke("CancelMinWait",minWaitingTime);
	}

	void ReceiveTurn(Turn turn)
	{

		//Debug.Log (gameBoard.ToString());
		gameBoard = (TTTBoard)((TTTTurn)turn).ApplyTurn(gameBoard);
		XTurn = !XTurn;
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

	void CancelMinWait()
	{
		minWait = false;
	}

	void Restart()
	{
		gameBoard = new TTTBoard(TTTBoard.TTTPiece.X);
		XTurn = true;
		waiting = false;
		minWait = false;
		CancelInvoke("CancelMinWait");
		DrawBoard();
	}
}

