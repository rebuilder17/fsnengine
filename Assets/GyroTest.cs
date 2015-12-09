using UnityEngine;
using System.Collections;

public class GyroTest : MonoBehaviour
{
	Vector3 m_gyroEuler = new Vector3();


	void Awake()
	{
		Input.gyro.enabled  = true;
	}

	void Start()
	{
		var testEuler   = new Vector3(0, 30, 90);
		
		Debug.Log(RotateEulerAnglesAroundZ(Quaternion.Euler(testEuler)).eulerAngles);
	}

	Quaternion RotateEulerAnglesAroundZ(Quaternion original)
	{
		var euler       = original.eulerAngles;
		var zRotate     = Quaternion.AngleAxis(-euler.z, Vector3.forward);
		var rotXAxis    = zRotate * Vector3.right;
		var rotYAxis    = zRotate * Vector3.up;

		return Quaternion.AngleAxis(euler.x, rotXAxis) * Quaternion.AngleAxis(euler.y, rotYAxis);
	}

	void Update()
	{
		var euler		= RotateEulerAnglesAroundZ(Input.gyro.attitude).eulerAngles;
		m_gyroEuler.x   = euler.y;		// Landscape Left에 맞도록 좌표축 변경
		m_gyroEuler.y   = -euler.x;
		
		var campos      = transform.position;
		campos.x        = Mathf.Sin(m_gyroEuler.x / 180.0f * Mathf.PI) * 2;
		campos.y        = Mathf.Sin(m_gyroEuler.y / 180.0f * Mathf.PI) * 2;
		transform.position = campos;
	}

	void OnGUI()
	{
		GUI.Label(new Rect(10, 10, 400, 300), string.Format("rotation\nX : {0}\nY : {1}\nZ : {2}", m_gyroEuler.x, m_gyroEuler.y, m_gyroEuler.z));
	}
}
