using UnityEngine;
using System.Collections.Generic;
using UniversalTurnBasedAI;

/// <summary>
/// Game controller for Connect K - a generalisation of Connect 4 where
/// board width and height and number in a row (k) can be defined. This
/// is just the Unity wrapper that handles all the GameObjects and coordinates
/// the playing of turns.
/// </summary>
public class ConnectK : MonoBehaviour 
{
	public GameObject gridSquare, winSquare;
	public float gridSize;
	public GameObject p1Piece, p2Piece;
	Vector2 bottomLeft;

	public int width, height, matches;

	ConnectKBoard board;
	List<GameObject> lastPieces;
	List<GameObject> vicSquares;
	List<GameObject> boardSquares;

	public GUISkin p1GUISkin, p2GUISkin;

	public ConnectKHumanPlayer humanP1, humanP2;
	public ConnectKAIPlayer aiP1, aiP2;
	public ConnectKRandomPlayer randomP1, randomP2;

	TurnAgent p1;
	TurnAgent p2;

	bool preGame = false;
	bool gameEnd = false;
	string[] agentOptions = {"Human", "AI", "Random"};
	int p1AgentSelection = 0, p2AgentSelection = 1;

	void Start()
	{
		preGame = true;
	}

	/// <summary>
	/// Controls the start and end UIs. Very basic default Unity UI stuff
	/// </summary>
	void OnGUI()
	{
		GUISkin defaultSkin = GUI.skin;

		GUILayout.BeginArea(new Rect(0,Screen.height/4,Screen.width,3*Screen.height/4));
		if(preGame) {

			GUILayout.BeginVertical();

			GUILayout.BeginHorizontal();

			GUI.skin = p1GUISkin;
			GUILayout.BeginVertical();
			GUILayout.Label("Player 1");
			p1AgentSelection = GUILayout.SelectionGrid(p1AgentSelection, agentOptions, 1, GUILayout.Width(Screen.width/2));
			if(p1AgentSelection == 1) {
				GUILayout.Label ("Thinking Time");
				GUILayout.Label(aiP1.timeLimit + " seconds");
				aiP1.timeLimit = GUILayout.HorizontalSlider(aiP1.timeLimit,0.1f,15f, GUILayout.Width(Screen.width/2));
				aiP1.multiThreaded = GUILayout.Toggle(aiP1.multiThreaded,"Multi-Threaded");
			}
			GUILayout.EndVertical();

			GUI.skin = p2GUISkin;
			GUILayout.BeginVertical();
			GUILayout.Label("Player 2");
			p2AgentSelection = GUILayout.SelectionGrid(p2AgentSelection, agentOptions, 1, GUILayout.Width(Screen.width/2));
			if(p2AgentSelection == 1) {
				GUILayout.Label ("Thinking Time");
				GUILayout.Label(aiP2.timeLimit + " seconds");
				aiP2.timeLimit = GUILayout.HorizontalSlider(aiP2.timeLimit,0.1f,15f, GUILayout.Width(Screen.width/2));
				aiP2.multiThreaded = GUILayout.Toggle(aiP2.multiThreaded,"Multi-Threaded");
			}
			GUILayout.EndVertical();

			GUILayout.EndHorizontal();

			GUI.skin = defaultSkin;
			GUILayout.FlexibleSpace();
			width = GUILayoutIntField("Board Width",width,1,int.MaxValue, GUILayout.Width(Screen.width/2));
			height = GUILayoutIntField("Board Height", height,1,int.MaxValue, GUILayout.Width(Screen.width/2));
			matches = GUILayoutIntField("Number in a row needed",matches,1,Mathf.Max(width,height), GUILayout.Width(Screen.width/2));

			if(GUILayout.Button("Start")) {

				StartNewGame();
			}
			GUILayout.EndVertical();

		} else if(gameEnd) {
			GUILayout.BeginVertical();
			if(GUILayout.Button("Menu")) {
				gameEnd = false;
				ClearBoard();
				DestroyBoard();
				preGame = true;
			}
			if(GUILayout.Button("Restart")) {
				gameEnd = false;
				ClearBoard();
				StartNewGame();
			}
			GUILayout.EndVertical();
		}
		GUILayout.EndArea();


	}

	void StartNewGame()
	{
		if(p1AgentSelection == 0) {
			p1 = humanP1;
		} else if(p1AgentSelection == 1) {
			p1 = aiP1;
		} else if(p1AgentSelection == 2) {
			p1 = randomP1;
		}
		
		if(p2AgentSelection == 0) {
			p2 = humanP2;
		} else if(p2AgentSelection == 1) {
			p2 = aiP2;
		} else if(p2AgentSelection == 2) {
			p2 = randomP2;
		}


		preGame = false;
		//create our internal board representation
		board = new ConnectKBoard(width,height,matches);
		InitBoard();
		lastPieces = new List<GameObject>();
		
		//initialise our turn agents
		p1.Init(board);
		p2.Init(board);
		
		//register to receive and process turns
		p1.TurnReadyEvent += ReceiveTurn;
		p2.TurnReadyEvent += ReceiveTurn;
		//start the turn loop
		PlayTurn();
	}

