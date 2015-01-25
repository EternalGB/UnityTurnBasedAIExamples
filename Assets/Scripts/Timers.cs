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

	public static IEnumerator CountdownRealtime(float duration, Action callback)
	{
		float start = Time.realtimeSinceStartup;
		while(Time.realtimeSinceStartup < start + duration)
			yield return 0;
		callback();
	}

	public static IEnumerator Countdown<T>(float duration, Action<T> callback, T arg)
	{
		for(float timer = duration; timer >= 0; timer -= Time.deltaTime)
			yield return 0;
		
		callback(arg);
	}

	public static IEnumerator Countdown<T1, T2>(float duration, Action<T1, T2> callback, T1 arg1, T2 arg2)
	{
		for(float timer = duration; timer >= 0; timer -= Time.deltaTime)
			yield return 0;
		
		callback(arg1,arg2);
	}

}

