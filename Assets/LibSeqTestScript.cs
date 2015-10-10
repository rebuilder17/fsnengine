using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using LibSequentia.Engine;
using LibSequentia.Data;

public class LibSeqTestScript : MonoBehaviour
{
	float							m_tension	= 0;


	Track []						m_tracks = new Track[2];
	TransitionScenario				m_tscen;
	
	void Start()
	{
		Application.targetFrameRate	= -1;

		// TEST : 버퍼 사이즈를 조절해본다.
		var audioSettings			= AudioSettings.GetConfiguration();
		Debug.Log("original buffer size is : " + audioSettings.dspBufferSize);
		//audioSettings.dspBufferSize	= 2048;
		//AudioSettings.Reset(audioSettings);
		//

		m_tracks[0]			= LibSequentiaMain.instance.LoadTrack("libsequentia/data/track1");
		m_tracks[1]			= LibSequentiaMain.instance.LoadTrack("libsequentia/data/track2");

		m_tscen				= LibSequentiaMain.instance.LoadTransitionScenario("libsequentia/data/ts_simpledj");

		LibSequentiaMain.instance.tension	= 1;



		var track1	= m_tracks[0];
		var track2	= m_tracks[1];

		m_stateSeq.Add(new StepState() { curtrack = track1, step = 1 });
		m_stateSeq.Add(new StepState() { curtrack = track1, step = 2 });
		m_stateSeq.Add(new StepState() { curtrack = track1, step = 3 });
		m_stateSeq.Add(new StepState() { curtrack = track1, step = 4 });
		m_stateSeq.Add(new StepState() { curtrack = track1, step = 5, newtrack = track2, newstep = 1, tscen = m_tscen });
		m_stateSeq.Add(new StepState() { curtrack = track1, step = 6, newtrack = track2, newstep = 2, tscen = m_tscen });
		m_stateSeq.Add(new StepState() { curtrack = track1, step = 7, newtrack = track2, newstep = 3, tscen = m_tscen });
		m_stateSeq.Add(new StepState() { curtrack = track2, step = 4 });
		m_stateSeq.Add(new StepState() { curtrack = track2, step = 5 });
		m_stateSeq.Add(new StepState() { curtrack = track2, step = 6 });
		m_stateSeq.Add(new StepState() { curtrack = track2, step = 7 });
	}


	// Step 이동 테스트
	struct StepState
	{
		public Track	curtrack;
		public int		step;

		public Track	newtrack;
		public int		newstep;

		public TransitionScenario tscen;
	}

	List<StepState>	m_stateSeq	= new List<StepState>();
	StepState		m_prevState	= new StepState();
	int				m_stateidx	= -1;
	bool			m_firstrun	= true;

	bool			m_newTrackReverse;

	void NextStep()
	{
		if (m_firstrun)
			m_firstrun	= false;

		if (m_stateidx < m_stateSeq.Count)
		{
			m_stateidx++;
		}

		var ctrl	= LibSequentiaMain.instance.stepControl;

		if (m_stateidx == 0)
		{
			var first	= m_stateSeq[0];
			ctrl.StartWithOneTrack(first.curtrack, 1, false);

			m_prevState	= first;
		}
		else if (m_stateidx >= m_stateSeq.Count)
		{
			ctrl.StepMove(m_stateSeq[m_stateSeq.Count - 1].step + 1, -1, false);
		}
		else
		{
			var cur		= m_stateSeq[m_stateidx];
			if (cur.newtrack != null && m_prevState.newtrack == null)
			{
				m_newTrackReverse	= false;
				ctrl.StepMove(cur.step, cur.newtrack, cur.tscen, cur.newstep, false);
			}
			else if (m_newTrackReverse && m_prevState.newstep == 1)
			{
				// 특수 케이스 처리 : 이전에 역방향으로 다음 트랙으로 넘어가는 자연 진행을 건 경우.
				// 새 트랙을 다시 올려줘야한다.
				m_newTrackReverse	= false;
				ctrl.StepMove(cur.step, cur.newtrack, cur.tscen, cur.newstep, false);
			}
			else if (cur.newtrack != null && m_prevState.newtrack != null)
			{
				if (m_newTrackReverse)
				{
					ctrl.StepMove(cur.newstep, cur.newtrack, cur.tscen, cur.step, false);
				}
				else
				{
					ctrl.StepMove(cur.step, cur.newtrack, cur.tscen, cur.newstep, false);
				}
			}
	
			else
			{
				ctrl.StepMove(cur.step, cur.newstep, false);
			}

			m_prevState	= cur;
		}

		Debug.LogWarning("NextStep - " + m_stateidx);
	}

