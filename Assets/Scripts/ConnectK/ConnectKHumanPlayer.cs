using UnityEngine;
using System.Collections.Generic;
using UniversalTurnBasedAI;

/// <summary>
/// The Human player for Connect K. Creates and manages a bunch of buttons used
/// to create Turns.
/// </summary>
public class ConnectKHumanPlayer : TurnAgent
{
	
	public ConnectKPiece player;

	public Texture2D buttonImage;
	ConnectKBoard board;
	float gridSize = 1;
	float buttonSize;
	bool ourTurn;

	public override void Init(IGameState state) 
	{
		board = state as ConnectKBoard;
		Vector3 diff = Camera.main.WorldToScreenPoint(new Vector3(gridSize,0)) - Camera.main.WorldToScreenPoint(Vector3.zero);
		buttonSize = Mathf.Abs(diff.x);
		ourTurn = player == board.player;
	}

	void OnGUI()
	{
		if(board != null && ourTurn) {
			Vector3 topLeft = (new Vector2(-(board.nCols/2f),board.nRows/2f + gridSize));
			
			for(int i = 0; i < board.nCols; i++) {
				Vector3 offset = new Vector3(gridSize*i,0);
				Vector3 pos = Camera.main.WorldToScreenPoint(topLeft + offset);
				if(GUI.Button(new Rect(pos.x,Screen.height - pos.y,buttonSize,buttonSize),buttonImage)) {
					OnTurnReady(new ConnectKTurn(i,player));
					ourTurn = false;
				}
			}
		} 
	}

	public override void GenerateNextTurn(IGameState state)
	{
		ConnectKBoard board = state as ConnectKBoard;
		this.board = board;
		ourTurn = player == board.player;
	}



}

