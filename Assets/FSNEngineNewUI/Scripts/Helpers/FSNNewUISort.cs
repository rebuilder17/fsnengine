using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Z 축 좌표에 맞춰서 자식 CanvasRenderer 들을 정렬하는 임시 헬퍼 컴포넌트
/// </summary>
public class FSNNewUISort : MonoBehaviour
{
	List<RectTransform> m_tempTrList    = new List<RectTransform>();	// 임시로 오브젝트들을 두는 리스트
	void Update()
	{
		m_tempTrList.Clear();

		var root	= transform;
		int count	= root.childCount;
		for (int i = 0; i < count; i++)									// 자식 순회
		{
			var tr	= root.GetChild(i);
			if (tr.GetComponent<CanvasRenderer>())						// CanvasRenderer가 있는 경우에만 RectTransform을 얻어온다
			{
				m_tempTrList.Add(tr.GetComponent<RectTransform>());
			}
		}

		m_tempTrList.Sort((a, b) =>										// Z 값에 따라 정렬하기
		{
			var az		= a.anchoredPosition3D.z;
			var bz		= b.anchoredPosition3D.z;
			var diff    = az - bz;
			
			if (diff < Mathf.Epsilon && diff > -Mathf.Epsilon)
				return 0;
			else
				//return diff < 0 ? -1 : 1;
				return diff > 0 ? -1 : 1;
		});

		int listcount   = m_tempTrList.Count;
		for(int i = 0; i < listcount; i++)								// 정렬한 순서에 따라서 자식 순서 맞추기
		{
			m_tempTrList[i].SetAsLastSibling();
		}
	}
}
