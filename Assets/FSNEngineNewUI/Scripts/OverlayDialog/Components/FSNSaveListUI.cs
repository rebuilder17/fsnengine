using UnityEngine;
using System.Collections;

/// <summary>
/// 세이브 파일 UI 리스트
/// </summary>
public class FSNSaveListUI : MonoBehaviour
{
	// Properties

	[SerializeField]
	FSNSaveListUIItem	m_newSaveItem;			// NEW 세이브 항목 (필요하면 active해서 사용함)
	[SerializeField]
	FSNSaveListUIItem	m_itemOriginal;			// 일반 세이브 항목 (비활성화로 두고 복제해서 사용)


}
