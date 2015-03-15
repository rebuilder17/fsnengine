using UnityEngine;
using System.Collections;
using UnityEngine.UI;


/// <summary>
/// Canvas와 Camera 등등을 초기화해주고 관리하는 오브젝트. NewUI 계열 모듈에서는 이 컴포넌트에 접근하는 것을 기본으로 한다.
/// </summary>
[RequireComponent(typeof(Canvas))]
public class FSNNewUICanvas : MonoBehaviour
{
	// Properties

	[SerializeField]
	Camera			m_camera;				// 이 캔버스를 표시하는데 사용할 카메라. Ortho/Perspective 양쪽 다 대응 가능


	// Members
	Canvas			m_canvas;				// UI 캔버스
	RectTransform	m_rectTrans;


	void Awake()
	{
		var screenSize		= FSNEngine.Instance.ScreenDimension;

		m_canvas			= gameObject.GetComponent<Canvas>() ?? gameObject.AddComponent<Canvas>();	// Canvas 생성, 세팅
		m_canvas.renderMode	= RenderMode.WorldSpace;							// World space에 둔다.

		m_rectTrans			= gameObject.GetComponent<RectTransform>();			// Size, 좌표계 세팅. 가운데를 중점으로 한다.
		m_rectTrans.pivot	= new Vector2(0.5f, 0.5f);
		m_rectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, screenSize.x);
		m_rectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, screenSize.y);
	}
	
	void Start ()
	{
		if((m_camera.cullingMask | 1 << gameObject.layer) == 0)					// 컬링 마스크 체크 - 이 레이어가 카메라에 표시되지 않는다면 경고
		{
			Debug.LogWarning("[FSNNewUICanvas] This UI canvas cannot shown by the camera - the camera's layer mask doesn't include this object's layer.");
		}

		if(m_camera.orthographic)												// Orthogonal일 시 카메라 세팅
		{
			m_camera.orthographicSize	= FSNEngine.Instance.ScreenYSize / 2f;
		}
		else
		{																		// Perspective일 시 카메라 세팅

			// 0. canvas의 점을 camera의 좌표계로 변환

			// 1. canvas가 만들어내는 평면과 카메라의 방향 벡터가 만나는 점을 구한다.

			// 2. 그 만나는 점을 포함하고 카메라의 방향 벡터와는 수직한 평면을 가정하고,

			// 3. 이 평면에서의 좌표계가 카메라에 투영될 때의 좌표가 (엔진에서의) 화면 좌표계와 일치하도록 canvas의 스케일을 조절한다.
		}
	}
}
