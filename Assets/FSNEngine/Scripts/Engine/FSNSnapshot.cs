using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// 스크립트를 해석하여 만든 각 순간의 장면.
/// </summary>
public class FSNSnapshot
{
	/// <summary>
	/// (interface) 스냅샷 하나를 구성하는 각 요소
	/// </summary>
	public interface IElement
	{
		int UniqueID { get; }

		/// <summary>
		/// 이 Element를 복제한 Element를 만든다. (Clone의 IElement 버전)
		/// </summary>
		/// <returns></returns>
		IElement GenericClone();

		/// <summary>
		/// 생성 당시의 초기 상태. 이 상태에서 최초로 지정된 상태로 transition한다
		/// (예 : 화면 바깥 -> 안쪽으로 등장할 때, InitialState는 화면 바깥 좌표)
		/// </summary>
		IElement GenericInitialState { get; }

		/// <summary>
		/// 사라질 시의 마지막 상태. 맨 마지막 상태에서 이 상태로 transition한다
		/// (예 : 화면 가장자리 -> 바깥으로 움직이며 사라질 때, FinalState는 화면 바깥 좌표)
		/// </summary>
		IElement GenericFinalState { get; }
	}

	/// <summary>
	/// 스냅샷 하나를 구성하는 각 요소
	/// </summary>
	public abstract class Element<SelfT> : IElement
		where SelfT : Element<SelfT>, new()
	{
		private static int globalIDCount	= 1;		// Element마다 고유 ID를 부여하기 위한 static

		/// <summary>
		/// 이 Element의 고유 ID
		/// </summary>
		public int UniqueID { get; private set; }

		/// <summary>
		/// 사용 가능한지 여부. 즉, MakeItUnique가 호출되었었는지/혹은 제대로 Clone되었는지
		/// </summary>
		public bool CanUse { get { return UniqueID != 0; } }


		/// <summary>
		/// 해당 요소의 위치
		/// </summary>
		public Vector3	Position { get; set; }

		/// <summary>
		/// 해당 요소의 색상 (Tint)
		/// </summary>
		public Color Color { get; set; }


		/// <summary>
		/// 생성 당시의 초기 상태. 이 상태에서 최초로 지정된 상태로 transition한다
		/// (예 : 화면 바깥 -> 안쪽으로 등장할 때, InitialState는 화면 바깥 좌표)
		/// </summary>
		public SelfT InitialState	{ get; private set; }

		/// <summary>
		/// 사라질 시의 마지막 상태. 맨 마지막 상태에서 이 상태로 transition한다
		/// (예 : 화면 가장자리 -> 바깥으로 움직이며 사라질 때, FinalState는 화면 바깥 좌표)
		/// </summary>
		public SelfT FinalState		{ get; private set; }

		

		//=============================================

		/// <summary>
		/// 이 오브젝트에 고유 ID 발급, 시작/끝상태를 새로 추가한다. 사실상의 초기화 루틴
		/// Clone하지 않는 오브젝트는 반드시 이를 호출해줘야함
		/// </summary>
		public void MakeItUnique()
		{
			UniqueID		= globalIDCount++;		// 고유 ID 발급

			InitialState	= Clone();				// InitialState/FinalState 를 자기 복제로 만들어둔다
			FinalState		= Clone();
		}

		/// <summary>
		/// 이 Element를 복제한 Element를 만든다.
		/// 동일한 UniqueID를 지니게 되며, 현재 스냅샷 - 다음 스냅샷으로 계승되는 Element를 만들어내기 위해 필요
		/// </summary>
		/// <returns></returns>
		public SelfT Clone()
		{
			if(!CanUse)
			{
				throw new System.InvalidOperationException("Element is not valid. If it's an original object, call MakeItUnique before any use.");
			}

			//SelfT newElem		= MakeInstance();
			SelfT newElem		= new SelfT();
			newElem.UniqueID	= UniqueID;			// UniqueID 복제
			CopyDataTo(newElem);					// 기타 데이터 복제

			return newElem;
		}


		/// <summary>
		/// 현재 데이터를 to 로 복제한다. 오버라이드해서 상속한 클래스의 데이터도 복제하는 코드를 추가한다
		/// </summary>
		/// <param name="to"></param>
		protected virtual void CopyDataTo(SelfT to)
		{
			to.Position	= Position;
			to.Color	= Color;
		}




		// IElement 구현. 이 클래스에 구현된 내용 랩핑
		public IElement GenericInitialState { get { return InitialState; } }
		public IElement GenericFinalState { get { return FinalState; } }
		public IElement GenericClone()	{ return Clone(); }
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

		private Dictionary<int, IElement>	m_elements;		// 레이어에 포함된 Element

		public Layer()
		{
			m_elements	= new Dictionary<int, IElement>();
		}

		/// <summary>
		/// Element 추가
		/// </summary>
		/// <param name="elem"></param>
		public void AddElement(IElement elem)
		{
			m_elements.Add(elem.UniqueID, elem);
		}

		/// <summary>
		/// UniqueID 로 element를 구한다
		/// </summary>
		/// <param name="uId"></param>
		/// <returns></returns>
		public IElement GetElement(int uId)
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

			foreach(IElement elem in m_elements.Values)						// 가지고 있는 Element 를 모두 복제한다. (UniqueID까지 복사)
			{
				newLayer.AddElement(elem.GenericClone());
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

