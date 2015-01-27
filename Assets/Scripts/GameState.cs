using UnityEngine;
using System.Collections.Generic;

public abstract class GameState
{

	public abstract bool IsTerminal();
	
	public abstract IEnumerable<Turn> GeneratePossibleTurns();

	public abstract GameState Clone();

}

