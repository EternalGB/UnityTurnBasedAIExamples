using GenericTurnBasedAI;
using UnityEngine;
using System.Collections.Generic;
using System;

public class ConnectKBoard : GameState
{

	ConnectKPiece[,] board;
	public int nRows
	{
		get{return board.GetLength(1);}
	}
	public int nCols
	{
		get{return board.GetLength(0);}
	}
	int k;

	public ConnectKPiece player;

	//none of these need to be cloned
	static List<IntVector2> ascendingDiags, descendingDiags, rows, columns;
	static Dictionary<List<IntVector2>, IntVector2> startDirs;
	static List<List<IntVector2>> allStarts;
	static bool startsInitiated = false;

	//hold the number of pieces in each row, column and diagonal for p1 and p2 respectively
	public Dictionary<IntVector2, float> p1Count;
	public Dictionary<IntVector2, float> p2Count;
	//keeps track of dirty state for each row, column and diagonal
	//a start is dirty if its p1 or p2 count is above k needs to be checked for matches
	//prevents us from checking every start with count above k that will never have any matches (is full)
	public Dictionary<IntVector2, bool> dirty;

	public ConnectKBoard(int numRows, int numColumns, int numMatches)
	{
		Debug.Log (string.Format("New Board: {0} X {1}    {2} matches needed",numRows,numColumns,numMatches));
		board = new ConnectKPiece[numRows,numColumns];
		Debug.Log (string.Format("nRows = {0}, nCols = {1}",nRows,nCols));
		k = numMatches;
		player = ConnectKPiece.P1;

		for(int x = 0; x < numRows; x++) {
			for(int y = 0; y < nCols; y++) {
				board[x,y] = ConnectKPiece.None;
			}
		}
		p1Count = new Dictionary<IntVector2, float>();
		p2Count = new Dictionary<IntVector2, float>();
		dirty = new Dictionary<IntVector2, bool>();
		if(!startsInitiated)
			InitStarts();
		foreach(List<IntVector2> starts in allStarts) {
			foreach(IntVector2 start in starts) {
				p1Count.Add(start,0);
				p2Count.Add(start,0);
				dirty.Add(start,false);
			}
		}

	}

	void InitStarts()
	{
		allStarts = new List<List<IntVector2>>();
		ascendingDiags = GetAscendingDiagonals();
		descendingDiags = GetDescendingDiagonals();
		rows = GetRows();
		columns = GetCols();
		allStarts.Add(ascendingDiags);
		allStarts.Add(descendingDiags);
		allStarts.Add(rows);
		allStarts.Add(columns);
		startDirs = new Dictionary<List<IntVector2>, IntVector2>();
		startDirs.Add(ascendingDiags,new IntVector2(1,1));
		startDirs.Add(descendingDiags, new IntVector2(-1,-1));
		startDirs.Add(rows, new IntVector2(1,0));
		startDirs.Add(columns, new IntVector2(0,1));
	}

	public ConnectKBoard(ConnectKBoard oldBoard)
	{
		p1Count = new Dictionary<IntVector2, float>();
		p2Count = new Dictionary<IntVector2, float>();
		dirty = new Dictionary<IntVector2, bool>();
		foreach(IntVector2 start in oldBoard.dirty.Keys) {
			p1Count.Add(start,oldBoard.p1Count[start]);
			p2Count.Add(start,oldBoard.p2Count[start]);
			dirty.Add(start,oldBoard.dirty[start]);
		}
	}

	public bool HasDescendingDiagonal(int x, int y)
	{
		int p = nRows;
		int q = nCols;
		return y >= (k-1)-x && y <= (q+p-k-1)-x;
	}

	public bool HasAscendingDiagonal(int x, int y)
	{
		int p = nRows;
		int q = nCols;
		//Debug.Log(string.Format("({0},{1}) - {5}: {1} >= {0} - ({3}-{4}) && {1} < {0} + ({2}-{4}))",x,y,p,q,k,result));
		return y >= x - (q-k) && y <= x + (p-k);
	}

	List<IntVector2> GetAscendingDiagonals()
	{
		List<IntVector2> diags = new List<IntVector2>();
		for(int x = 0; x <= nCols - k; x++)
			diags.Add(new IntVector2(x,0));
		for(int y = 1; y <= nRows - k; y++)
			diags.Add(new IntVector2(0,y));
		return diags;
	}