	void PlayTurn()
	{
		//Generate the next player's turn
		if(board.player == ConnectKPiece.P1) {
			p1.GenerateNextTurn(board);
		} else {
			p2.GenerateNextTurn(board);
		}
	}

	void ReceiveTurn(ITurn turn)
	{
		//Apply the turn and redraw the board
		board = turn.ApplyTurn(board) as ConnectKBoard;
		DrawBoard();
		//check to see if the game is over
		if(board.IsTerminal()) {
			ConnectKPiece winner;
			if(board.player == ConnectKPiece.P1)
				winner = ConnectKPiece.P2;
			else
				winner = ConnectKPiece.P1;
			Line winLine = board.GetWinningLine();
			if(winLine != null)
				DrawVictory(winLine, winner);
			Debug.Log ("Game over");
			gameEnd = true;

			p1.TurnReadyEvent -= ReceiveTurn;
			p2.TurnReadyEvent -= ReceiveTurn;

		} else
			PlayTurn();
	}

	void InitBoard()
	{
		if(boardSquares == null)
			boardSquares = new List<GameObject>();

		bottomLeft = -(new Vector2(width/2f - gridSize/2f,height/2f - gridSize/2f));
		for(int i = 0; i < width; i++) {
			for(int j = 0; j < height; j++) {
				Vector2 pos = GetRealPosition(i,j);
				GameObject square = (GameObject)Instantiate(gridSquare,pos,Quaternion.identity);
				boardSquares.Add(square);
			}
		}
		//resize the camera to see the whole board
		float cameraWidth = (board.nCols+2)*gridSize;
		float cameraHeight = (board.nRows+2)*gridSize;
		if(cameraWidth > cameraHeight*Camera.main.aspect) {
			cameraHeight = cameraWidth/Camera.main.aspect;
		}
		Camera.main.orthographicSize = cameraHeight/2;
	}

	void DrawBoard()
	{

		foreach(GameObject piece in lastPieces)
			Destroy(piece);
		lastPieces.Clear();
		for(int x = 0; x < board.nCols; x++) {
			for(int y = 0; y < board.nRows; y++) {
				if(board.GetPiece(x,y) == ConnectKPiece.P1) {
					lastPieces.Add((GameObject)Instantiate(p1Piece, GetRealPosition(x,y), Quaternion.identity));
				} else if(board.GetPiece(x,y) == ConnectKPiece.P2){
					lastPieces.Add((GameObject)Instantiate(p2Piece, GetRealPosition(x,y), Quaternion.identity));
				}
			}
		}
	}

	void ClearBoard()
	{
		foreach(GameObject piece in lastPieces)
			Destroy(piece);
		lastPieces.Clear();

		foreach(GameObject square in vicSquares)
			Destroy(square);
		vicSquares.Clear();
	}

	void DestroyBoard()
	{
		if(boardSquares != null) {
			foreach(GameObject square in boardSquares)
				Destroy(square);
			boardSquares.Clear();
		}

	}

	void DrawVictory(Line line, ConnectKPiece winner)
	{
		vicSquares = new List<GameObject>();
		ConnectKPiece lastPiece = board.GetPiece(line.start);
		int x = line.start.x + line.dir.x;
		int y = line.start.y + line.dir.y;
		int count = 0;
		IntVector2 winStart = line.start;
		if(lastPiece == winner) {
			count = 1;
		}
		while(x >= 0 && x < board.nCols && y >= 0 && y < board.nRows) {
			if(board.GetPiece(x,y) == winner) {
				if(lastPiece != winner)
					winStart = new IntVector2(x,y);
				count++;
			} else {
				if(count >= board.k)
					break;
				count = 0;
			}
			lastPiece = board.GetPiece(x,y);
			x += line.dir.x;
			y += line.dir.y;
		}
		if(count >= board.k) {
			x = winStart.x;
			y = winStart.y;
			for(int i = 0; i < count; i++) {
				vicSquares.Add(Instantiate(winSquare, GetRealPosition(x,y), Quaternion.identity) as GameObject);
				x += line.dir.x;
				y += line.dir.y;
			}
		}
	}

	Vector2 GetRealPosition(int x, int y)
	{
		return bottomLeft + new Vector2(x*gridSize,y*gridSize);
	}

	int GUILayoutIntField(string label, int value, int min, int max, params GUILayoutOption[] options)
	{
		string strVal = value.ToString();
		GUILayout.BeginHorizontal();
		GUILayout.Label (label, options);
		strVal = GUILayout.TextField(strVal, options);
		GUILayout.EndHorizontal();
		int returnVal;
		if(int.TryParse(strVal, out returnVal)) {
			if(returnVal >= min && returnVal <= max)
				return returnVal;
			else if(returnVal > max)
				return max;
			else
				return min;
		} else {
			return min;
		}
	}

}
