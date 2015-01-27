using System.Collections.Generic;

public class ChessPiece
{


	public PieceType type;
	public PieceColor color;
	public bool firstMoveDone;

	public ChessPiece (PieceType type, PieceColor color)
	{
		this.type = type;
		this.color = color;
		firstMoveDone = false;
	}
	

	public override bool Equals (object obj)
	{
		if(obj.GetType().Equals(typeof(ChessPiece))) {
			ChessPiece other = (ChessPiece)obj;
			return other.color == color && other.type == type;
		} else
			return false;

	}

	public ChessPiece Clone()
	{
		ChessPiece piece =  new ChessPiece(type,color);
		piece.firstMoveDone = firstMoveDone;
		return piece;
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
	static int[][] pawnCaptures = new int[][]{ new int[]{1,1}, new int[]{-1,1}};
	
	List<ChessTurn> GetPawnMoves(ChessBoard board, int posX, int posY)
	{
		List<ChessTurn> moves = new List<ChessTurn>();
		int dir = 1;
		if(color != PieceColor.White)
			dir = -1;
		if(posY+dir >= 0 && posY+dir < board.size) {
			if(!board.board[posX,posY+dir].IsOccupied())
				moves.Add(new ChessTurn(posX,posY,posX,posY+dir));
			if(!firstMoveDone && !board.board[posX,posY+dir].IsOccupied() && !board.board[posX,posY+dir+dir].IsOccupied())
				moves.Add(new ChessTurn(posX,posY,posX,posY+dir+dir));
			foreach(int[] cap in pawnCaptures) {
				if(OpposingPieceAt(board, posX + cap[0], posY + dir*cap[1])) {
					moves.Add(new ChessTurn(posX,posY,posX + cap[0], posY + dir*cap[1]));
				}
			}
		}
		return moves;
	}

	List<ChessTurn> GetRookMoves(ChessBoard board, int posX, int posY)
	{
		return GetLinearMoves(board,posX,posY,rookDirs,board.size);
	}

	List<ChessTurn> GetBishopMoves(ChessBoard board, int posX, int posY)
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
			      !board.board[newX,newY].IsOccupied()) {
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

	List<ChessTurn> GetKnightMoves(ChessBoard board, int posX, int posY)
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

	List<ChessTurn> GetQueenMoves(ChessBoard board, int posX, int posY)
	{
		List<ChessTurn> turns = new List<ChessTurn>();
		turns.AddRange(GetRookMoves(board,posX,posY));
		turns.AddRange(GetBishopMoves(board,posX,posY));
		return turns;
	}

	List<ChessTurn> GetKingMoves(ChessBoard board, int posX, int posY)
	{
		List<ChessTurn> turns = new List<ChessTurn>();
		turns.AddRange(GetLinearMoves(board,posX,posY,rookDirs,1));
		turns.AddRange(GetLinearMoves(board,posX,posY,bishopDirs,1));
		return turns;
	}

	bool OpposingPieceAt(ChessBoard board, int posX, int posY)
	{
		return posX >= 0 && posX < board.size && posY >= 0 && posY < board.size && 
			board.board[posX,posY].IsOccupied() && board.board[posX,posY].piece.color != color;
	}

	bool AlliedPieceAt(ChessBoard board, int posX, int posY)
	{
		return posX >= 0 && posX < board.size && posY >= 0 && posY < board.size && 
			board.board[posX,posY].IsOccupied() && board.board[posX,posY].piece.color == color;
	}
	
	
}