	List<IntVector2> GetDescendingDiagonals()
	{
		List<IntVector2> diags = new List<IntVector2>();
		for(int x = 0; x <= nCols - k; x++)
			diags.Add(new IntVector2(x,nRows-1));
		for(int y = k-1; y <= nRows - 2; y++)
			diags.Add(new IntVector2(0,y));
		return diags;
	}

	List<IntVector2> GetRows()
	{
		List<IntVector2> rows = new List<IntVector2>();
		for(int y = 0; y < nRows; y++)
			rows.Add(new IntVector2(0,y));
		return rows;
	}

	List<IntVector2> GetCols()
	{
		List<IntVector2> cols = new List<IntVector2>();
		for(int x = 0; x < nCols; x++)
			cols.Add(new IntVector2(x,0));
		return cols;
	}

	public IntVector2 GetAscendingDiagonalStart(int x, int y)
	{
		int dist = Math.Min(x,y);
		return new IntVector2(x-dist,y-dist);
	}

	public IntVector2 GetDescendingDiagonalStart(int x, int y)
	{
		int dist = Math.Min(x,nRows-y-1);
		return new IntVector2(x-dist,y+dist);
	}

	public IntVector2 GetRowStart(int x, int y)
	{
		return new IntVector2(0,y);
	}

	public IntVector2 GetColStart(int x, int y)
	{
		return new IntVector2(x,0);
	}

	public void AddPiece(ConnectKPiece piece, int column)
	{
		int actualY = nRows-1;
		for(int y = 0; y < nRows; y++) {
			if(board[column,y] != ConnectKPiece.None)
				actualY = y;
		}
		board[column,actualY] = piece;
		Dictionary<IntVector2,float> dict;
		if(piece == ConnectKPiece.P1)
			dict = p1Count;
		else
			dict = p2Count;
		List<IntVector2> starts = new List<IntVector2>();
		if(HasAscendingDiagonal(column,actualY)) {
			dict[GetAscendingDiagonalStart(column,actualY)] += 1;
			starts.Add(GetAscendingDiagonalStart(column,actualY));
		}
		if(HasDescendingDiagonal(column,actualY)) {
			dict[GetDescendingDiagonalStart(column,actualY)] += 1;
			starts.Add(GetDescendingDiagonalStart(column,actualY));
		}
		dict[GetRowStart(column,actualY)] += 1;
		dict[GetColStart(column,actualY)] += 1;
		starts.Add(GetRowStart(column,actualY));
		starts.Add(GetColStart(column,actualY));
		//this ensures that we only have to check rows/columns/diagonals that have actually had changes
		foreach(IntVector2 start in starts) {
			if(dict[start] >= k)
				dirty[start] = true;
		}
	}

	bool CheckMatches()
	{
		bool result = false;
		foreach(List<IntVector2> starts in allStarts) {
			IntVector2 dir = startDirs[starts];
			foreach(IntVector2 start in starts) {
				if((p1Count[start] >= k || p2Count[start] >= k) && dirty[start]) {
					result = result || CheckMatch(start.x,start.y,dir.x,dir.y);
					dirty[start] = false;
				}
			}
		}
		return result;
	}

	bool CheckMatch(int xStart, int yStart, int xDir, int yDir)
	{

		int count = 0;
		ConnectKPiece piece = board[xStart,yStart];
		int x = xStart;
		int y = yStart;
		while(x >= 0 && x < nCols && y >= 0 && y < nRows) {
			if(board[x,y] == piece)
				count++;
			else {
				piece = board[x,y];
				count = 1;
			}
			if(count == k)
				return true;
			x += xDir;
			y += yDir;
		}
		return count == k;
	}

	public override bool IsTerminal ()
	{
		return CheckMatches();
	}

	public override System.Collections.Generic.IEnumerable<Turn> GeneratePossibleTurns ()
	{
		for(int x = 0; x < nCols; x++) {
			if(board[x,nRows-1] == ConnectKPiece.None)
				yield return new ConnectKTurn(x,player);
		}
	}

	public override GameState Clone ()
	{
		return new ConnectKBoard(this);
	}

}

public class IntVector2
{

	public int x, y;

	public IntVector2(int x, int y)
	{
		this.x = x;
		this.y = y;
	}

	public override int GetHashCode ()
	{
		int hash = 17;
		hash = hash*23 + x;
		hash = hash*23 + y;
		return hash;
	}

	public override bool Equals (object obj)
	{
		IntVector2 other = obj as IntVector2;
		if(other == null)
			return false;
		return x == other.x && y == other.y;
	}
}
