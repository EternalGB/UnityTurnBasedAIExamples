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
	static List<Line> allLines;
	static bool linesInitiated = false;
	static Dictionary<IntVector2, List<Line>> lineDict; 

	//hold the number of pieces in each row, column and diagonal for p1 and p2 respectively
	public Dictionary<Line, float> p1Count;
	public Dictionary<Line, float> p2Count;
	//keeps track of dirty state for each row, column and diagonal
	//a start is dirty if its p1 or p2 count is above k needs to be checked for matches
	//prevents us from checking every start with count above k that will never have any matches (is full)
	public Dictionary<Line, bool> dirty;

	public ConnectKBoard(int numColumns, int numRows, int numMatches)
	{
		Debug.Log (string.Format("New Board: {0} X {1}    {2} matches needed",numColumns,numRows,numMatches));
		board = new ConnectKPiece[numColumns,numRows];
		Debug.Log (string.Format("nRows = {0}, nCols = {1}",nRows,nCols));
		k = numMatches;
		player = ConnectKPiece.P1;

		for(int x = 0; x < nCols; x++) {
			for(int y = 0; y < nRows; y++) {
				board[x,y] = ConnectKPiece.None;
			}
		}
		p1Count = new Dictionary<Line, float>();
		p2Count = new Dictionary<Line, float>();

		dirty = new Dictionary<Line, bool>();
		if(!linesInitiated)
			InitLines();

		foreach(Line line in allLines) {
			p1Count.Add(line,0);
			p2Count.Add(line,0);
			dirty.Add(line,false);
		}

	}

	//clone constructor
	public ConnectKBoard(ConnectKBoard oldBoard)
	{
		p1Count = new Dictionary<Line, float>();
		p2Count = new Dictionary<Line, float>();
		dirty = new Dictionary<Line, bool>();
		foreach(Line line in oldBoard.dirty.Keys) {
			p1Count.Add(line,oldBoard.p1Count[line]);
			p2Count.Add(line,oldBoard.p2Count[line]);
			dirty.Add(line,oldBoard.dirty[line]);
		}
	}

	//precompute a bunch of stuff
	void InitLines()
	{
		allLines = new List<Line>();
		allLines.AddRange(GetAscendingDiagonals());
		allLines.AddRange(GetDescendingDiagonals());
		allLines.AddRange(GetRows());
		allLines.AddRange(GetCols());
		lineDict = new Dictionary<IntVector2, List<Line>>();
		for(int x = 0; x < nCols; x++) {
			for(int y = 0; y < nRows; y++) {
				IntVector2 pos = new IntVector2(x,y);
				lineDict.Add(pos, new List<Line>());
				//ascending diagonals
				int dist = Math.Min(x,y);
				if(HasAscendingDiagonal(x,y))
					lineDict[pos].Add(new Line(new IntVector2(x-dist,y-dist), new IntVector2(1,1)));
				//descending diagonals
				dist = Math.Min(x,nRows-y-1);
				if(HasDescendingDiagonal(x,y))
					lineDict[pos].Add(new Line(new IntVector2(x-dist,y+dist), new IntVector2(1,-1)));
				//rows
				lineDict[pos].Add(new Line(new IntVector2(0,y), new IntVector2(1,0)));
				//cols
				lineDict[pos].Add(new Line(new IntVector2(x,0), new IntVector2(0,1)));
			}
		}
		linesInitiated = true;
	}



	List<Line> GetAscendingDiagonals()
	{
		List<Line> diags = new List<Line>();
		for(int x = 0; x <= nCols - k; x++)
			diags.Add(new Line(new IntVector2(x,0), new IntVector2(1,1)));
		for(int y = 1; y <= nRows - k; y++)
			diags.Add(new Line(new IntVector2(0,y), new IntVector2(1,1)));
		return diags;
	}
	
	List<Line> GetDescendingDiagonals()
	{
		List<Line> diags = new List<Line>();
		for(int x = 0; x <= nCols - k; x++)
			diags.Add(new Line(new IntVector2(x,nRows-1), new IntVector2(1,-1)));
		for(int y = k-1; y <= nRows - 2; y++)
			diags.Add(new Line(new IntVector2(0,y), new IntVector2(1,-1)));
		return diags;
	}
	
	List<Line> GetRows()
	{
		List<Line> rows = new List<Line>();
		for(int y = 0; y < nRows; y++)
			rows.Add(new Line(new IntVector2(0,y), new IntVector2(1,0)));
		return rows;
	}
	
	List<Line> GetCols()
	{
		List<Line> cols = new List<Line>();
		for(int x = 0; x < nCols; x++)
			cols.Add(new Line(new IntVector2(x,0), new IntVector2(0,1)));
		return cols;
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

	public bool ColumnFull(int col)
	{
		return board[col,nRows-1] != ConnectKPiece.None;
	}

	public static List<Line> GetLines(IntVector2 pos)
	{
		return lineDict[pos];
	}

	public void AddPiece(ConnectKPiece piece, int column)
	{

		int actualY = nRows-1;
		for(int y = 0; y < nRows; y++) {
			if(board[column,y] != ConnectKPiece.None)
				actualY = y;
		}
		IntVector2 pos = new IntVector2(column,actualY);
		board[pos.x,pos.y] = piece;
		Dictionary<Line,float> dict;
		if(piece == ConnectKPiece.P1)
			dict = p1Count;
		else
			dict = p2Count;

		List<Line> lines = GetLines(pos);
		//this ensures that we only have to check rows/columns/diagonals that have actually had changes
		foreach(Line line in lines) {
			dict[line] += 1;
			if(dict[line] >= k)
				dirty[line] = true;
		}
	}

	bool CheckMatches()
	{
		bool result = false;
		foreach(Line line in allLines) {
			if((p1Count[line] >= k || p2Count[line] >= k) && dirty[line]) {
				result = result || CheckMatch(line.start.x,line.start.y,line.dir.x,line.dir.y);
				dirty[line] = false;
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

	public ConnectKPiece GetPiece(int x, int y)
	{
		return board[x,y];
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

	public class Line
	{
		public IntVector2 start;
		public IntVector2 dir;

		public Line (IntVector2 start, IntVector2 dir)
		{
			this.start = start;
			this.dir = dir;
		}

		public override bool Equals (object obj)
		{
			Line other = obj as Line;
			if(other == null)
				return false;

			return start.Equals(other.start) && dir.Equals(other.dir);
		}

		public override int GetHashCode ()
		{
			int hash = 17;
			hash = hash*23 + start.GetHashCode();
			hash = hash*23 + dir.GetHashCode();
			return hash;
		}
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
