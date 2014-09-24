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

		/// <summary>
		/// 이 오브젝트의 원본 오브젝트. 즉, "이전 상태"에 해당하는 것
		/// </summary>
		IElement GenericClonedFrom { get; }

		Vector3 Position { get; }
		Color Color { get; }
		float Alpha { get; }
		float TransitionTime { get; }

		/// <summary>
		/// 사용 가능한지 여부
		/// </summary>
		bool CanUse { get; }

		/// <summary>
		/// 복제로 연결된 엘레먼트가 총 몇 개 있는지
		/// </summary>
		int ChainedParentCount { get; }
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
		/// Color 에 추가로 적용되는 Alpha
		/// </summary>
		public float Alpha { get; set; }

		/// <summary>
		/// 트랜지션 애니메이션 시간.
		/// 특정적으로, InitialState/FinalState 에서는 처음 등장/마지막 사라지는 시간을 나타낸다
		/// </summary>
		public float TransitionTime { get; set; }


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

		/// <summary>
		/// 오브젝트의 오리지널, 즉 "이전 상태"
		/// </summary>
		public SelfT ClonedFrom		{ get; private set; }

		/// <summary>
		/// 복제로 연결된 엘레먼트가 총 몇 개 있는지
		/// </summary>
		public int ChainedParentCount
		{
			get
			{
				// TODO : 카운트를 캐싱해두고 ClonnedFrom 에 무언가 지정할 때 계산하는 방식으로 최적화가 가능할 것 같음.

				int count		= 0;
				var parentRef	= ClonedFrom;
				while(parentRef != null)
				{
					parentRef	= parentRef.ClonedFrom;
					count++;
				}
				return count;
			}
		}

		

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

			ClonedFrom		= null;
		}

		/// <summary>
		/// 이 Element를 복제한 Element를 만든다.
		/// 동일한 UniqueID를 지니게 되며, 현재 스냅샷 - 다음 스냅샷으로 계승되는 Element를 만들어내기 위해 필요
		/// </summary>
		/// <param name="cloneInitialFinalState">시작/끝 상태도 복제할 것인지. false면 복제하지 않고 원본의 것을 그대로 사용한다. (레퍼런싱)</param>
		/// <returns></returns>
		public SelfT Clone(bool cloneInitialFinalState = false)
		{
			if(!CanUse)
			{
				throw new System.InvalidOperationException("Element is not valid. If it's an original object, call MakeItUnique before any use.");
			}

			//SelfT newElem		= MakeInstance();
			SelfT newElem		= new SelfT();
			newElem.UniqueID	= UniqueID;			// UniqueID 복제
			if(cloneInitialFinalState)				// 플래그가 있을 경우, Initial/Final State도 복제
			{
				InitialState	= InitialState.Clone();
				FinalState		= FinalState.Clone();
			}
			newElem.ClonedFrom	= this as SelfT;	// 이 오브젝트가 오리지널

			CopyDataTo(newElem);					// 기타 데이터 복제

			return newElem;
		}


		/// <summary>
		/// 현재 데이터를 to 로 복제한다. 오버라이드해서 상속한 클래스의 데이터도 복제하는 코드를 추가한다
		/// </summary>
		/// <param name="to"></param>
		protected virtual void CopyDataTo(SelfT to)
		{
			to.InitialState	= InitialState;
			to.FinalState	= FinalState;

			to.Position	= Position;
			to.Color	= Color;
			to.Alpha	= Alpha;
			to.TransitionTime	= TransitionTime;
		}




		// IElement 구현. 이 클래스에 구현된 내용 랩핑
		public IElement GenericInitialState { get { return InitialState; } }
		public IElement GenericFinalState { get { return FinalState; } }
		public IElement GenericClone()	{ return Clone(); }
		public IElement GenericClonedFrom { get { return ClonedFrom; } }
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


		// Members
		
		private Dictionary<int, IElement>	m_elements;		// 레이어에 포함된 Element

		/// <summary>
		/// 추가 데이터
		/// </summary>
		public object CustomData { get; set; }

		// Static Memebers

		static Dictionary<string, int>		s_nameAlias;	// Layer 이름
		static Layer()
		{
			s_nameAlias	= new Dictionary<string, int>();
		}
		/// <summary>
		/// 정수 ID를 대신하는 string 이름 지정하기
		/// </summary>
		/// <param name="id"></param>
		/// <param name="name"></param>
		public static void AddLayerNameAlias(int id, string name)
		{
			s_nameAlias[name]	= id;
		}
		/// <summary>
		/// string 이름에서 정수 ID 가져오기
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public static int GetLayerIdFromAlias(string name)
		{
			return s_nameAlias[name];
		}


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
			if(!elem.CanUse)						// MakeItUnique 혹은 Clone을 거치지 않은 IElement는 사용할 수 없다
				throw new System.InvalidOperationException("Element is not valid. If it's an original object, call MakeItUnique before any use.");

			m_elements.Add(elem.UniqueID, elem);
		}

		/// <summary>
		/// Element 삭제
		/// </summary>
		/// <param name="uId"></param>
		public void RemoveElement(int uId)
		{
			m_elements.Remove(uId);
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
		/// 해당 UniqueID 를 지닌 element가 있는지 여부
		/// </summary>
		/// <param name="uId"></param>
		/// <returns></returns>
		public bool HasElement(int uId)
		{
			return m_elements.ContainsKey(uId);
		}

		/// <summary>
		/// 현재 포함되어있는 ID 목록
		/// </summary>
		public ICollection<int> UniqueIDList
		{
			get
			{
				return m_elements.Keys;
			}
		}

		/// <summary>
		/// 포함되어있는 엘레먼트들
		/// </summary>
		public ICollection<IElement> Elements
		{
			get
			{
				return m_elements.Values;
			}
		}

		public bool IsEmpty
		{
			get { return m_elements.Count == 0; }
		}

		/// <summary>
		/// 이 Layer를 복제한 오브젝트를 만들어낸다.
		/// 현재 스냅샷의 다음 스냅샷을 만들 시, 계승되는 오브젝트에 관한 관리를 하기 위해 필요함
		/// NOTE : 추가 데이터 (CustomData) 는 복제하지 않음.
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

		/// <summary>
		/// 빈 레이어
		/// </summary>
		public static readonly Layer Empty	= new Layer();
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
	//private Dictionary<string, int>	m_layerIDList;			// 레이어 ID 목록

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
	public IInGameSetting InGameSetting { get; set; }


	//========================================================================================

	public FSNSnapshot()
	{
		//m_layerIDList	= new Dictionary<string, int>();
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
		Layer layer	= null;
		m_layerList.TryGetValue(layerID, out layer);
		return layer;
	}

	/// <summary>
	/// Layer 구하기
	/// </summary>
	/// <param name="layerName"></param>
	/// <returns></returns>
	public Layer GetLayer(string layerName)
	{
		int layerID	= Layer.GetLayerIdFromAlias(layerName);
		return GetLayer(layerID);
	}

	public Layer MakeNewLayer(int layerID)
	{
		var newLayer			= new Layer();
		m_layerList[layerID]	= newLayer;
		return newLayer;
	}

	/// <summary>
	/// 레이어 세팅하기
	/// </summary>
	/// <param name="layerID"></param>
	/// <param name="layer"></param>
	public void SetLayer(int layerID, Layer layer)
	{
		m_layerList[layerID]	= layer;
	}

	/// <summary>
	/// 레이어 세팅하기
	/// </summary>
	/// <param name="layerName"></param>
	public void SetLayer(string layerName, Layer layer)
	{
		int layerID	= Layer.GetLayerIdFromAlias(layerName);
		SetLayer(layerID, layer);
	}
}

