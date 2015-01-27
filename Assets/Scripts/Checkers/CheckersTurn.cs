using System.Collections;

public class CheckersTurn : Turn
{

	int fromX, fromY, toX, toY;

	public CheckersTurn (int fromX, int fromY, int toX, int toY)
	{
		this.fromX = fromX;
		this.fromY = fromY;
		this.toX = toX;
		this.toY = toY;
	}

	public override GameState ApplyTurn (GameState state)
	{
		CheckersBoard board = (CheckersBoard)state.Clone();
		board.MovePiece(fromX,fromY,toX,toY);
		return board;
	}
	
}

