using UnityEngine;
using System.Collections;
using GenericTurnBasedAI;

public class StatCollector : MonoBehaviour
{

	public int numGames;
	int gameCount = 0;
	GameState gameBoard;
	TurnEngine whiteAI;
	TurnEngine blackAI;
	bool whiteTurn = true;

	void Start()
	{
		whiteAI = new TurnEngineSingleThreaded(new ChessEvaluator(PieceColor.White),1,true,true);
		blackAI = new TurnEngineSingleThreaded(new ChessEvaluator(PieceColor.Black),1,true,true);
		whiteAI.TurnReadyEvent += ReceiveTurn;
		blackAI.TurnReadyEvent += ReceiveTurn;

		Restart();

	}

	void GetNextTurn()
	{
		if(whiteTurn) {
			StartCoroutine(whiteAI.GetNextTurn(gameBoard));
		} else {
			StartCoroutine(blackAI.GetNextTurn(gameBoard));
		}
		whiteTurn = !whiteTurn;
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

		gameBoard = turn.ApplyTurn(gameBoard);
		if(gameBoard.IsTerminal()) {
			GameEnd();
		} else
			GetNextTurn();
	}

	void GameEnd()
	{
		gameCount++;
		if(gameCount > numGames) {
			EngineStats whiteStats = whiteAI.Stats;
			EngineStats blackStats = blackAI.Stats;
			Debug.Log ("White Stats - " + whiteStats.ToString());
			Debug.Log ("Black Stats - " + blackStats.ToString());
		} else {
			Debug.Log ("Finished " + gameCount + " out of " + numGames + " games");
			Restart();
		}
	}

	void Restart()
	{
		whiteTurn = true;
		gameBoard = new ChessBoard(PieceColor.White);
		GetNextTurn();
	}

}

