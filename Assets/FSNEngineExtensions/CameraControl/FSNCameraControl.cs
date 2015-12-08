using UnityEngine;
using System.Collections;

/// <summary>
/// 부착한 카메라에 대해 스크립트 레벨 컨트롤, 기기의 자이로스코프 연동 움직임
/// </summary>
public class FSNCameraControl : MonoBehaviour
{
	// Members

	

	/// <summary>
	/// 스크립트로 컨트롤하는 위치
	/// </summary>
	public Vector3 controlPosition
	{
		get; set;
	}
}
