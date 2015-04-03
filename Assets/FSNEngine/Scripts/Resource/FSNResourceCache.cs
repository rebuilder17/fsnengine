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
	//static Dictionary<Category, Depot>	s_tempDepots= null;
	static Depot						s_tempDepot	= null;
	static Category						s_tempDepotName;


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

		if (s_tempDepot != null && s_tempDepotName == category)	// 로딩 세션 사용중이라면 임시 보관소에도 로딩 현황을 올린다.
		{
			s_tempDepot.m_resourceDict[assetname]	= retv;
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

	/// <summary>
	/// 스크립트 간 전환, Scene 전환 등 다량의 리소스를 새로 불러오거나 버려야 하는 상황에 호출
	/// </summary>
	public static void StartLoadingSession(Category category)
	{
		s_tempDepot		= new Depot();
		s_tempDepotName	= category;
	}

	/// <summary>
	/// 로딩 상황 끝. 이번 세션에 로딩한 (= 이번 세션에 필요한) 리소스만 남기고 해제한다.
	/// </summary>
	public static void EndLoadingSession()
	{
		Depot oldDepot	= null;
		s_depotDict.TryGetValue(s_tempDepotName, out oldDepot);

		if (oldDepot != null)
		{
			HashSet<string> removeList	 = new HashSet<string>();

			foreach(var resname in oldDepot.m_resourceDict.Keys)
			{
				if (!s_tempDepot.m_resourceDict.ContainsKey(resname))	// 새로 로딩된 리소스 중에 없는 것은 해제한다.
				{
					//oldDepot.m_resourceDict.Remove(resname);
					removeList.Add(resname);
				}
			}

			foreach (var resname in removeList)
				oldDepot.m_resourceDict.Remove(resname);
		}

		s_tempDepot	= null;		// 임시 저장소 해제
	}
}