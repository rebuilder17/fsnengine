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
			movement.x          = CalcGyroTangentDiff(rotEuler.y) * m_gyroMovementFactor;	// Landscape Left 에 맞춰서 x, y 를 넣는다.
			movement.y          = CalcGyroTangentDiff(-rotEuler.x) * m_gyroMovementFactor;
			
			finalPosition       += movement;
		}

		m_tr.localPosition      = finalPosition;
		//_testpos = finalPosition;
    }

	static Quaternion NormalizeQuaternion(Quaternion q)
	{
		Quaternion result;
		float sq = q.x * q.x;
		sq += q.y * q.y;
		sq += q.z * q.z;
		sq += q.w * q.w;
		//detect badness
		//assert(sq > 0.1f);
		float inv = 1.0f / Mathf.Sqrt(sq);
		result.x = q.x * inv;
		result.y = q.y * inv;
		result.z = q.z * inv;
		result.w = q.w * inv;
		return result;
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
		//var rotXAxis    = zRotate * Vector3.right;
		//var rotYAxis    = zRotate * Vector3.up;
		var rotXAxis    = new Vector3(Mathf.Cos(-euler.z / 180f * Mathf.PI), Mathf.Sin(-euler.z / 180f * Mathf.PI)).normalized;
		var rotYAxis    = new Vector3(-rotXAxis.y, rotXAxis.x);

		//return Quaternion.AngleAxis(euler.x, rotXAxis) * Quaternion.AngleAxis(euler.y, rotYAxis);
		var result		= NormalizeQuaternion(Quaternion.AngleAxis(euler.x, rotXAxis)) * NormalizeQuaternion(Quaternion.AngleAxis(euler.y, rotYAxis));
		_xaxis          = rotXAxis * 100;
		_yaxis          = rotYAxis * 100;
		_zrot           = zRotate.eulerAngles;
		//_testpos        = result.eulerAngles;
		_testpos        = euler;
        return NormalizeQuaternion(result);
	}

	/// <summary>
	/// 각도를 탄젠트를 사용하여 일정 범위 안에 넣는다.
	/// </summary>
	/// <param name="angle"></param>
	/// <returns></returns>
	float CalcGyroTangentDiff(float angle)
	{
		while (angle < 0)
			angle += 360.0f;
		while (angle >= 360.0f)
			angle -= 360.0f;

		if (angle >= 90 && angle <= 270)
		{
			angle = -(angle - 180.0f);
		}
		else if (angle >= 270)
		{
			angle = angle - 360;
		}

		var angleLimit    = 60.0f;
		return Mathf.Tan(Mathf.Clamp(angle, -angleLimit, angleLimit) / 180.0f * Mathf.PI);
	}

	Vector3 _testpos = Vector3.zero;
	Vector3 _xaxis  = Vector3.zero;
	Vector3 _yaxis  = Vector3.zero;
	Vector3 _zrot   = Vector3.zero;
	void OnGUI()
	{
		GUI.Label(new Rect(10, 100, 300, 50), _testpos.ToString());
		GUI.Label(new Rect(10, 120, 300, 50), "x axis : " + _xaxis.ToString() + "\ny axis : " + _yaxis.ToString() + "\nz rotation : " + _zrot.ToString());
	}
}
