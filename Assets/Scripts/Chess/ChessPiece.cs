using System.Collections.Generic;

public class ChessPiece
{


	public PieceType type;
	public PieceColor color;
	public bool captured;
	public bool firstMoveDone;

	public ChessPiece (PieceType type, PieceColor color, bool captured)
	{
		this.type = type;
		this.color = color;
		this.captured = captured;
		firstMoveDone = false;
	}
	

	public override bool Equals (object obj)
	{
		if(obj.GetType().Equals(typeof(ChessPiece))) {
			ChessPiece other = (ChessPiece)obj;
			return other.color == color && other.type == type && other.captured == captured;
		} else
			return false;

	}

	public List<ChessTurn> GetPossibleMoves(ChessBoard board, int posX, int posY)
	{
		switch (type) {
		case PieceType.Pawn:
			return GetPawnMoves(board,posX,posY);
		case PieceType.Rook:
			return GetRookMoves(board,posX,posY);
		case PieceType.Bishop:
			return GetBishopMoves(board,posX,posY);
		case PieceType.Knight:
			return GetKnightMoves(board,posX,posY);
		case PieceType.Queen:
			return GetQueenMoves(board,posX,posY);
		case PieceType.King:
			return GetKingMoves(board,posX,posY);
		default:
			return new List<ChessTurn>();
		}
	}

	static int[][] rookDirs = new int[][]{new int[]{1,0}, new int[]{0,1}, new int[]{0,-1}, new int[]{-1,0}};
	static int[][] bishopDirs = new int[][]{new int[]{1,1}, new int[]{-1,1}, new int[]{1,-1}, new int[]{-1,-1}};
	static int[][] knightMoves = new int[][]{	new int[]{1,2}, new int[]{-1,2}, new int[]{2,1}, new int[]{2,-1}, 
												new int[]{1,-2}, new int[]{-1,-2}, new int[]{-2,1}, new int[]{-2,-1}};

	public List<ChessTurn> GetPawnMoves(ChessBoard board, int posX, int posY)
	{
		List<ChessTurn> moves = new List<ChessTurn>();
		int dir = 1;
		List<int> amounts = new List<int>();
		amounts.Add(1);
		if(color != PieceColor.White)
			dir = -1;
		if(!firstMoveDone)
			amounts.Add(2);

		foreach(int amount in amounts) {
			if(posY+amount*dir < board.size) {
				if(!board.board[posX,posY+amount*dir].HasPiece() || OpposingPieceAt(board,posX,posY+amount*dir)) {
					moves.Add(new ChessTurn(posX,posY,posX,posY+amount*dir));
				}
			}
		}
		return moves;
	}

	public List<ChessTurn> GetRookMoves(ChessBoard board, int posX, int posY)
	{
		return GetLinearMoves(board,posX,posY,rookDirs,board.size);
	}

	public List<ChessTurn> GetBishopMoves(ChessBoard board, int posX, int posY)
	{
		return GetLinearMoves(board,posX,posY,bishopDirs,board.size);
	}

	List<ChessTurn> GetLinearMoves(ChessBoard board, int posX, int posY, int[][] dirs, int maxDist)
	{
		List<ChessTurn> moves = new List<ChessTurn>();
		foreach(int[] dir in dirs) {
			int newX = posX+dir[0];
			int newY = posY+dir[1];
			int moveCount = 0;
			while(moveCount < maxDist &&
			      newX >= 0 && newX < board.size &&
			      newY >= 0 && newY < board.size &&
			      !board.board[newX,newY].HasPiece()) {
				moves.Add(new ChessTurn(posX,posY,newX,newY));
				newX += dir[0];
				newY += dir[1];
				moveCount++;
			}
			if(OpposingPieceAt(board,newX,newY)) {
				moves.Add(new ChessTurn(posX,posY,newX,newY));
			}
		}
		return moves;
	}

	public List<ChessTurn> GetKnightMoves(ChessBoard board, int posX, int posY)
	{
		List<ChessTurn> moves = new List<ChessTurn>();
		foreach(int[] move in knightMoves) {
			int newX = posX+move[0];
			int newY = posY+move[1];
			if(newX >= 0 && newX < board.size &&
			   newY >= 0 && newY < board.size &&
			   !AlliedPieceAt(board,posX,posY)) {
				moves.Add(new ChessTurn(posX,posY,newX,newY));
			}
		}
		return moves;
	}

	public List<ChessTurn> GetQueenMoves(ChessBoard board, int posX, int posY)
	{
		List<ChessTurn> turns = new List<ChessTurn>();
		turns.AddRange(GetRookMoves(board,posX,posY));
		turns.AddRange(GetBishopMoves(board,posX,posY));
		return turns;
	}

	public List<ChessTurn> GetKingMoves(ChessBoard board, int posX, int posY)
	{
		List<ChessTurn> turns = new List<ChessTurn>();
		turns.AddRange(GetLinearMoves(board,posX,posY,rookDirs,1));
		turns.AddRange(GetLinearMoves(board,posX,posY,bishopDirs,1));
		return turns;
	}

	bool OpposingPieceAt(ChessBoard board, int posX, int posY)
	{
		return board.board[posX,posY].HasPiece() && board.board[posX,posY].piece.color != color;
	}

	bool AlliedPieceAt(ChessBoard board, int posX, int posY)
	{
		return board.board[posX,posY].HasPiece() && board.board[posX,posY].piece.color == color;
	}
	
	
}
