using UnityEngine;
using System.Collections;

public class ConnectK : MonoBehaviour 
{
	public GameObject gridSquare;
	public float gridSize;
	public GameObject red, yellow;
	Vector2 bottomLeft;

	public int width, height, matches;

	ConnectKBoard board;

	void Start()
	{
		board = new ConnectKBoard(width,height,matches);
		InitBoard();
	}

	void InitBoard()
	{
		//gridSize = gridSquare.renderer.bounds.size.x;
		bottomLeft = -(new Vector2(width/2 - gridSize/2,height/2 - gridSize/2));
		for(int i = 0; i < width; i++) {
			for(int j = 0; j < height; j++) {
				Vector2 pos = GetRealPosition(i,j);
				GameObject square = (GameObject)Instantiate(gridSquare,pos,Quaternion.identity);
			}
		}
	}

	Vector2 GetRealPosition(int x, int y)
	{
		return bottomLeft + new Vector2(x*gridSize,y*gridSize);
	}

}
