using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// 각종 유틸리티 함수 모음
/// </summary>
public static class FSNUtils
{
	/// <summary>
	/// ICollection의 원소를 모두 복사한 Array를 만들어낸다
	/// </summary>
	/// <param name="enumerable"></param>
	/// <returns></returns>
	public static T[] MakeArray<T>(ICollection<T> collection)
	{
		T[] array	= new T[collection.Count];
		collection.CopyTo(array, 0);
		return array;
	}
}
