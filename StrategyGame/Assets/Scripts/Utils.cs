using System;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{

	public static V GetDictionaryDefault<K, V>(Dictionary<K, V> dict, K key, V value)
	{
		V v;
		if (dict.TryGetValue(key, out v))
		{
			return v;
		}
		return value;
	}

	public static int GetDictionaryIntDefault<K>(Dictionary<K, float> dict, K key, int value)
	{
		float v;
		if (dict.TryGetValue(key, out v))
		{
			return Mathf.RoundToInt(v);
		}
		return value;
	}
}
