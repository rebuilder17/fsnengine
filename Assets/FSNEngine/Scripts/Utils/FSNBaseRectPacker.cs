using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace FSNRectPacker
{
	/// <summary>
	/// 텍스쳐 팩커용 데이터를 위한 인터페이스
	/// </summary>
	public interface IData
	{
		int width { get; }
		int height { get; }
	}

	/// <summary>
	/// 텍스쳐 팩커 베이스 클래스
	/// </summary>
	/// <typeparam name="DataT"></typeparam>
	public abstract class BaseRectPacker<DataT>
		where DataT : class, IData
	{
		class Node
		{
			struct RectSpace
			{
				public int xMin, yMin, xMax, yMax;
				public int padding;

				public int width { get { return xMax - xMin + padding; } }
				public int height {  get { return yMax - yMin + padding; } }

				public int widthNoPad { get { return xMax - xMin; } }
				public int heightNoPad { get { return yMax - yMin; } }

				/// <summary>
				/// 특정 width, height를 지닌 사각형을 포함할 수 있는지 여부
				/// </summary>
				/// <param name="tw"></param>
				/// <param name="th"></param>
				/// <returns></returns>
				public bool CanContain(int tw, int th)
				{
					return (width >= tw) && (height >= th);
				}

				public bool FitsCorrectly(int tw, int th)
				{
					return (width == tw) && (height == th);
				}

				/// <summary>
				/// width 방향으로 지정한 길이만큼 나눠서 Rect로 리턴한다
				/// </summary>
				/// <param name="divideLength"></param>
				/// <param name="divRect"></param>
				/// <param name="remainRect"></param>
				public void DivideHorizontally(int divideLength, out RectSpace divRect, out RectSpace remainRect)
				{
					divRect			= this;
					remainRect		= this;

					int divline		= divRect.xMin + divideLength;
					divRect.xMax    = divline - padding;	// Padding이 포함되어있으므로 제거해야한다.
					remainRect.xMin = divline;
                }

				/// <summary>
				/// height 방향으로 지정한 길이만큼 나눠서 Rect로 리턴한다
				/// </summary>
				/// <param name="divideLength"></param>
				/// <param name="divRect"></param>
				/// <param name="remainRect"></param>
				public void DivideVertically(int divideLength, out RectSpace divRect, out RectSpace remainRect)
				{
					divRect         = this;
					remainRect      = this;

					int divline     = divRect.yMin + divideLength;
					divRect.yMax    = divline - padding;    // Padding이 포함되어있으므로 제거해야한다.
					remainRect.yMin = divline;
				}
			}
			//

			// Members

			Node []		m_child	= new Node[2];
			DataT		m_data	= null;
			RectSpace   m_rect;

			/// <summary>
			/// 이 노드가 leaf인지
			/// </summary>
			bool isLeaf
			{
				get { return m_child[0] == null && m_child[1] == null; }
			}

			//

			public Node(int width, int height, int padding) : this(0, 0, width, height, padding)
			{

			}

			public Node(int xMin, int yMin, int xMax, int yMax, int padding)
			{
				m_rect  = new RectSpace() { xMin = xMin, yMin = yMin, xMax = xMax, yMax = yMax, padding = padding };
            }

			private Node(RectSpace rect)
			{
				m_rect  = rect;
			}
			//

			/// <summary>
			/// 자기 자신 혹은 하위 레벨에 데이터를 삽입한다.
			/// </summary>
			/// <returns>null이 아닐 경우, 해당 데이터를 포함한 노드. NULL일 경우 데이터 삽입 실패.</returns>
			public Node InsertData(DataT data)
			{
				if (!isLeaf)	// Leaf 노드가 아닐 경우, Leaf 노드쪽으로 Insert를 시도한다.
				{
					return m_child[0].InsertData(data) ?? m_child[1].InsertData(data);
				}
				else
				{               // Leaf 노드일 경우

					if (m_data != null || !m_rect.CanContain(data.width, data.height))	// 이미 데이터가 할당되었거나 데이터 사이즈에 맞지 않는 경우, null 리턴
						return null;
					else if (m_rect.FitsCorrectly(data.width, data.height))             // 딱 맞는 빈 공간일 경우엔 데이터 삽입 후 리턴
					{
						m_data  = data;
						return this;
					}
					else
					{
						RectSpace divRect, remainRect;

						// 사각형을 배치하고 나눴을 때, 어느쪽을 크게 잘라야할지 계산한다. (좀더 큰 공간이 남는 편이 유리하다)
						int remainw	= m_rect.width - data.width;
						int remainh	= m_rect.height - data.height;
						if (remainw > remainh)						// 가로방향을 나눠야함
						{
							m_rect.DivideHorizontally(data.width, out divRect, out remainRect);
						}
						else
						{                                           // 세로방향을 나눠야함
							m_rect.DivideVertically(data.height, out divRect, out remainRect);
						}
						m_child[0]  = new Node(divRect);
						m_child[1]  = new Node(remainRect);

						return m_child[0].InsertData(data);			// 나눈 방향에 대해서 데이터 크기와 맞춘 노드 쪽에 insert 시도
                    }
				}
			}

			/// <summary>
			/// 트리 전체를 순회하며 리스트에 결과값들을 추가한다.
			/// </summary>
			/// <param name="list"></param>
			public void BuildOutput(LinkedList<Output> list)
			{
				if (!isLeaf)                // Leaf가 아니면 자식들을 콜한다.
				{
					m_child[0].BuildOutput(list);
					m_child[1].BuildOutput(list);
				}
				else
				{
					if (m_data != null)		//  데이터가 있는 경우 리스트로 출력한다.
					{
						var output  = new Output();
						output.data = m_data;
						output.xMin = m_rect.xMin;
						output.xMax = output.xMin + m_rect.widthNoPad - 1;
						output.yMin = m_rect.yMin;
						output.yMax = output.yMin + m_rect.heightNoPad - 1;
                        list.AddLast(output);
					}
				}
			}
		}

		/// <summary>
		/// 데이터 링크드 리스트에서 사용할 타입
		/// </summary>
		class DataListItem
		{
			/// <summary>
			/// 정렬에 사용할 정렬 기준값
			/// </summary>
			public int weight { get; private set; }
			public DataT data { get; private set; }

			public DataListItem(DataT data)
			{
				this.data   = data;
				weight      = GetWeight(data);
			}

			/// <summary>
			/// 정렬 기준값을 계산한다.
			/// </summary>
			/// <param name="data"></param>
			/// <returns></returns>
			public static int GetWeight(DataT data)
			{
				return Mathf.Max(data.width, data.height);  // 가로/세로 중 큰 축을 정렬 기준으로 삼는다.
			}
		}

		/// <summary>
		/// 패킹 결과 데이터
		/// </summary>
		protected struct Output
		{
			public int xMin, xMax, yMin, yMax;
			public DataT data;
		}
		//

		// Members

		Node						m_rootNode;				// 패킹 계산에 사용할 데이터 노드
		LinkedList<DataListItem>	m_dataList;             // 패킹 계산 전에 데이터를 모아둘 리스트

		int m_outWidth, m_outHeight;						// 출력할 패킹 데이터 크기

		public BaseRectPacker()
		{
			m_dataList  = new LinkedList<DataListItem>();
		}

		/// <summary>
		/// 패킹을 할 데이터 집어넣기
		/// </summary>
		/// <param name="data"></param>
		public void PushData(DataT data)
		{
			int dataDim = DataListItem.GetWeight(data);
			var curNode = m_dataList.First;

			while(curNode != null)					// 삽입할 위치를 찾는다.
			{
				var value   = curNode.Value;
				if (dataDim <= value.weight)		// 삽입하려는 데이터의 비중이 더 크다면 다음 위치로 계속 진행해야 한다. (내림차순)
				{
					curNode = curNode.Next;
				}
				else
				{
					break;
				}
			}

			if (curNode == null)
			{
				m_dataList.AddLast(new DataListItem(data));
			}
			else
			{
				m_dataList.AddBefore(curNode, new DataListItem(data));
			}
		}

		/// <summary>
		/// 쌓아둔 데이터 삭제
		/// </summary>
		public void Clear()
		{
			m_dataList.Clear();
			m_rootNode  = null;
		}

		/// <summary>
		/// 쌓아둔 데이터를 일정 크기 안에 패킹한다.
		/// </summary>
		/// <param name="packWidth"></param>
		/// <param name="packHeight"></param>
		public bool Pack(int packWidth, int packHeight, int padding = 1)
		{
			m_outWidth  = packWidth;
			m_outHeight = packHeight;
			m_rootNode  = new Node(packWidth, packHeight, padding);

			foreach (var entry in m_dataList)					// 데이터를 하나씩 넣는다.
			{
				if (m_rootNode.InsertData(entry.data) == null)  // 만약 삽입할 수 없는 데이터가 나타날 경우
				{
					OnFailure();
					return false;
				}
			}

			// 성공, 출력 리스트를 만들어 콜백에 전달한다.
			var output		= new LinkedList<Output>();
			m_rootNode.BuildOutput(output);
			var outputArray = new Output[output.Count];
			output.CopyTo(outputArray, 0);

			OnSuccess(m_outWidth, m_outHeight, outputArray);

			return true;
		}

		/// <summary>
		/// 성공시 콜백. 여기에서 외부 데이터로 출력해야 한다.
		/// </summary>
		/// <param name="output"></param>
		protected abstract void OnSuccess(int width, int height, Output[] output);
		/// <summary>
		/// 실패시 콜백
		/// </summary>
		protected abstract void OnFailure();
	}
}

