using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public static class FSNResourceCache
{
	/// <summary>
	/// 리소스 카테고리
	/// </summary>
	public enum Category
	{
		Engine,				// 엔진에서 로드
		Script,				// 스크립트에서 로드

		UI,					// UI 객체들
	}

	/// <summary>
	/// 카테고리 하나에 해당
	/// </summary>
	class Depot
	{
		public Dictionary<string, Object>	m_resourceDict	= new Dictionary<string, Object>();
	}

	// Static Members

	static Dictionary<Category, Depot>	s_depotDict	= new Dictionary<Category, Depot>();


	public static T Load<T>(Category category, string assetname)
		where T : Object
	{
		Object retv	= null;
		Depot depot	= null;
		if(!s_depotDict.TryGetValue(category, out depot))
		{
			depot					= new Depot();
			s_depotDict[category]	= depot;
		}

		if(!depot.m_resourceDict.TryGetValue(assetname, out retv))
		{
			retv	= Resources.Load<T>(assetname);
			depot.m_resourceDict[assetname]	= retv;
		}
		
		return retv as T;
	}

	public static void UnloadCategory(Category category)
	{
		Depot depot	= null;
		if(s_depotDict.TryGetValue(category, out depot))
		{
			depot.m_resourceDict.Clear();
		}
	}
}