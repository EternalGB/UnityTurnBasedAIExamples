using UnityEngine;
using System.Collections.Generic;
using GenericTurnBasedAI;
using UnityEngine.UI;

public class ConnectKHumanPlayer : TurnAgent
{

	public RectTransform buttonCanvas;
	public GameObject buttonPrefab;
	public Color ourColor;
	public ConnectKPiece player;
	//public int gridSize;


	List<Button> buttons;

	public override void Init(GameState state) 
	{
		ConnectKBoard board = state as ConnectKBoard;
		InitButtons(board,1);
	}

	void InitButtons(ConnectKBoard board, float gridSize)
	{

		buttonCanvas.position = new Vector3(0,gridSize/2);
		buttonCanvas.sizeDelta = new Vector2(board.nCols*gridSize, board.nRows*gridSize + gridSize);

		Vector2 topLeft = (new Vector2(-(board.nCols/2),board.nCols/2 + gridSize/2));

		buttons = new List<Button>();
		for(int i = 0; i < board.nCols; i++) {
			GameObject buttonObj = (GameObject)Instantiate(buttonPrefab, topLeft + new Vector2(gridSize*i,0), Quaternion.identity);
			buttonObj.GetComponent<Image>().color = ourColor;
			RectTransform trans = buttonObj.transform as RectTransform;
			trans.sizeDelta = new Vector2(gridSize,gridSize);
			trans.SetParent(buttonCanvas);
			Button button = buttonObj.GetComponent<Button>();
			int index = i;
			//register the button to generate the actual turn
			button.onClick.AddListener( () =>  {
				OnTurnReady(new ConnectKTurn(index,player));
				buttonCanvas.gameObject.SetActive(false);
			});
			buttons.Add(button);
		}
		buttonCanvas.gameObject.SetActive(false);
	}
	

	public override void GenerateNextTurn(GameState state)
	{
		ConnectKBoard board = state as ConnectKBoard;
		buttonCanvas.gameObject.SetActive(true);
		for(int i = 0; i < buttons.Count; i++) {
			if(board.ColumnFull(i))
				buttons[i].interactable = false;
			else
				buttons[i].interactable = true;
		}
	}



}

