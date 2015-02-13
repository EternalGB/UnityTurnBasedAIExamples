using UnityEngine;
using System.Collections;
using UniversalTurnBasedAI;

/// <summary>
/// Connect K random player. Useful for testing
/// </summary>
public class ConnectKRandomPlayer : TurnAgent
{

	public ConnectKPiece player;
	TurnEngine engine;
	
	public override void Init (GameState state)
	{
		//intialise turn engines
		ConnectKBoard board = state as ConnectKBoard;
		engine = new TurnEngineSingleThreaded(new EvaluatorRandom(-1,1),2,false);
		engine.TurnReadyEvent += HandleTurnReadyEvent;
	}
	
	/// <summary>
	/// Passes the turn ready event up from the TurnEngine 
	/// </summary>
	/// <param name="bestTurn">Best turn.</param>
	void HandleTurnReadyEvent (Turn bestTurn)
	{
		OnTurnReady(bestTurn);
		Debug.Log (System.Enum.GetName(typeof(ConnectKPiece),player) + " : " + engine.Stats.ToString());
		engine.ResetStatisticsLog();
	}
	
	/// <summary>
	/// Wrapper for the TurnEngine call
	/// </summary>
	/// <param name="state">State.</param>
	public override void GenerateNextTurn (GameState state)
	{
		StartCoroutine(engine.GetNextTurn(state));
	}

}

