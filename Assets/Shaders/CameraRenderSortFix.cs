using UnityEngine;
using System.Collections;

/// <summary>
/// 투명 오브젝트들을 카메라 평면에 수직한 축 순서대로 정렬한다.
/// </summary>
public class CameraRenderSortFix : MonoBehaviour
{
	void OnEnable()
	{
		GetComponent<Camera>().transparencySortMode = TransparencySortMode.Orthographic;
	}
}
