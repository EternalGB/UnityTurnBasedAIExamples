using System.Collections.Generic;

public class ChessBoard : GameState
{

	public BoardPosition[,] board;
	public int size = 8;
	public PieceColor playerColor;


	public ChessBoard(PieceColor playerColor)
	{
		board = new BoardPosition[size,size];
		this.playerColor = playerColor;
		InitPieces();
	}

	public ChessBoard(ChessBoard oldBoard)
	{
		board = new BoardPosition[size,size];
		playerColor = oldBoard.playerColor;
		size = oldBoard.size;
		for(int x = 0; x < size; x++) {
			for(int y = 0; y < size; y++) {
				board[x,y] = oldBoard.board[x,y].Clone();
			}
		}
	}

	void InitPieces()
	{
		for(int x = 0; x < size; x++) {
			for(int y = 0; y < size; y++) {
				board[x,y] = new BoardPosition(null);
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
			board[0,y] = new BoardPosition(new ChessPiece(PieceType.Rook,color));
			board[1,y] = new BoardPosition(new ChessPiece(PieceType.Knight,color));
			board[2,y] = new BoardPosition(new ChessPiece(PieceType.Bishop,color));
			board[3,y] = new BoardPosition(new ChessPiece(PieceType.Queen,color));
			board[4,y] = new BoardPosition(new ChessPiece(PieceType.King,color));
			board[5,y] = new BoardPosition(new ChessPiece(PieceType.Bishop,color));
			board[6,y] = new BoardPosition(new ChessPiece(PieceType.Knight,color));
			board[7,y] = new BoardPosition(new ChessPiece(PieceType.Rook,color));
		}
		for(int x = 0; x < size; x++) {
			board[x,1] = new BoardPosition(new ChessPiece(PieceType.Pawn,PieceColor.White));
			board[x,size-2] = new BoardPosition(new ChessPiece(PieceType.Pawn,PieceColor.Black));
		}

	}

	public override IEnumerable<Turn> GeneratePossibleTurns()
	{
		for(int x = 0; x < size; x++) {
			for(int y = 0; y < size; y++) {
				if(board[x,y].IsOccupied() && board[x,y].piece.color == playerColor) {
					List<ChessTurn> chessTurns = board[x,y].piece.GetPossibleMoves(this,x,y);
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

	bool ContainsPiece(ChessPiece piece)
	{
		for(int x = 0; x < size; x++) {
			for(int y = 0; y < size; y++) {
				if(board[x,y].IsOccupied() && piece.Equals(board[x,y].piece))
					return true;
			}
		}
		return false;
	}

	int[] FindPieceLocationOnBoard(ChessPiece piece)
	{
		for(int x = 0; x < size; x++) {
			for(int y = 0; y < size; y++) {
				ChessPiece next = board[x,y].piece;
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
				if(board[x,y].IsOccupied()) {
					ChessPiece next = board[x,y].piece;
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

}
