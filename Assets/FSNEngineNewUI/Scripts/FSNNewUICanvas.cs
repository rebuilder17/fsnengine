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

	bool			m_awake	= false;



	public void CheckAndDoInit()
	{
		Awake();
	}


	void Awake()
	{
		if (m_awake)
			return;
		m_awake	= true;

		var screenSize		= FSNEngine.Instance.ScreenDimension;

		m_canvas			= gameObject.GetComponent<Canvas>() ?? gameObject.AddComponent<Canvas>();	// Canvas 생성, 세팅
		m_canvas.renderMode	= RenderMode.WorldSpace;							// World space에 둔다.

		m_rectTrans			= gameObject.GetComponent<RectTransform>();			// Size, 좌표계 세팅. 가운데를 중점으로 한다.
		m_rectTrans.pivot	= new Vector2(0.5f, 0.5f);
		m_rectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, screenSize.x);
		m_rectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, screenSize.y);


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

			var screensize	= FSNEngine.Instance.ScreenDimension;

			// 0. canvas의 점을 camera의 좌표계로 변환
			var toCamera	= m_rectTrans.localToWorldMatrix * m_camera.transform.worldToLocalMatrix;
			var canvasP0	= toCamera.MultiplyPoint(Vector3.zero);
			var canvasP1	= toCamera.MultiplyPoint(Vector3.up * screensize.y);
			var canvasP2	= toCamera.MultiplyPoint(Vector3.right * screensize.x);

			// 1. canvas가 만들어내는 평면과 카메라의 방향 벡터가 만나는 점을 구한다.
			var canvPlane	= new Plane(canvasP0, canvasP1, canvasP2);
			float enter;
			canvPlane.Raycast(new Ray(Vector3.zero, Vector3.forward), out enter);
			var intersectP	= Vector3.forward * enter;

			// 2. 그 만나는 점을 포함하고 카메라의 방향 벡터와는 수직한 평면(= 카메라와 마주보는)을 가정하고,
			var paralPlane	= new Plane(Vector3.back, intersectP);

			// 3. 이 평면에서의 좌표계가 카메라에 투영될 때의 좌표가 (엔진에서의) 화면 좌표계와 일치하도록 canvas의 스케일을 조절한다.
			var screenRay		= m_camera.ScreenPointToRay(new Vector3(m_camera.pixelWidth / 2, m_camera.pixelHeight));
			screenRay.origin	= m_camera.transform.InverseTransformPoint(screenRay.origin);		// ray를 카메라 로컬 스페이스로 옮겨오기 (world 기준으로 ray가 생성되었으므로)
			screenRay.direction	= m_camera.transform.InverseTransformDirection(screenRay.direction);// 방향도...

			float raydist;
			paralPlane.Raycast(screenRay, out raydist);												// 모든 것이 카메라 로컬 스페이스에 있으므로 안심하고 레이캐스팅

			float distFromScreenSqr	= (screenRay.GetPoint(raydist)).sqrMagnitude;
			//float distFromScreenSqr	= Mathf.Pow(raydist, 2);

			float planeHalfHeight	= Mathf.Sqrt(distFromScreenSqr - Mathf.Pow(paralPlane.distance, 2));	// 가상 plane의 높이/2 를 구한다
			float scale				= planeHalfHeight / (screensize.y / 2f);
			m_rectTrans.localScale	= Vector3.one * scale;
		}
	}
}
