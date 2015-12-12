using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public static class FSNResourceCache
{
	/// <summary>
	/// 커스텀 리소스 로더에 대한 인터페이스
	/// </summary>
	public interface ICustomLoader
	{
		object LoadResource(string path);
		void UnloadResource(object res);
	}

	//==========================================================================


	/// <summary>
	/// 리소스 카테고리
	/// </summary>
	public enum Category
	{
		Engine,				// 엔진에서 로드
		Script,				// 스크립트에서 로드

		UI,					// UI 객체들
	}

	class ResourceBox
	{
		public object		res;
		public System.Type	type;
	}

	/// <summary>
	/// 카테고리 하나에 해당
	/// </summary>
	class Depot
	{
		public Dictionary<string, ResourceBox>	m_resourceDict	= new Dictionary<string, ResourceBox>();
	}

	// Static Members

	static Dictionary<Category, Depot>	s_depotDict	= new Dictionary<Category, Depot>();
	static Depot						s_tempDepot	= null;
	static Category						s_tempDepotName;

	static Dictionary<System.Type, ICustomLoader>	s_loader	= new Dictionary<System.Type, ICustomLoader>();


	/// <summary>
	/// 커스텀 리소스 로더 설치하기
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="loader"></param>
	public static void InstallLoader<T>(ICustomLoader loader)
	{
		s_loader[typeof(T)]	= loader;
	}


	public static T Load<T>(Category category, string assetname)
		where T : class
	{
		ResourceBox retv	= null;
		Depot depot	= null;
		if(!s_depotDict.TryGetValue(category, out depot))
		{
			depot					= new Depot();
			s_depotDict[category]	= depot;
		}

		if(!depot.m_resourceDict.TryGetValue(assetname, out retv))
		{
			retv					= new ResourceBox();
			retv.type				= typeof(T);
			ICustomLoader loader	= null;
			s_loader.TryGetValue(retv.type, out loader);
			if (loader != null)
			{
				retv.res	= loader.LoadResource(assetname);
			}
			else
			{
				retv.res	= Resources.Load(assetname) as T;
			}
			depot.m_resourceDict[assetname]	= retv;
		}

		if (s_tempDepot != null && s_tempDepotName == category)	// 로딩 세션 사용중이라면 임시 보관소에도 로딩 현황을 올린다.
		{
			s_tempDepot.m_resourceDict[assetname]	= retv;
		}
		
		return retv.res as T;
	}

	public static void UnloadCategory(Category category)
	{
		Depot depot	= null;
		if(s_depotDict.TryGetValue(category, out depot))
		{
			foreach(var box in depot.m_resourceDict.Values)
			{
				if (box.type != null)
				{
					var loader	= s_loader[box.type];
					loader.UnloadResource(box.res);
				}
			}
			depot.m_resourceDict.Clear();
		}

		Resources.UnloadUnusedAssets();
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
				if (!s_tempDepot.m_resourceDict.ContainsKey(resname))	// 새로 로딩된 리소스 중에 없는 것은 해제한다. (해제 목록에 넣는다)
				{
					removeList.Add(resname);
				}
			}

			foreach (var resname in removeList)							// 해제 목록을 순회
			{
				var box = oldDepot.m_resourceDict[resname];
				if (box.type != null)									// 만약 로더 구현이 존재하는 경우, 리소스 언로드를 실행
				{
					var loader	= s_loader[box.type];
					loader.UnloadResource(box.res);
				}
				oldDepot.m_resourceDict.Remove(resname);
			}
		}

		s_tempDepot	= null;		// 임시 저장소 해제

		Resources.UnloadUnusedAssets();
	}
}