using UnityEngine;
using System.Collections;

public class QuaternionTest : MonoBehaviour
{
	void Start()
	{
		Input.gyro.enabled  = true;
	}

	Quaternion _rot     = Quaternion.identity;
	void Update()
	{
		var gravityDir	= -Input.gyro.gravity.normalized;		// 아래쪽을 향한 중력 벡터를 반전하기 위해서 - 를 붙인다.
		var gravityRot  = Quaternion.LookRotation(gravityDir);
		transform.localRotation = gravityRot;

		_rot			= gravityRot;
    }

	void OnGUI()
	{
		GUI.Label(new Rect(10, 10, 300, 100), _rot.eulerAngles.ToString());
	}
}
