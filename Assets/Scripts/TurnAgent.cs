using UnityEngine;
using System.Collections;
using UniversalTurnBasedAI;

/// <summary>
/// Provides a common interface for Human, AI or possible Network players to connect
/// with a game. Also has the benefit of being attachable to a GameObject so you can
/// swap in and out players using the inspector.
/// </summary>
public abstract class TurnAgent : MonoBehaviour
{

	public delegate void TurnReady(ITurn bestTurn);
	public event TurnReady TurnReadyEvent;

	public abstract void Init(IGameState state);

	/// <summary>
	/// Generate the next turn. After this call has been made <see cref="OnTurnReady"/>
	/// should be used to return the Turn.
	/// </summary>
	/// <param name="state">State.</param>
	public abstract void GenerateNextTurn(IGameState state);

	/// <summary>
	/// Raises the turn ready event. Implementing classes should call this
	/// when they have a turn ready to play
	/// </summary>
	/// <param name="turn">Turn.</param>
	protected virtual void OnTurnReady(ITurn turn)
	{
		if(TurnReadyEvent != null)
			TurnReadyEvent(turn);
	}

}

