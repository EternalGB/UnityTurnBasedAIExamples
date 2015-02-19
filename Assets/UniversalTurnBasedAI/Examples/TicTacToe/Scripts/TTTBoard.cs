using UniversalTurnBasedAI;

/// <summary>
/// Internal representation of our Tic Tac Toe board
/// </summary>
public class TTTBoard : IGameState
{

	public enum TTTPiece
	{
		None, X, O
	}

	TTTPiece[] board;
	public TTTPiece player;
	public int Size
	{
		get; private set;
	}

	public TTTBoard(TTTPiece player)
	{
		Size = 9;
		board = new TTTPiece[Size];
		this.player = player;
	}

	public TTTBoard(TTTBoard oldBoard)
	{
		Size = oldBoard.Size;
		board = new TTTPiece[Size];

		for(int i = 0; i < board.Length; i++) {
			board[i] = oldBoard.board[i];
		}
		player = oldBoard.player;
	}


	static int[][] lines = new int[][] {
		new int[] {0,1,2},
		new int[] {3,4,5},
		new int[] {6,7,8},
		new int[] {0,3,6},
		new int[] {1,4,7},
		new int[] {2,5,8},
		new int[] {0,4,8},
		new int[] {2,4,6}
	};

	public void SetPiece(int pos, TTTPiece piece)
	{
		board[pos] = piece;
	}

	public TTTPiece GetPiece(int pos)
	{
		return board[pos];
	}

	public void SetPlayer(TTTPiece player)
	{
		this.player = player;
	}

	public bool IsTerminal ()
	{
		bool boardFull = true;
		bool match = false;
		foreach(int[] line in lines) {
			if(board[line[0]] != TTTPiece.None)
				match = match || (board[line[0]] == board[line[1]] && board[line[1]] == board[line[2]]);
		}
		for(int i = 0; i < board.Length; i++)
			boardFull = boardFull && board[i] != TTTPiece.None;
		return boardFull || match;
	}

	public bool Winner(TTTPiece player)
	{
		bool winner = false;
		foreach(int[] line in lines) {
			winner = winner || 
				(board[line[0]] == player && board[line[1]] == player && board[line[2]] == player);
		}
		return winner;
	}

	public bool Loser(TTTPiece player)
	{
		if(player == TTTPiece.O)
			player = TTTPiece.X;
		else
			player = TTTPiece.O;
		return Winner(player);
	}

	public System.Collections.Generic.IEnumerable<ITurn> GeneratePossibleTurns ()
	{
		for(int i = 0; i < Size; i++) {
			if(board[i] == TTTPiece.None)
				yield return new TTTTurn(i,player);
		}
	}

	public IGameState Clone ()
	{
		return new TTTBoard(this);
	}


}
