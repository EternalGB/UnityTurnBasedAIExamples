using UnityEngine;
using System.Collections;
using GenericTurnBasedAI;

public class HashingTest : MonoBehaviour
{

	void Start()
	{
		GameState b1 = new ChessBoard(PieceColor.White);
		GameState b2 = new ChessBoard(PieceColor.Black);
		Debug.Log ("b1: " + b1.GetHashCode());
		Debug.Log ("b2: " + b2.GetHashCode());
		Debug.Log ("b1 clone: " + b1.Clone().GetHashCode());
	}

}

