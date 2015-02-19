using System.Collections.Generic;

namespace UniversalTurnBasedAI
{

	/// <summary>
	/// Represents the sum of actions that a player can take on their turn
	/// in the game.
	/// 
	/// <seealso cref="IGameState"/>
	/// <seealso cref="TurnEngine"/>
	/// </summary>
	public interface ITurn 
	{
		/// <summary>
		/// Applies this <see cref="ITurn"/> to <paramref name="state"/> giving the resulting <see cref="IGameState"/>.
		/// The <see cref="TurnEngine"/> clones <paramref name="state"/> before passing it to this function when called internally
		/// to prevent the original GameState from being overridden
		/// <seealso cref="IGameState"/>
		/// <seealso cref="TurnEngine"/>
		/// </summary>
		/// <returns>The GameState that is a result of applying this turn to <paramref name="state"/>.</returns>
		/// <param name="state">The state to apply the turn to.</param>
		IGameState ApplyTurn(IGameState state);
	}
}



