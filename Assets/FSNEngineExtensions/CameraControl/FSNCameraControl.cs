using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 부착한 카메라에 대해 스크립트 레벨 컨트롤, 기기의 자이로스코프 연동 움직임
/// </summary>
public class FSNCameraControl : MonoBehaviour
{
	// Properties

	[SerializeField]
	bool            m_useGyroMovement = true;       // 자이로 센서에 따라 카메라가 살짝 이동
	[SerializeField]
	float           m_gyroMovementFactor = 0.5f;   // 자이로 센서에 따라 반응하여 움직이는 최대 거리
	[SerializeField]
	bool            m_useControlMovement = true;    // 외부 컨트롤을 통해 움직일 수 있는지
	[SerializeField]
	Canvas          m_referenceCanvas;				// 카메라 컨트롤 좌표 계산용 캔버스


	// Static Members

	static List<FSNCameraControl>   s_instances = new List<FSNCameraControl>();
	static void RegisterInstance(FSNCameraControl obj)
	{
		int count   = s_instances.Count;
		for (int i = 0; i < count; i++)			// 중복 오브젝트 방지
		{
			if (s_instances[i] == obj)
				return;
		}
		s_instances.Add(obj);					// 오브젝트 리스트에 추가
	}

	static void UnRegisterInstance(FSNCameraControl obj)
	{
		int count   = s_instances.Count;
		for (int i = 0; i < count; i++)         // 오브젝트 제거
		{
			if (s_instances[i] == obj)
			{
				s_instances.RemoveAt(i);
				return;
			}
		}
	}
	
	/// <summary>
	/// 카메라 위치 조정
	/// </summary>
	public static Vector3 controlPosition
	{
		set
		{
			int count   = s_instances.Count;
			for(int i = 0; i < count; i++)
			{
				s_instances[i].m_controlPosition = value;
            }
		}
	}


	// Members

	Transform           m_tr;
	Vector3             m_originalPosition;
	protected Vector3	m_controlPosition;      // 컨트롤로 조종되는 좌표
	RectTransform       m_canvasTr;				// 레퍼런스 캔버스의 트랜스폼

	void Awake()
	{
		if (m_useGyroMovement)
			Input.gyro.enabled  = true;

		m_tr				= transform;
		m_originalPosition  = m_tr.localPosition;
		RegisterInstance(this);

		if (m_useControlMovement)
			m_canvasTr      = m_referenceCanvas.GetComponent<RectTransform>();
    }

	void OnDestroy()
	{
		UnRegisterInstance(this);
	}


	void Update()
	{
		Vector3 finalPosition   = m_originalPosition;

		if (m_useControlMovement)					// 외부 컨트롤 좌표 추가
		{
			finalPosition		+= m_controlPosition  * m_canvasTr.localScale.x;
		}

		if (m_useGyroMovement)						// 자이로 센서 계산 추가
		{
			var rotEuler        = RotateEulerAxisAroundZ(Input.gyro.attitude).eulerAngles;
			Vector3 movement    = new Vector3();
			movement.x          = Mathf.Sin(rotEuler.y / 180.0f * Mathf.PI) * m_gyroMovementFactor;
			movement.y          = Mathf.Sin(-rotEuler.x / 180.0f * Mathf.PI) * m_gyroMovementFactor;
			
			finalPosition       += movement;
		}

		m_tr.localPosition      = finalPosition;
		//_testpos = finalPosition;
    }

	/// <summary>
	/// Euler각에서 Z축 회전을 없애고 x,y 회전축을 Z축 회전각에 따라 회전시킨다.
	/// </summary>
	/// <param name="original"></param>
	/// <returns></returns>
	Quaternion RotateEulerAxisAroundZ(Quaternion original)
	{
		var euler       = original.eulerAngles;
		var zRotate     = Quaternion.AngleAxis(-euler.z, Vector3.forward);
		var rotXAxis    = zRotate * Vector3.right;
		var rotYAxis    = zRotate * Vector3.up;

		return Quaternion.AngleAxis(euler.x, rotXAxis) * Quaternion.AngleAxis(euler.y, rotYAxis);
	}

	//Vector3 _testpos = Vector3.zero;
	//void OnGUI()
	//{
	//	GUI.Label(new Rect(10, 100, 300, 50), _testpos.ToString());
	//}
}
