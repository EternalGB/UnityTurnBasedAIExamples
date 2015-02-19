using System.Collections.Generic;

namespace UniversalTurnBasedAI
{
	/// <summary>
	/// Represents the current state of some game. Implement
	/// this interface to provide a <see cref="TurnEngine"/> with domain specific knowledge.
	/// The effiency of <see cref="IsTerminal"/>, <see cref="GeneratePossibleTurns"/> and
	/// <see cref="Clone"/> will all effect the performance of the <see cref="TurnEngine"/>.
	/// 
	/// Some information that is typically useful is: whose turn it is and some representation
	/// of the state of each component of the game. It can also be useful to track some meta-information
	/// about the state that can be used by an evaluation function rather than having the evaluation
	/// function compute the information every time. For example, counts of things within the GameState.
	/// <seealso cref="TurnEngine"/>
	/// <seealso cref="ITurn"/>
	/// <seealso cref="IEvaluator"/>
	/// </summary>
	public interface IGameState
	{

		/// <summary>
		/// Determines whether this GameState is terminal.
		/// </summary>
		/// <returns><c>true</c> If this GameState is terminal; otherwise, <c>false</c>.</returns>
		bool IsTerminal();

		/// <summary>
		/// A coroutine for generating every possible <see cref="ITurn"/>. Generating turns this way
		/// allows turns to be generated and search lazily. The efficiency of generating each turn
		/// effects the performance of the <see cref="TurnEngine"/>. You will only get the benefits
		/// of lazy evaluation if you yield each turn after generating them. Generating all turns and
		/// then iterating through them will negatively effect the performance of the <see cref="TurnEngine"/>.
		/// 
		/// This method should only produce turns that are valid for this particular GameState
		/// </summary>
		/// <returns>The possible turns.</returns>
		IEnumerable<ITurn> GeneratePossibleTurns();

		/// <summary>
		/// Returns an exact copy of the current GameState with new references. Any GameState information that
		/// is altered by a <see cref="ITurn"/> should be deep copied to prevent Turns writing over shared information. 
		/// 
		/// The efficiency of this method effects the performance of the <see cref="TurnEngine"/>, try to copy as little
		/// information as possible but remember that you will probably need new instances of any 
		/// reference types that can be altered by a <see cref="ITurn"/>.
		/// </summary>
		IGameState Clone();
		
	}
}



