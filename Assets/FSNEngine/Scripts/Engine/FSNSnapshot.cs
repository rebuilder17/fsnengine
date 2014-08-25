using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// 스크립트를 해석하여 만든 각 순간의 장면.
/// </summary>
public class FSNSnapshot
{
	/// <summary>
	/// 스냅샷 하나를 구성하는 각 요소
	/// </summary>
	public abstract class Element
	{
		private static int globalIDCount	= 0;		// Element마다 고유 ID를 부여하기 위한 static

		/// <summary>
		/// 이 Element의 고유 ID
		/// </summary>
		public int UniqueID { get; private set; }


		/// <summary>
		/// 해당 요소의 위치
		/// </summary>
		public Vector3	Position { get; set; }

		/// <summary>
		/// 해당 요소의 색상 (Tint)
		/// </summary>
		public Color Color { get; set; }

		//=============================================

		public Element()
		{
			UniqueID = globalIDCount++;			// 고유 ID 발급
		}

		/// <summary>
		/// 이 Element를 복제한 Element를 만든다.
		/// 동일한 UniqueID를 지니게 되며, 현재 스냅샷 - 다음 스냅샷으로 계승되는 Element를 만들어내기 위해 필요
		/// </summary>
		/// <returns></returns>
		public Element Clone()
		{
			Element newElem		= MakeInstance();
			newElem.UniqueID	= UniqueID;			// UniqueID 복제
			CopyDataTo(newElem);					// 기타 데이터 복제

			return newElem;
		}

		/// <summary>
		/// 현재 데이터를 to 로 복제한다. 오버라이드해서 상속한 클래스의 데이터도 복제하는 코드를 추가한다
		/// </summary>
		/// <param name="to"></param>
		protected virtual void CopyDataTo(Element to)
		{
			to.Position	= Position;
			to.Color	= Color;
		}

		/// <summary>
		/// 현재 클래스의 인스턴스를 생성한다. 반드시 상속한 클래스 내에서 정의한다.
		/// 그냥 자기 자신을 new해서 리턴하면 됨.
		/// </summary>
		/// <returns></returns>
		protected abstract Element MakeInstance();
	}

	/// <summary>
	/// Element 묶음 (레이어)
	/// </summary>
	public sealed class Layer
	{
		/// <summary>
		/// 비교 결과에 관한 정의
		/// </summary>
		public struct Match
		{
			/// <summary>
			/// 두 Layer에 모두 포함됨
			/// </summary>
			public int[] Matching;
			/// <summary>
			/// This 에 해당하는 Layer에만 있음
			/// </summary>
			public int[] OnlyInThis;
			/// <summary>
			/// Other 에 해당하는 Layer에만 있음
			/// </summary>
			public int[] OnlyInOther;
		}

		//==========================================================

		private Dictionary<int, Element>	m_elements;		// 레이어에 포함된 Element

		public Layer()
		{
			m_elements	= new Dictionary<int, Element>();
		}

		/// <summary>
		/// Element 추가
		/// </summary>
		/// <param name="elem"></param>
		public void AddElement(Element elem)
		{
			m_elements.Add(elem.UniqueID, elem);
		}

		/// <summary>
		/// UniqueID 로 element를 구한다
		/// </summary>
		/// <param name="uId"></param>
		/// <returns></returns>
		public Element GetElement(int uId)
		{
			return m_elements[uId];
		}

		/// <summary>
		/// 이 Layer를 복제한 오브젝트를 만들어낸다.
		/// 현재 스냅샷의 다음 스냅샷을 만들 시, 계승되는 오브젝트에 관한 관리를 하기 위해 필요함
		/// </summary>
		/// <returns></returns>
		public Layer Clone()
		{
			Layer newLayer	= new Layer();

			foreach(Element elem in m_elements.Values)						// 가지고 있는 Element 를 모두 복제한다. (UniqueID까지 복사)
			{
				newLayer.AddElement(elem.Clone());
			}

			return newLayer;
		}

