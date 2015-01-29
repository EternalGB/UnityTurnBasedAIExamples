using System.Collections.Generic;

public class ChessBoard : GameState
{

	public ChessPiece[,] board;
	public int size = 8;
	public PieceColor playerColor;
	//holds the position of any current enPassant pawn (can only ever be two max)
	public List<int[]> enPassant;

	public ChessBoard(PieceColor playerColor)
	{
		board = new ChessPiece[size,size];
		enPassant = null;
		this.playerColor = playerColor;
		InitPieces();
	}

	public ChessBoard(ChessBoard oldBoard)
	{
		board = new ChessPiece[size,size];
		playerColor = oldBoard.playerColor;
		size = oldBoard.size;
		if(oldBoard.enPassant != null)
			enPassant = new List<int[]>(oldBoard.enPassant);
		else
			enPassant = null;
		for(int x = 0; x < size; x++) {
			for(int y = 0; y < size; y++) {
				if(oldBoard.IsOccupied(x,y))
					board[x,y] = oldBoard.board[x,y].Clone();
				else
					board[x,y] = oldBoard.board[x,y];
			}
		}
	}

	void InitPieces()
	{
		for(int x = 0; x < size; x++) {
			for(int y = 0; y < size; y++) {
				board[x,y] = ChessPiece.Empty;
			}
		}
		//do top and bottom rows
		int[] topAndBottomRows = new int[]{0,size-1};
		foreach(int y in topAndBottomRows) {
			PieceColor color;
			if(y == 0) {
				color = PieceColor.White;
			} else {
				color = PieceColor.Black;
			}
			board[0,y] = new ChessPiece(PieceType.Rook,color);
			board[1,y] = new ChessPiece(PieceType.Knight,color);
			board[2,y] = new ChessPiece(PieceType.Bishop,color);
			board[3,y] = new ChessPiece(PieceType.Queen,color);
			board[4,y] = new ChessPiece(PieceType.King,color);
			board[5,y] = new ChessPiece(PieceType.Bishop,color);
			board[6,y] = new ChessPiece(PieceType.Knight,color);
			board[7,y] = new ChessPiece(PieceType.Rook,color);
		}
		for(int x = 0; x < size; x++) {
			board[x,1] = new ChessPiece(PieceType.Pawn,PieceColor.White);
			board[x,size-2] = new ChessPiece(PieceType.Pawn,PieceColor.Black);
		}

	}

	public override IEnumerable<Turn> GeneratePossibleTurns()
	{
		for(int x = 0; x < size; x++) {
			for(int y = 0; y < size; y++) {
				if(IsOccupied(x,y) && board[x,y].color == playerColor) {
					List<ChessTurn> chessTurns = board[x,y].GetPossibleMoves(this,x,y);
					foreach(ChessTurn ct in chessTurns)
						yield return (Turn)ct;
				}
			}
		}
	}

	public override bool IsTerminal ()
	{
		//bad first one: terminal when king captured
		ChessPiece whiteKing = new ChessPiece(PieceType.King, PieceColor.White);
		ChessPiece blackKing = new ChessPiece(PieceType.King, PieceColor.Black);
		return !ContainsPiece(whiteKing) || !ContainsPiece(blackKing);

		//TODO check detection

		/*
		ChessPiece whiteKing = new ChessPiece(PieceType.King, PieceColor.White, false);
		ChessPiece blackKing = new ChessPiece(PieceType.King, PieceColor.Black, false);
		int[] whiteLoc = FindPieceLocationOnBoard(whiteKing);
		int[] blackLoc = FindPieceLocationOnBoard(blackKing);
		return whiteKing.GetPossibleMoves(this,whiteLoc[0],whiteLoc[1]).Count == 0 || blackKing.GetPossibleMoves(this,blackLoc[0],blackLoc[1]).Count == 0;
		*/
		//TODO stalemates
	}

	public override GameState Clone ()
	{
		return new ChessBoard(this);
	}

	public bool OnBoard(int x, int y)
	{
		return x >= 0 && x < size && y >= 0 && y < size;
	}

	public bool IsOccupied(int x, int y)
	{
		return OnBoard(x,y) && board[x,y].type != PieceType.None;
	}

	bool ContainsPiece(ChessPiece piece)
	{
		for(int x = 0; x < size; x++) {
			for(int y = 0; y < size; y++) {
				if(IsOccupied(x,y) && piece.Equals(board[x,y]))
					return true;
			}
		}
		return false;
	}

	int[] FindPieceLocationOnBoard(ChessPiece piece)
	{
		for(int x = 0; x < size; x++) {
			for(int y = 0; y < size; y++) {
				ChessPiece next = board[x,y];
				if(next != null && next.type == piece.type && next.color == piece.color) {
					return new int[]{x,y};
				}
			}
		}
		return null;
	}

	public override string ToString ()
	{
		string message = "";
		for(int y = size-1; y >= 0; y--) {
			for(int x = 0; x < size; x++) {
				if(IsOccupied(x,y)) {
					ChessPiece next = board[x,y];
					message += "<b>";
					if(next.color == PieceColor.Black)
						message += "<i>";
					switch (next.type) {
						case PieceType.Pawn:
						message += "P";
						break;
						case PieceType.Rook:
						message += "R";
						break;
						case PieceType.Bishop:
						message += "B";
						break;
						case PieceType.Knight:
						message += "N";
						break;
						case PieceType.Queen:
						message += "Q";
						break;
						case PieceType.King:
						message += "K";
						break;
						default:
						throw new System.ArgumentOutOfRangeException ();
					}

					if(next.color == PieceColor.Black)
						message += "</i>";
					message += "</b>";
				} else {
					message += "E";
				}
				message += "\t";
			}
			message += "\n";
		}
		return message;
	}

	static int fieldPrime = 23;

	public override int GetHashCode ()
	{
		int hash = 17;

		for(int x = 0; x < size; x++) {
			for(int y = 0; y < size; y++) {
				hash = hash*fieldPrime + board[x,y].GetHashCode();
			}
		}
		hash = hash*fieldPrime + playerColor.GetHashCode();
		return hash;
	}

}
