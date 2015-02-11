using UnityEngine;
using System.Collections;
using GenericTurnBasedAI;

public class ConnectKAIPlayer : TurnAgent
{

	public ConnectKPiece player;
	[Range(1,15)]
	public int timeLimit;
	TurnEngine engine;

	public override void Init (GameState state)
	{
		ConnectKBoard board = state as ConnectKBoard;
		int depthLimit = board.nRows*board.nCols;
		engine = new TurnEngineMultiThreaded(new ConnectKEvaluator(player,board.nCols,board.nRows,board.k),timeLimit,depthLimit,true);
		engine.TurnReadyEvent += HandleTurnReadyEvent;
	}

	void HandleTurnReadyEvent (Turn bestTurn)
	{
		OnTurnReady(bestTurn);
		Debug.Log (System.Enum.GetName(typeof(ConnectKPiece),player) + " : " + engine.Stats.ToString());
		engine.ResetStatisticsLog();
	}

	public override void GenerateNextTurn (GameState state)
	{
		StartCoroutine(engine.GetNextTurn(state));
	}

}