	void PrevStep()
	{
		if (m_firstrun)
		{
			m_stateidx	= m_stateSeq.Count;
			m_firstrun	= false;
		}

		if (m_stateidx > -1)
		{
			m_stateidx--;
		}

		var ctrl	= LibSequentiaMain.instance.stepControl;

		if (m_stateidx >= m_stateSeq.Count - 1)
		{
			var last	= m_stateSeq[m_stateSeq.Count - 1];
			ctrl.StartWithOneTrack(last.curtrack, last.step, true);

			m_prevState	= last;
		}
		else if (m_stateidx < 0)
		{
			ctrl.StepMove(0, -1, true);
		}
		else
		{
			var cur		= m_stateSeq[m_stateidx];
			if (cur.newtrack != null && m_prevState.newtrack == null)
			{
				m_newTrackReverse	= true;
				ctrl.StepMove(cur.newstep, cur.curtrack, cur.tscen, cur.step, true);
			}
			else if (!m_newTrackReverse && m_prevState.newstep == 3)
			{
				// 특수 케이스 처리 : 이전에 역방향으로 다음 트랙으로 넘어가는 자연 진행을 건 경우.
				// 새 트랙을 다시 올려줘야한다.
				m_newTrackReverse	= true;
				ctrl.StepMove(cur.newstep, cur.curtrack, cur.tscen, cur.step, true);
			}
			else if (cur.newtrack != null && m_prevState.newtrack != null)
			{
				if (m_newTrackReverse)
				{
					ctrl.StepMove(cur.newstep, cur.curtrack, cur.tscen, cur.step, true);
				}
				else
				{
					ctrl.StepMove(cur.step, cur.curtrack, cur.tscen, cur.newstep, true);
				}
			}
	
			else
			{
				ctrl.StepMove(cur.step, -1, true);
			}

			m_prevState	= cur;
		}

		Debug.LogWarning("PrevStep - " + m_stateidx);
	}

	

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Quote))
		{
			NextStep();
		}

		if (Input.GetKeyDown(KeyCode.Semicolon))
		{
			PrevStep();
		}


		/*
		if (Input.GetKeyDown(KeyCode.N))			// 자연 진행
		{
			Debug.Log("Natural Transition");
			m_masterplayer.DoNaturalProgress();
		}
		
		if (Input.GetKeyDown(KeyCode.M))			// 강제 진행
		{
			Debug.Log("Manual Transition");
			m_masterplayer.DoManualProgress();
		}


		if (Input.GetKeyDown(KeyCode.C))			// C 키 : 다음 트랙 준비
		{
			Debug.Log("newtrack");
			m_trackIdx = (m_trackIdx + 1) % 2;
			m_masterplayer.SetNewTrack(m_tracks[m_trackIdx], m_tscen);
		}
		 */

		
		if(Input.GetKeyDown(KeyCode.Equals))		// '+' 키 (텐션 업)
		{
			m_tension	= Mathf.Min(1f, m_tension + 0.1f);
			LibSequentiaMain.instance.tension	= m_tension;
		}

		if (Input.GetKeyDown(KeyCode.Minus))		// '-' 키 (텐션 다운)
		{
			m_tension	= Mathf.Max(0f, m_tension - 0.1f);
			LibSequentiaMain.instance.tension	= m_tension;
		}

		if (Input.GetKeyDown(KeyCode.Comma))		// < 키 (이전 트랙쪽으로 트랜지션 옮기기)
		{
			LibSequentiaMain.instance.songTransition	= Mathf.Max(0, LibSequentiaMain.instance.songTransition - 0.1f);
		}

		if (Input.GetKeyDown(KeyCode.Period))		// > 키 (다음 트랙쪽으로 트랜지션 옮기기)
		{
			LibSequentiaMain.instance.songTransition	= Mathf.Min(1, LibSequentiaMain.instance.songTransition + 0.1f);
		}

		// 페이드아웃
		if (Input.GetKeyDown(KeyCode.E))
		{
			LibSequentiaMain.instance.stepControl.ForceOut();
			m_firstrun	= true;
			m_stateidx	= -1;
		}

		// 강제 리셋
		if(Input.GetKeyDown(KeyCode.R))
		{
			LibSequentiaMain.instance.Reset();
			m_firstrun	= true;
			m_stateidx	= -1;
		}

		if (m_firstrun)
		{
			int idx	= -1;

			if (Input.GetKeyDown(KeyCode.Alpha1))
				idx	= 0;
			if (Input.GetKeyDown(KeyCode.Alpha2))
				idx = 1;
			if (Input.GetKeyDown(KeyCode.Alpha3))
				idx = 2;
			if (Input.GetKeyDown(KeyCode.Alpha4))
				idx = 3;
			if (Input.GetKeyDown(KeyCode.Alpha5))
				idx = 4;
			if (Input.GetKeyDown(KeyCode.Alpha6))
				idx = 5;
			if (Input.GetKeyDown(KeyCode.Alpha7))
				idx = 6;
			if (Input.GetKeyDown(KeyCode.Alpha8))
				idx = 7;
			if (Input.GetKeyDown(KeyCode.Alpha9))
				idx = 8;
			if (Input.GetKeyDown(KeyCode.Alpha0))
				idx = 9;

			if (idx != -1)
			{
				m_stateidx	= idx;
				var state	= m_stateSeq[idx];
				LibSequentiaMain.instance.stepControl.StartWithTwoTrack(state.curtrack, state.step, state.newtrack, state.newstep, state.tscen);
				
				m_prevState	= state;
				m_firstrun	= false;
			}
		}
	}

	void OnGUI()
	{
		/*
		var buttonrect	= new Rect() { x = 0, y = 0, width = 200, height = 100 };
		if (GUI.Button(buttonrect, "Natural"))			// 자연 진행
		{
			Debug.Log("Natural Transition");
			m_masterplayer.DoNaturalProgress();
		}

		buttonrect.x	= 200;
		if (GUI.Button(buttonrect, "Manual"))			// 강제 진행
		{
			Debug.Log("Manual Transition");
			m_masterplayer.DoManualProgress();
		}

		buttonrect.x	= 0;
		buttonrect.y	= 100;
		if (GUI.Button(buttonrect, "tension+"))		// '+' 키 (텐션 업)
		{
			m_tension	= Mathf.Min(1f, m_tension + 0.1f);
			m_masterplayer.tension	= m_tension;
		}

		buttonrect.x	= 200;
		if (GUI.Button(buttonrect, "tension-"))		// '-' 키 (텐션 다운)
		{
			m_tension	= Mathf.Max(0f, m_tension - 0.1f);
			m_masterplayer.tension	= m_tension;
		}

		buttonrect.x	= 0;
		buttonrect.y	= 200;
		if (GUI.Button(buttonrect, "NextTrack"))			// C 키 : 다음 트랙 준비
		{
			Debug.Log("newtrack");
			m_trackIdx = (m_trackIdx + 1) % 2;
			m_masterplayer.SetNewTrack(m_tracks[m_trackIdx], m_tscen);
		}

		buttonrect.x	= 0;
		buttonrect.y	= 300;
		if (GUI.Button(buttonrect, "<<"))		// < 키 (이전 트랙쪽으로 트랜지션 옮기기)
		{
			m_masterplayer.transition	= Mathf.Max(0, m_masterplayer.transition - 0.1f);
		}

		buttonrect.x	= 200;
		if (GUI.Button(buttonrect, ">>"))		// > 키 (다음 트랙쪽으로 트랜지션 옮기기)
		{
			m_masterplayer.transition	= Mathf.Min(1, m_masterplayer.transition + 0.1f);
		}
		 */
	}
}
