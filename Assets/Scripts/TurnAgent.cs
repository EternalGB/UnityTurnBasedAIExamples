using UnityEngine;
using System.Collections;
using GenericTurnBasedAI;

public abstract class TurnAgent : MonoBehaviour
{

	public delegate void TurnReady(Turn bestTurn);
	public event TurnReady TurnReadyEvent;

	public abstract void Init(GameState state);

	public abstract void GenerateNextTurn(GameState state);

	protected virtual void OnTurnReady(Turn turn)
	{
		if(TurnReadyEvent != null)
			TurnReadyEvent(turn);
	}

}