		/// <summary>
		/// 다른 Layer와 비교해서 두 레이어에 모두 포함되는 Element와 그렇지 않은 Element를 찾아내 리턴한다
		/// </summary>
		/// <param name="other"></param>
		/// <param name="matching"></param>
		/// <param name="notmatching"></param>
		public Match CompareAndReturnElements(Layer other)
		{
			Match result;

			HashSet<int> inThis		= new HashSet<int>(m_elements.Keys);
			HashSet<int> inOther	= new HashSet<int>(other.m_elements.Keys);

			HashSet<int> matching	= new HashSet<int>(inThis);
			matching.IntersectWith(inOther);								// 두 Layer에 모두 포함되는 것들

			inThis.ExceptWith(matching);									// 각각 교집합 부분을 모두 소거한다 (각자에게만 있는 것만 남음)
			inOther.ExceptWith(matching);

			result.Matching			= FSNUtils.MakeArray<int>(matching);
			result.OnlyInThis		= FSNUtils.MakeArray<int>(inThis);
			result.OnlyInOther		= FSNUtils.MakeArray<int>(inOther);
			return result;
		}
	}

	/// <summary>
	/// 미리 정의된 레이어
	/// </summary>
	public enum PreDefinedLayers
	{
		Text	= 99,
	}

	//=========================================================================================

	private Dictionary<int, Layer>	m_layerList;			// 레이어 목록
	private Dictionary<string, int>	m_layerIDList;			// 레이어 ID 목록

	private static PreDefinedLayers[]	PreDefinedLayerIDs;	// 미리 정의된 레이어 ID 목록


	/// <summary>
	/// 역진행의 경우, Swipe 동작으로 끊어지지 않고 바로 이전 스냅샷으로 이어지는가에 대하여
	/// </summary>
	public bool LinkToForward { get; set; }

	/// <summary>
	/// 순진행의 경우, Swipe 동작으로 끊어지지 않고 바로 다음 스냅샷으로 이어지는가에 대하여
	/// </summary>
	public bool LinkToBackward { get; set; }


	// TODO : 실행 중 세팅값들도 필요한 것들은 Snapshot에 보관되어야함

	/// <summary>
	/// 현재 스냅샷의 인게임 세팅
	/// </summary>
	public FSNInGameSetting InGameSetting { get; set; }


	//========================================================================================

	public FSNSnapshot()
	{
		m_layerIDList	= new Dictionary<string, int>();
		m_layerList		= new Dictionary<int, Layer>();

		InGameSetting	= FSNInGameSetting.DefaultInGameSetting;
	}

	static FSNSnapshot()
	{
		PreDefinedLayerIDs = System.Enum.GetValues(typeof(PreDefinedLayers)) as PreDefinedLayers[];
	}



	/// <summary>
	/// 현재 이 스냅샷에 등장하는 미리 정의되지 않은 (= 유저가 정의한) 레이어 ID 모두 구하기
	/// </summary>
	public int[] NonPreDefinedLayerIDs
	{
		get
		{
			HashSet<int> ids		= new HashSet<int>(m_layerList.Keys);
			foreach(PreDefinedLayers predef in PreDefinedLayerIDs)
			{
				ids.Remove((int)predef);
			}

			return FSNUtils.MakeArray<int>(ids);
		}
	}

	/// <summary>
	/// Layer 구하기
	/// </summary>
	/// <param name="layer"></param>
	/// <returns></returns>
	public Layer GetLayer(PreDefinedLayers layer)
	{
		return GetLayer((int)layer);
	}

	/// <summary>
	/// Layer 구하기
	/// </summary>
	/// <param name="layerID"></param>
	/// <returns></returns>
	public Layer GetLayer(int layerID)
	{
		return m_layerList[layerID];
	}

	/// <summary>
	/// Layer 구하기
	/// </summary>
	/// <param name="layerName"></param>
	/// <returns></returns>
	public Layer GetLayer(string layerName)
	{
		int layerID	= m_layerIDList[layerName];
		return GetLayer(layerID);
	}
}

