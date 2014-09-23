using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SnapshotEngineTest : MonoBehaviour
{
	class InputSession
	{
		public const float overTime = 0.3f;

		public KeyCode key;
		public float timeStart;
		public bool started;

		public void Start(KeyCode kc)
		{
			if(started) return;

			key			= kc;
			timeStart	= Time.time;
			started		= true;
		}

		public bool CheckOver()
		{
			bool over	= Time.time >= overTime + timeStart;
			if(over)
			{
				started	= false;
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
		m_seqEngine	= GetComponent<FSNSequenceEngine>();
		//

		// 테스트 1.
		//m_seqEngine.StartSnapshotSequence(FSNSnapshotSequence.GenerateTestSequence());


		// 테스트 2.
		FSNSequence	rawSeq	= FSNSequence.GenerateTestSequence();
		var sshotSeq		= FSNSnapshotSequence.Builder.BuildSnapshotSequence(rawSeq);

		m_seqEngine.StartSnapshotSequence(sshotSeq);

		// 세팅 체인 테스트

		FSNInGameSetting settingbase	= new FSNInGameSetting();
		FSNInGameSetting.Chain settingover	= new FSNInGameSetting.Chain(settingbase);

		settingbase.FontSize	= 50;
		settingover.FontSize	= 100;

		settingbase.SwipeWeight	= 0.8f;

		FSNInGameSetting frozen			= settingover.Freeze();

		Debug.Log("Chain font size : " + frozen.FontSize);
		Debug.Log("Chain SwipeWeight : " + frozen.SwipeWeight);
	}

	void Update()
	{
		if(m_seqEngine.CanSwipe)					// ** 입력 가능한 상태일 경우
		{
			if(m_input.started)						// 입력중인 경우
			{
				var flow	= m_keyToFlow[m_input.key];

				if(!Input.GetKey(m_input.key))		// 키를 뗀 경우 리셋
				{
					m_input.Reset();
					m_seqEngine.PartialSwipe(flow, 0f);
				}
				else
				{									// 키를 떼지 않은 경우

					if(m_input.CheckOver())			// 완료된 경우
					{
						m_seqEngine.FullSwipe(flow);
					}
					else
					{
						m_seqEngine.PartialSwipe(flow, m_input.Progress);
					}
				}
			}
			else
			{												// 입력중이 아닌 경우
				foreach(KeyCode key in m_keyToFlow.Keys)	// 입력 가능한 방향키 하나씩 체크
				{
					if(Input.GetKeyDown(key))
					{
						m_input.Start(key);
					}
				}
			}
		}
	}
}
