using UnityEngine;
using System.Collections;

public class GyroTest : MonoBehaviour
{
	void Awake()
	{
		Input.gyro.enabled  = true;
	}

	void Update()
	{
		transform.rotation = Quaternion.Euler(Input.gyro.attitude.eulerAngles * 0.3f);
	}
}
