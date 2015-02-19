using UnityEngine;
using System.Collections;
using UniversalTurnBasedAI;

/// <summary>
/// The AI player Monobehaviour for Connect K
/// </summary>
public class ConnectKAIPlayer : TurnAgent
{

	public ConnectKPiece player;
	public bool multiThreaded = false;
	[Range(0.1f,15)]
	public float timeLimit;
	TurnEngine engine;

	public override void Init (IGameState state)
	{
		//intialise turn engines
		ConnectKBoard board = state as ConnectKBoard;
		int depthLimit = board.nRows*board.nCols; //there can only ever be width*height possible moves so no need to search further
		if(multiThreaded)
			engine = new TurnEngineMultiThreaded(new ConnectKEvaluator(player,board.nCols,board.nRows,board.k),timeLimit,depthLimit,true);
		else
			engine = new TurnEngineSingleThreaded(new ConnectKEvaluator(player,board.nCols,board.nRows,board.k),timeLimit,depthLimit,true);
		//Register to pass the turn ready even up
		engine.TurnReadyEvent += HandleTurnReadyEvent;
	}
	

	/// <summary>
	/// Passes the turn ready event up from the TurnEngine 
	/// </summary>
	/// <param name="bestTurn">Best turn.</param>
	void HandleTurnReadyEvent (ITurn bestTurn)
	{
		Debug.Log (System.Enum.GetName(typeof(ConnectKPiece),player) + " : " + engine.Stats.ToString());
		OnTurnReady(bestTurn);

		engine.ResetStatisticsLog();
	}

	/// <summary>
	/// Wrapper for the TurnEngine call
	/// </summary>
	/// <param name="state">State.</param>
	public override void GenerateNextTurn (IGameState state)
	{
		StartCoroutine(engine.GetNextTurn(state));
	}

}

