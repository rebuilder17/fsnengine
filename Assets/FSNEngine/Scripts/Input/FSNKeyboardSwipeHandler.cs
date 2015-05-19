using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class FSNKeyboardSwipeHandler : MonoBehaviour
{
	class InputSession
	{
		public const float overTime = 1f;

		public KeyCode key;
		public float timeStart;
		public bool started;

		public bool Start(KeyCode kc)
		{
			if(started) return false;

			key			= kc;
			timeStart	= Time.time;
			started		= true;

			return true;
		}

		public bool CheckOver()
		{
			bool over	= Time.time >= overTime + timeStart;
			if(over)
			{
				Reset();
			}
			return over;
		}

		public void Reset()
		{
			started	= false;
		}

		public float Progress
		{
			get
			{
				if(!started) return 0f;
				return Mathf.Lerp(0, 1, (Time.time - timeStart) / overTime);
			}
		}
	}


	FSNSequenceEngine m_seqEngine;
	InputSession		m_input;

	Dictionary<KeyCode, FSNInGameSetting.FlowDirection> m_keyToFlow	=  new Dictionary<KeyCode, FSNInGameSetting.FlowDirection>();


	void Awake()
	{
		m_keyToFlow[KeyCode.UpArrow]	= FSNInGameSetting.FlowDirection.Up;
		m_keyToFlow[KeyCode.DownArrow]	= FSNInGameSetting.FlowDirection.Down;
		m_keyToFlow[KeyCode.LeftArrow]	= FSNInGameSetting.FlowDirection.Left;
		m_keyToFlow[KeyCode.RightArrow]	= FSNInGameSetting.FlowDirection.Right;

		m_input	= new InputSession();
	}

	void Start()
	{

	}

	void Update()
	{
		var ctrlsys = FSNEngine.Instance.ControlSystem;
		if(!ctrlsys.SwipeBlocked)					// ** 입력 가능한 상태일 경우
		{
			if(m_input.started)						// 입력중인 경우
			{
				var flow	= m_keyToFlow[m_input.key];

				if(!Input.GetKey(m_input.key))		// 키를 뗀 경우 리셋
				{
					m_input.Reset();
					ctrlsys.ClearSwipe();			// 엔진으로 메세지 보내기
				}
				else
				{									// 키를 떼지 않은 경우

					if(m_input.CheckOver())			// 완료된 경우
					{
						ctrlsys.ClearSwipe();		// 엔진으로 메세지 보내기
					}
					else
					{
						ctrlsys.Swipe(flow, m_input.Progress * 2000f);	// 엔진으로 보내기
					}
				}
			}
			else
			{												// 입력중이 아닌 경우
				foreach(KeyCode key in m_keyToFlow.Keys)	// 입력 가능한 방향키 하나씩 체크
				{
					if(Input.GetKeyDown(key))
					{
						if (m_input.Start(key))
						{
							ctrlsys.StartSwipe();			// 엔진으로 메세지 보내기
						}
					}
				}
			}
		}
	}
}
