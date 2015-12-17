using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Z 축 좌표에 맞춰서 자식 CanvasRenderer 들을 정렬하는 임시 헬퍼 컴포넌트
/// </summary>
public class FSNNewUISort : MonoBehaviour
{
	[SerializeField]
	bool			m_exactCanvasRendererCheck = false; // Canvas Renderer가 있는 것만 체크
	[SerializeField]
	int				m_baseSortOrder	= 0;				// sort order를 오버라이드하는 방식을 사용하는 경우, 그 베이스값
	
	System.Action	m_updateRoutine;

	void Awake()
	{
		if (m_exactCanvasRendererCheck)
		{
			var tempTrList  = new List<RectTransform>();
			m_updateRoutine = () =>
			{
				tempTrList.Clear();

				var root    = transform;
				int count   = root.childCount;

				for (int i = 0; i < count; i++)                                 // 자식 순회
				{
					var tr  = root.GetChild(i);
					if (tr.GetComponent<CanvasRenderer>())                      // CanvasRenderer가 있는 경우에만 RectTransform을 얻어온다
					{
						tempTrList.Add(tr.GetComponent<RectTransform>());
					}
				}

				tempTrList.Sort((a, b) =>                                     // Z 값에 따라 정렬하기
				{
					var az      = a.anchoredPosition3D.z;
					var bz      = b.anchoredPosition3D.z;
					var diff    = az - bz;

					if (diff < Mathf.Epsilon && diff > -Mathf.Epsilon)
						return 0;
					else
						//return diff < 0 ? -1 : 1;
						return diff > 0 ? -1 : 1;
				});

				int listcount   = tempTrList.Count;
				for (int i = 0; i < listcount; i++)                             // 정렬한 순서에 따라서 자식 순서 맞추기
				{
					tempTrList[i].SetAsLastSibling();
				}
			};
        }
		else
		{
			var tempTrList  = new List<Transform>();
			m_updateRoutine = () =>
			{
				tempTrList.Clear();

				var root    = transform;
				int count   = root.childCount;

				for (int i = 0; i < count; i++)                                 // 자식 순회
				{
					var tr  = root.GetChild(i);
					tempTrList.Add(tr);
				}

				tempTrList.Sort((a, b) =>                                     // Z 값에 따라 정렬하기
				{
					var az      = a.localPosition.z;
					var bz      = b.localPosition.z;
					var diff    = az - bz;

					if (diff < Mathf.Epsilon && diff > -Mathf.Epsilon)
						return 0;
					else
						//return diff < 0 ? -1 : 1;
						return diff > 0 ? -1 : 1;
				});

				int listcount   = tempTrList.Count;
				for (int i = 0; i < listcount; i++)                             // 정렬한 순서에 따라서 자식 순서 맞추기
				{
					//tempTrList[i].SetAsLastSibling();
					
					var canvas	= tempTrList[i].GetComponentInChildren<Canvas>();
					canvas.overrideSorting	= true;
					canvas.sortingOrder = m_baseSortOrder + i;
				}
			};
        }
	}

	
	void Update()
	{
		m_updateRoutine();
	}
}
