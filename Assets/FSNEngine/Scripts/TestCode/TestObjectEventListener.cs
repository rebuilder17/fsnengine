using UnityEngine;
using System.Collections;

public class TestObjectEventListener : FSNBaseGameObjectEventListener
{
	MeshRenderer m_renderer;

	void Awake()
	{
		m_renderer	= GetComponent<MeshRenderer>();
	}

	public override void OnUpdateColor(Color color)
	{
		m_renderer.material.color	= color;
	}

	void Update()
	{
		transform.Rotate(Random.rotationUniform.eulerAngles * Time.deltaTime);
	}
}