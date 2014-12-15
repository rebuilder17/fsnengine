using UnityEngine;
using System.Collections;

public class UnityGUIMatrixFix : MonoBehaviour
{
	[SerializeField]
	float			m_desiredHeight	= 720;


	void OnGUI()
	{
		float scale	= (float)Screen.height / m_desiredHeight;
		GUI.matrix	= Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1));
	}
}
