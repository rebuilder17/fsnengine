// Solution from : http://answers.unity3d.com/questions/448771/simulate-touch-with-mouse.html
// Input.Touch 오브젝트를 직접 생성할 수 있게 해주는 유틸리티 클래스

using UnityEngine;
using System.Reflection;
using System.Collections.Generic;


public class UnityTouchCreator
{
	static BindingFlags						flag	= BindingFlags.Instance | BindingFlags.NonPublic;
	static Dictionary<string, FieldInfo>	fields;
	object touch;

	public float deltaTime			{ get { return ((Touch)touch).deltaTime;	} set { fields["m_TimeDelta"].SetValue(touch, value); } }
	public int tapCount				{ get { return ((Touch)touch).tapCount;		} set { fields["m_TapCount"].SetValue(touch, value); } }
	public TouchPhase phase			{ get { return ((Touch)touch).phase;		} set { fields["m_Phase"].SetValue(touch, value); } }	
	public Vector2 deltaPosition	{ get { return ((Touch)touch).deltaPosition;} set { fields["m_PositionDelta"].SetValue(touch, value); } }
	public int fingerId				{ get { return ((Touch)touch).fingerId;		} set { fields["m_FingerId"].SetValue(touch, value); } }
	public Vector2 position			{ get { return ((Touch)touch).position;		} set { fields["m_Position"].SetValue(touch, value); } }
	public Vector2 rawPosition		{ get { return ((Touch)touch).rawPosition;	} set { fields["m_RawPosition"].SetValue(touch, value); } }

	public Touch Create()
	{
		return (Touch)touch;
	}

	public UnityTouchCreator()
	{
		touch = new Touch();
	}

	static UnityTouchCreator()
	{
		fields = new Dictionary<string, FieldInfo>();
		foreach (var f in typeof(Touch).GetFields(flag))
		{
			fields.Add(f.Name, f);
			//Debug.Log("name: " + f.Name);
		}
	}
}