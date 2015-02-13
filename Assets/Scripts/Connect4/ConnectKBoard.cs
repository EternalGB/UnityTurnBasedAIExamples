using UniversalTurnBasedAI;
using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Our internal representation of the Connect K game state. Uses a 2 dimensional array
/// of <see cref="ConnectKPiece"/> to represent the board. Also keeps track of all of the
/// possible "lines" which are the rows, columns and diagonals on the board that are long
/// enough to fit "k" pieces, as well as a dictionary that maps board positions to the lines
/// that they lie on. 
/// 
/// For each line we also create a dictionary that counts the total number 
/// of pieces for each player on that line and a dictionary that tracks whether or not that
/// line has been checked for victory since the move was made. This helps speed up the 
/// terminal checks and evaluation functions as we only have to iterate through the lines that
/// have been changed rather than the entire board.
/// 
/// <seealso cref="ConnectKPiece"/>
/// <seealso cref="ConnectKTurn"/>
/// <see cref="ConnectKEvaluator"/>
/// <seealso cref="Line"/>
/// </summary>
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
	/// <summary>
	/// The number of matches required
	/// </summary>
	public int k;

	public ConnectKPiece player;

	//none of these need to be cloned
	protected List<Line> allLines;
	protected Dictionary<IntVector2, List<Line>> lineDict; 

	//hold the number of pieces in each row, column and diagonal for p1 and p2 respectively
	public Dictionary<Line, float> p1Count;
	public Dictionary<Line, float> p2Count;
	//keeps track of dirty state for each row, column and diagonal
	//a start is dirty if its p1 or p2 count is above k needs to be checked for matches
	//prevents us from checking every start with count above k that will never have any matches (is full)
	public Dictionary<Line, bool> dirty;
	

	/// <summary>
	/// Initializes a new instance of the <see cref="ConnectKBoard"/> class.
	/// </summary>
	/// <param name="numColumns">Number columns.</param>
	/// <param name="numRows">Number rows.</param>
	/// <param name="numMatches">Number matches required to win</param>
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
		InitLines();

		foreach(Line line in allLines) {
			p1Count.Add(line,0);
			p2Count.Add(line,0);
			dirty.Add(line,false);
		}

	}

	/// <summary>
	/// The clone constructor. Initiates a new <see cref="ConnectKBoard"/> using relevant information
	/// from <paramref name="oldBoard"/>. Copies a reference to the line list and deep copies the
	/// piece counts and dirty dictionaries.
	/// </summary>
	/// <param name="oldBoard">Old board.</param>
	public ConnectKBoard(ConnectKBoard oldBoard)
	{
		board = new ConnectKPiece[oldBoard.nCols, oldBoard.nRows];
		for(int x = 0; x < nCols; x++) {
			for(int y = 0; y < nRows; y++) {
				board[x,y] = oldBoard.board[x,y];
			}
		}

		player = oldBoard.player;
		k = oldBoard.k;

		allLines = oldBoard.allLines;
		lineDict = oldBoard.lineDict;

		p1Count = new Dictionary<Line, float>();
		p2Count = new Dictionary<Line, float>();
		dirty = new Dictionary<Line, bool>();
		foreach(Line line in oldBoard.dirty.Keys) {
			p1Count.Add(line,oldBoard.p1Count[line]);
			p2Count.Add(line,oldBoard.p2Count[line]);
			dirty.Add(line,oldBoard.dirty[line]);
		}
	}

	/// <summary>
	/// Initialises <param name="allLines"> and <param name="lineDict">
	/// </summary>
	void InitLines()
	{
		allLines = GetLines(nCols,nRows,k);
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
	}

	/// <summary>
	/// Gets the lines.
	/// </summary>
	/// <returns>The lines.</returns>
	/// <param name="nCols">N cols.</param>
	/// <param name="nRows">N rows.</param>
	/// <param name="k">K.</param>
	public static List<Line> GetLines(int nCols, int nRows, int k)
	{
		List<Line> allLines = new List<Line>();
		allLines.AddRange(GetAscendingDiagonals(nCols, nRows, k));
		allLines.AddRange(GetDescendingDiagonals(nCols, nRows, k));
		allLines.AddRange(GetRows(nCols, nRows));
		allLines.AddRange(GetCols(nCols, nRows));
		return allLines;
	}



	static List<Line> GetAscendingDiagonals(int nCols, int nRows, int k)
	{
		List<Line> diags = new List<Line>();
		for(int x = 0; x <= nCols - k; x++)
			diags.Add(new Line(new IntVector2(x,0), new IntVector2(1,1)));
		for(int y = 1; y <= nRows - k; y++)
			diags.Add(new Line(new IntVector2(0,y), new IntVector2(1,1)));
		return diags;
	}
	
	static List<Line> GetDescendingDiagonals(int nCols, int nRows, int k)
	{
		List<Line> diags = new List<Line>();
		for(int x = 0; x <= nCols - k; x++)
			diags.Add(new Line(new IntVector2(x,nRows-1), new IntVector2(1,-1)));
		for(int y = k-1; y <= nRows - 2; y++)
			diags.Add(new Line(new IntVector2(0,y), new IntVector2(1,-1)));
		return diags;
	}
	
	static List<Line> GetRows(int nCols, int nRows)
	{
		List<Line> rows = new List<Line>();
		for(int y = 0; y < nRows; y++)
			rows.Add(new Line(new IntVector2(0,y), new IntVector2(1,0)));
		return rows;
	}
	
	static List<Line> GetCols(int nCols, int nRows)
	{
		List<Line> cols = new List<Line>();
		for(int x = 0; x < nCols; x++)
			cols.Add(new Line(new IntVector2(x,0), new IntVector2(0,1)));
		return cols;
	}

	/// <summary>
	/// Determines if position (x,y) has descending diagonal line which could make a match on this board
	/// </summary>
	/// <returns><c>true</c> if this instance has descending diagonal; otherwise, <c>false</c>.</returns>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	public bool HasDescendingDiagonal(int x, int y)
	{
		int p = nRows;
		int q = nCols;
		return y >= (k-1)-x && y <= (q+p-k-1)-x;
	}

	/// <summary>
	/// Determines if position (x,y) has ascending diagonal line which could make a match on this board
	/// </summary>
	/// <returns><c>true</c> if this instance has ascending diagonal; otherwise, <c>false</c>.</returns>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	public bool HasAscendingDiagonal(int x, int y)
	{
		int p = nRows;
		int q = nCols;
		return y >= x - (q-k) && y <= x + (p-k);
	}

	public bool ColumnFull(int col)
	{
		return board[col,nRows-1] != ConnectKPiece.None;
	}

	List<Line> GetLines(IntVector2 pos)
	{
		return lineDict[pos];
	}

	/// <summary>
	/// Adds a piece to the given column. Does not check if the column is full
	/// </summary>
	/// <param name="piece">Piece.</param>
	/// <param name="column">Column.</param>
	public void AddPiece(ConnectKPiece piece, int column)
	{
		//find the correct row by "simulating" gravity
		int actualY = nRows-1;
		for(int y = 0; y < nRows; y++) {
			if(board[column,y] == ConnectKPiece.None) {
				actualY = y;
				break;
			}
		}
		IntVector2 pos = new IntVector2(column,actualY);
		board[pos.x,pos.y] = piece;
		//determine which piece count to update
		Dictionary<Line,float> dict;
		if(piece == ConnectKPiece.P1)
			dict = p1Count;
		else
			dict = p2Count;

		List<Line> lines = GetLines(pos);
		//Update the piece counts and dirty dictionary
		foreach(Line line in lines) {
			dict[line] += 1;
			if(dict[line] >= k)
				dirty[line] = true;
		}
	}

	/// <summary>
	/// Search the line list for altered lines and determine if any have 'k' in a row
	/// </summary>
	/// <returns><c>true</c>, if there are 'k' in a row pieces, <c>false</c> otherwise.</returns>
	bool CheckMatches()
	{
		bool result = false;
		foreach(Line line in allLines) {
			if((p1Count[line] >= k || p2Count[line] >= k) && dirty[line]) {
				result = result || CheckLine(line);
				dirty[line] = false;
			}
		}
		return result;
	}

	/// <summary>
	/// Check a single line for 'k' in a row
	/// </summary>
	/// <returns><c>true</c>, if the line has 'k' in a row, <c>false</c> otherwise.</returns>
	/// <param name="line>Line to check</para>
	bool CheckLine(Line line)
	{
		int count = 0;
		ConnectKPiece piece = board[line.start.x,line.start.y];
		if(piece != ConnectKPiece.None)
			count++;
		int x = line.start.x + line.dir.x;
		int y = line.start.y + line.dir.y;
		while(x >= 0 && x < nCols && y >= 0 && y < nRows) {
			if(board[x,y] != ConnectKPiece.None) {
				if(board[x,y] == piece)
					count++;
				else
					count = 1;
			} else {
				count = 0;
			}
			if(count >= k)
				return true;

			piece = board[x,y];
			x += line.dir.x;
			y += line.dir.y;
		}
		return count >= k;
	}

	public bool IsEmpty(int x, int y)
	{
		return IsValidSpace(x,y) && GetPiece(x,y) == ConnectKPiece.None;
	}

	public bool IsValidSpace(int x, int y)
	{
		return x >= 0 && x < nCols && y >= 0 && y < nRows;
	}

	public bool IsBoardFull()
	{
		bool result = true;
		for(int x = 0; x < nCols; x++) {
			result = result && !IsEmpty(x,nRows-1);
		}
		return result;
	}

	public ConnectKPiece GetPiece(int x, int y)
	{
		return board[x,y];
	}

	public override bool IsTerminal ()
	{
		return CheckMatches() || IsBoardFull();
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

/// <summary>
/// Represents a sequences of positions on the board using a starting position
/// somewhere on the side of the board and a direction to step in. Also overrides
/// GetHashCode and Equals so that it can be used as a Dictionary key without
/// reference equality
/// </summary>
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

/// <summary>
/// A convenience class for holding two ints that can be
/// stored in a Dictionary without using reference equality
/// </summary>
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
