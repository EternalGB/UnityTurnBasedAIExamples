using System.Collections.Generic;
using GenericTurnBasedAI;

public class CheckersBoard : GameState
{

	public enum Piece
	{
		None,White,Black
	}

	public int size;
	Piece[,] board;

	public CheckersBoard(int size)
	{
		this.size = size;
		board = new Piece[size,size];
		for(int x = 0; x < size ; x++) {
			for(int y = 0; y < size; y++) {
				board[x,y] = Piece.None;
				if((x%2 != 0 && y%2 != 0) || (x%2 == 0 && y%2 == 0)) {
					if(y >= 0 && y < 4) {
						board[x,y] = Piece.White;
					} else if (y >= 6 && y < size) {
						board[x,y] = Piece.Black;
					}
				}
			}
		}
	}

	public CheckersBoard(Piece[,] board)
	{
		size = board.Length;
		this.board = new Piece[size,size];
		for(int x = 0; x < size ; x++) {
			for(int y = 0; y < size; y++) {
				this.board[x,y] = board[x,y]; 
			}
		}
	}



	public Piece GetPiece(int x, int y)
	{
		return board[x,y];
	}

	public void MovePiece(int fromX, int fromY, int toX, int toY)
	{
		board[toX,toY] = board[fromX,fromY];
		board[fromX,fromY] = Piece.None;
	}

	public bool IsOccupied(int x, int y)
	{
		return board[x,y] != Piece.None;
	}

	public override IEnumerable<Turn> GeneratePossibleTurns ()
	{
		throw new System.NotImplementedException ();
	}

	public override bool IsTerminal ()
	{
		int blackCount = 0;
		int whiteCount = 0;
		for(int x = 0; x < size ; x++) {
			for(int y = 0; y < size; y++) {
				if(board[x,y] == Piece.White)
					whiteCount++;
				else if(board[x,y] == Piece.Black)
					blackCount++;
			}
		}
		if(blackCount == 0 || whiteCount == 0)
			return true;



		return false;
	}

	public override GameState Clone ()
	{
		return new CheckersBoard(board);
	}

}
