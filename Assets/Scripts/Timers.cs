using UnityEngine;
using System.Collections;
using System;

public class Timers 
{

	public static IEnumerator Countdown(float duration, Action callback)
	{
		for(float timer = duration; timer >= 0; timer -= Time.deltaTime)
			yield return 0;
		
		callback();
	}

}

