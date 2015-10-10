using UnityEngine;
using System.Collections;

namespace LibSequentia.Engine
{
	/// <summary>
	/// 실제 플레이어 컴포넌트용 인터페이스
	/// </summary>
	public interface IPlayerComponent
	{
		void SetAudioClip(int layerIndex, Data.IAudioClipHandle handle);
		void ClearAudioClip(int layerIndex);
		Data.IAutomationControl GetAutomationTarget(int layerIndex);

		double fullAudioLength { get; }

		void PlayScheduled(double dsptime, double offset = 0);
		void StopImmediately();

		bool readyToPlay { get; }
	}


	/// <summary>
	/// 섹션 재생을 관리
	/// </summary>
	public class SectionPlayer
	{
		/// <summary>
		/// 플레이어 상태
		/// </summary>
		enum State
		{
			BeforePlay,			// 실제 음원 재생 전
			BeforeFillIn,		// Fill-in 이전
			FillIn,				// Fill-in
			Looping,			// Section 루프중
			AfterLoop,			// 루프 이후
			//End,				// 음원 종료
		}

		/// <summary>
		/// section간 전환 타입
		/// </summary>
		public enum TransitionType
		{
			Natural	= 0,		// 자연 전환
			Manual,				// 강제 전환

			Instant,			// 즉시 전환. 오로지 StartSection에서만 사용 가능

			None	= -1,		// 지정되지 않음
		}


		// Constants

		const int		c_playerComponentsCount	= 2;



		// Members

		Data.Section	m_sectionData;					// 재생하려는 섹션 정보
		//double			m_sectionAudioLength;			// 총 섹션 음원 길이 (초)

		BeatSyncClock	m_clock;						// 박자에 맞는 시간 계산을 할 용도
		State			m_state;						// 플레이어 상태
		//double			m_timePlayStart;				// 음원 재생을 시작한 dsptime
		double			m_loopLengthDspTime;			// 루프 길이

		/// <summary>
		/// 다음번에 올 루프 끝부분 dspTime
		/// </summary>
		public double nextLoopEndDspTime { get; private set; }
		/// <summary>
		/// 첫 루프가 시작되는 타이밍
		/// </summary>
		public double firstLoopStartDspTime { get; private set; }


		TransitionType	m_startTransition;				// 시작시 Transition 타입
		TransitionType	m_endTransition	= TransitionType.None;	// 끝날 시 Transition 타입 (None일 때는 끝내지 않고 루프)
		double			m_endTransitionLength;			// 전환 시간
		double			m_properNextSectionStartTime;	// 전환시 다음 섹션이 시작하면 되는 시간

		bool			m_loopReserved;
		bool			m_suppressLooping;				// 새 루프를 시작하지 못하게 함


		IPlayerComponent []	m_playerComponents	= new IPlayerComponent[c_playerComponentsCount];	// SectionPlayer에 연동된 실제 오디오 플레이어 컴포넌트
		int					m_playerComponentIndex	= 0;											// 현재 사용중인 player 컴포넌트 인덱스
		IPlayerComponent currentPlayerComponent
		{
			get
			{
				return m_playerComponents[m_playerComponentIndex];
			}
		}
		/// <summary>
		/// 다른 인덱스의 playerComponent 참조하게 설정
		/// </summary>
		public void SwitchPlayerComponent()
		{
			m_playerComponentIndex = (m_playerComponentIndex+1) % c_playerComponentsCount;
		}

		Data.IAutomationControl m_transitionAutomationTarget;	// 섹션 전환시 오토메이션 타겟
		Data.IAutomationControl [] m_tensionAutomationTargets;


		public float		m_tension	= 0;

		/// <summary>
		/// 긴장도.
		/// </summary>
		public float tension
		{
			get { return m_tension; }
			set
			{
				m_tension	= value;
				if (m_sectionData != null)
				{
					m_sectionData.tension	= m_tension;
				}
			}
		}



		/// <summary>
		/// 실제 재생 전이거나 루프가 끝난 경우 true
		/// </summary>
		public bool isReadyOrFinished
		{
			get
			{
				return m_state == State.BeforePlay || m_state == State.AfterLoop;
			}
		}

		public TransitionType currentEndTransition
		{
			get
			{
				return m_endTransition;
			}
		}

		/// <summary>
		/// 실제 트랜지션 중인지
		/// </summary>
		public bool isOnTransition { get; private set; }



		MonoBehaviour	m_context;						// MonoBehaviour
		Coroutine		m_updateCo;						// 현재의 update 코루틴
		Coroutine		m_transitionCo;					// 트랜지션 업데이트 코루틴



		public SectionPlayer(MonoBehaviour context)
		{
			m_context = context;
		}

		public void SetPlayerComponents(IPlayerComponent p1, IPlayerComponent p2)
		{
			m_playerComponents[0] = p1;
			m_playerComponents[1] = p2;
		}

		public void SetTransitionAutomationTarget(Data.IAutomationControl ctrl)
		{
			m_transitionAutomationTarget	= ctrl;
		}

		public void SetTensionAutomationTargets(params Data.IAutomationControl [] ctrls)
		{
			m_tensionAutomationTargets	= ctrls;
		}


		/// <summary>
		/// 현재 section의 정보 (layer 등등)을 현재 플레이어에 세팅
		/// </summary>
		void SetSectionToCurrentPlayer()
		{
			var curplayer = currentPlayerComponent;
			for (int i = 0; i < Data.Section.c_maxLayerPerSection; i++)	// 각 Layer 정보 세팅
			{
				var layer	= m_sectionData.GetLayer(i);
				if (layer != null)
				{
					curplayer.SetAudioClip(i, layer.clipHandle);
				}
				else
				{
					curplayer.ClearAudioClip(i);
				}
			}
		}

		/// <summary>
		/// Section 재생 준비
		/// </summary>
		public void StartSection(Data.Section section, BeatSyncClock clock, TransitionType ttype, double startTime = 0)
		{
			EndSection();												// 이전에 재생중이었을 수도 있는 섹션을 강제 종료


			// 초기화

			m_sectionData		= section;
			m_clock				= clock;

			m_startTransition	= ttype;
			m_endTransition		= TransitionType.None;
			isOnTransition		= false;
			

			// 텐션 관련 설정
			for (int i = 0; i < Data.Section.c_maxLayerPerSection; i++)
			{
				var layer		= section.GetLayer(i);
				if (layer != null)
				{
					layer.SetAutomationTarget(m_tensionAutomationTargets[i]);
				}
			}
			section.tension		= m_tension;


			if (startTime == 0)											// 시작 시간이 지정되지 않은 경우 다음 안전한 비트를 시작 시간으로
			{
				startTime		= m_clock.CalcNextSafeBeatTime();
			}
			
			m_updateCo	= m_context.StartCoroutine(UpdateStateCo(startTime));	// 코루틴 시작
		}

		/// <summary>
		/// 업데이트 코루틴
		/// </summary>
		/// <returns></returns>
		IEnumerator UpdateStateCo(double startTime)
		{
			var curplayer	= currentPlayerComponent;
			var beatTime	= m_clock.SecondPerBeat;


			// *** 재생하기 전

			m_state				= State.BeforePlay;

			SetSectionToCurrentPlayer();								// 각 layer정보 세팅
			//m_sectionAudioLength	= curplayer.fullAudioLength;		// 전체 오디오 길이 구하기
			while (!curplayer.readyToPlay)								// 로딩 대기
				yield return null;

			// 각종 시간값 계산
			var timePlay		= startTime;
			var timeFillIn		= timePlay + beatTime * m_sectionData.beatFillIn;
			var timeLoopStart	= timePlay + beatTime * m_sectionData.beatStart;
			m_loopLengthDspTime	= beatTime * m_sectionData.beatLoopLength;
			var timeNextLoopEnd	= timeLoopStart + m_loopLengthDspTime;


			// 현재 재생 종류, section 세팅값을 보고 음원의 어느 지점부터 재생할지 정한다.
			double offset		= 0;
			if (m_startTransition == TransitionType.Manual)		// 강제 전환 시작시에는 fill-in 앞부분은 스킵한다.
			{
				offset			= timeFillIn - timePlay;
				
				timeFillIn		-= offset;						// 시간값들 조정
				timeLoopStart	-= offset;
				timeNextLoopEnd	-= offset;
			}
			else if (m_startTransition == TransitionType.Instant)
			{
				offset			= timeLoopStart - timePlay;

				timeFillIn		-= offset;						// 시간값들 조정
				timeLoopStart	-= offset;
				timeNextLoopEnd	-= offset;
			}
			//Debug.Log("PlayScheduled time : " + timePlay + ", offset : " + offset);
			curplayer.PlayScheduled(timePlay, offset);			// 다음번 비트에 음원 재생


			// 필요한 시간값은 멤버로 보관
			//m_timePlayStart		= timePlay;
			firstLoopStartDspTime	= timeLoopStart;
			nextLoopEndDspTime		= timeNextLoopEnd;
			
			// 전환 오토메이션을 미리 앞부분으로 적용해둔다.
			var auto	= GetAutomation(m_startTransition == TransitionType.Manual? m_sectionData.inTypeManual : m_sectionData.inTypeNatural);
			if (auto != null)
			{
				m_transitionAutomationTarget.Set(auto.targetParam, auto.GetValue(0));
			}



			while (AudioSettings.dspTime < timePlay)			// 재생될 때까지 대기
				yield return null;


			// *** 재생 후, Fill-in 이전

			m_state			= State.BeforeFillIn;
			//Debug.Log(m_state.ToString() + " ... dspTime : " + AudioSettings.dspTime);

			if (m_startTransition == TransitionType.Natural)	// 자연 전환일 경우 여기에서 트랜지션
			{
				while (AudioSettings.dspTime < timeFillIn)
				{
					if (auto != null)
					{
						var t	= (AudioSettings.dspTime - timePlay) / (timeLoopStart - timePlay);
						m_transitionAutomationTarget.Set(auto.targetParam, auto.GetValue((float)t));
						//Debug.LogFormat("t = {0}, value = {1}", t, auto.GetValue((float)t));
					}
					yield return null;
				}
			}
			else
			{
				while (AudioSettings.dspTime < timeFillIn)		// Fill-in 지점까지 대기
					yield return null;
			}
			

			// *** Fill - in

			m_state				= State.FillIn;
			//Debug.Log(m_state.ToString() + " ... dspTime : " + AudioSettings.dspTime);

			
			if (m_startTransition == TransitionType.Natural)	// 자연 전환일 경우, 루프 시작까지 남은 부분에 대해서 전환
			{
				while (AudioSettings.dspTime < timeLoopStart)
				{
					if (auto != null)
					{
						var t	= (AudioSettings.dspTime - timePlay) / (timeLoopStart - timePlay);
						m_transitionAutomationTarget.Set(auto.targetParam, auto.GetValue((float)t));
						//Debug.LogFormat("t = {0}, value = {1}", t, auto.GetValue((float)t));
					}
					yield return null;
				}
				m_transitionAutomationTarget.Set(auto.targetParam, auto.GetValue(1));	// 마지막 값으로 보정
			}
			else
			{													// 강제 전환
				while (AudioSettings.dspTime < timeLoopStart)
				{
					if (auto != null)
					{
						var t	= (AudioSettings.dspTime - timeFillIn) / (timeLoopStart - timeFillIn);
						m_transitionAutomationTarget.Set(auto.targetParam, auto.GetValue((float)t));
						//Debug.LogFormat("t = {0}, value = {1}", t, auto.GetValue((float)t));
					}
					yield return null;
				}
				m_transitionAutomationTarget.Set(auto.targetParam, auto.GetValue(1));	// 마지막 값으로 보정
			}


			// *** Looping

			m_state				= State.Looping;
			m_loopReserved		= false;								// 루핑 예약을 했는지 여부
			m_suppressLooping	= false;								// 루프 억제 여부
			while(true)													// fadeout 설정이 되지 않는다면 계속 루프한다.
			{
				var nextSafeBeat	= m_clock.CalcNextSafeBeatTime();
				if (System.Math.Abs(nextSafeBeat - nextLoopEndDspTime) < beatTime
												&& !m_loopReserved && !m_suppressLooping)	// 다음번 비트에 루프를 해야하는 경우
				{
					var loopStartOffset = beatTime * m_sectionData.beatStart;

					// 다른쪽 플레이어에 섹션 정보 세팅, 루프 시작 위치로 재생 예약
					SwitchPlayerComponent();
					SetSectionToCurrentPlayer();

					while (!currentPlayerComponent.readyToPlay)						// 로딩될 때까지 대기
						yield return null;

					if (!m_suppressLooping)											// 로딩 대기 중에 이 플래그값이 true가 될 수도 있다. 그렇지 않을 때만 실제 재생
					{
						currentPlayerComponent.PlayScheduled(nextLoopEndDspTime, loopStartOffset);

						//Debug.Log("Loop reserved");
						m_loopReserved		= true;									// 루프 예약 지정
					}
				}

				yield return null;


				if (AudioSettings.dspTime >= nextLoopEndDspTime)					// 루프 시간을 넘을 때마다
				{
					m_loopReserved		= false;									// 루프 예약 여부 해제
					nextLoopEndDspTime	+= m_loopLengthDspTime;						// 다음 루프 시간 지정
				}
			}
		}

		/// <summary>
		/// 현재 Section 바로 종료.
		/// </summary>
		public void EndSection()
		{
			if (m_updateCo != null)
			{
				m_context.StopCoroutine(m_updateCo);

				if (m_transitionCo != null)
					m_context.StopCoroutine(m_transitionCo);

				m_endTransition	= TransitionType.None;
				isOnTransition	= false;
				m_state			= State.AfterLoop;
				
				currentPlayerComponent.StopImmediately();
				SwitchPlayerComponent();
				currentPlayerComponent.StopImmediately();
			}
		}

		/// <summary>
		/// 아웃 트랜지션 예약
		/// </summary>
		/// <param name="outtype"></param>
		/// <param name="transitionLength">트랜지션에 걸리는 시간</param>
		/// <returns>다음 섹션 시작 시간</returns>
		public double FadeoutSection(TransitionType outtype, double transitionLength)
		{
			if (m_transitionCo != null)							// 트랜지션이 걸려있다면 이전 것을 취소
			{
				m_context.StopCoroutine(m_transitionCo);
			}

			m_endTransition			= outtype;
			m_endTransitionLength	= transitionLength;

			switch(m_endTransition)
			{
				case TransitionType.Natural:
					m_transitionCo	= m_context.StartCoroutine(TransitionCo_Natural());
					break;

				case TransitionType.Manual:
					m_transitionCo	= m_context.StartCoroutine(TransitionCo_Manual());
					break;
				default:
					throw new System.InvalidOperationException("wrong transition type : " + outtype);
			}

			return m_properNextSectionStartTime;
		}

		IEnumerator TransitionCo_Natural()
		{
			var transitionStart	= nextLoopEndDspTime - m_endTransitionLength;
			var transitionEnd	= nextLoopEndDspTime;
			if (transitionStart <= AudioSettings.dspTime)		// 적당한 트랜지션 시작 시간이 이미 지나버린 경우, 다음 루프때 시도해야 한다.
			{
				transitionStart	+= m_loopLengthDspTime;
				transitionEnd	+= m_loopLengthDspTime;
			}

			m_properNextSectionStartTime	= transitionStart;	// 다음 섹션 시작 시간 결과값


			//Debug.Log("current dspTime : " + AudioSettings.dspTime + ", transitionStart : " + transitionStart);
			//Debug.Log("nextLoopEndDspTime : " + nextLoopEndDspTime + ", m_endtransitionLength : " + m_endTransitionLength);

			while (AudioSettings.dspTime < transitionStart)		// 트랜지션 시작 시간까지 대기
			{
				// FIX : 이번 회차에 루프를 끝내야 하는 경우 루프 중지 플래그를 올린다.
				if (!m_suppressLooping && transitionEnd - AudioSettings.dspTime < m_loopLengthDspTime)
				{
					m_suppressLooping	= true;
					//Debug.Log("TransitionCo_Natural : loop suppressed");
				}

				yield return null;
			}

			isOnTransition		= true;							// 실제 트랜지션 시작
			//Debug.Log("Natural Transition Start dspTime : " + AudioSettings.dspTime);

			// NOTE : 스테이트 꼬임 방지/강제 트랜지션의 경우 트랜지션이 모든 것을 우선하므로 루프를 멈춘다.
			// 이로 인해 문제가 생기면 다시 되돌려보고 생각한다...
			m_context.StopCoroutine(m_updateCo);

			var automation	= GetAutomation(m_sectionData.outTypeNatural);
			while (AudioSettings.dspTime < transitionEnd)		// 루프 타이밍까지 대기, 이후 루프 종료. 음원은 알아서 끝나게 내버려둔다
			{
				if (automation != null)							// 오토메이션 적용
				{
					var t	= (AudioSettings.dspTime - transitionStart) / (transitionEnd - transitionStart);
					m_transitionAutomationTarget.Set(automation.targetParam, automation.GetValue((float)t));
				}
				yield return null;
			}
			m_transitionAutomationTarget.Set(automation.targetParam, automation.GetValue(1));	// 마지막 값으로 보정

			isOnTransition		= false;						// 실제 트랜지션 종료
			m_endTransition		= TransitionType.None;
			//Debug.Log("Natural Transition End dspTime : " + AudioSettings.dspTime);


			//m_context.StopCoroutine(m_updateCo);

			m_state		= State.AfterLoop;
		}

		IEnumerator TransitionCo_Manual()
		{
			// TODO : 이렇게 하면 BPM이 다른 트랙끼리는 동작이 이상할 수도 있다...?
			var transitionStart	= m_clock.CalcNextSafeBeatTime();
			var transitionEnd	= transitionStart + m_endTransitionLength;

			m_properNextSectionStartTime	= transitionStart;	// 다음 섹션 시작 시간 결과값

			while (AudioSettings.dspTime < transitionStart)		// 트랜지션 시작 시간까지 대기
				yield return null;

			// NOTE : 스테이트 꼬임 방지/강제 트랜지션의 경우 트랜지션이 모든 것을 우선하므로 루프를 멈춘다.
			// 이로 인해 문제가 생기면 다시 되돌려보고 생각한다...
			m_context.StopCoroutine(m_updateCo);

			isOnTransition		= true;							// 실제 트랜지션 시작
			//Debug.Log("Manual Transition Start dspTime : " + AudioSettings.dspTime);

			var automation	= GetAutomation(m_sectionData.outTypeManual);
			while (AudioSettings.dspTime < transitionEnd)		// 트랜지션 끝까지 반복, 오토메이션 적용
			{
				if (automation != null)
				{
					var t	= (AudioSettings.dspTime - transitionStart) / (transitionEnd - transitionStart);
					m_transitionAutomationTarget.Set(automation.targetParam, automation.GetValue((float)t));
					//Debug.Log(string.Format("out transition value : {0} at t = {1}", automation.GetValue((float)t), t));
				}
				yield return null;
			}
			m_transitionAutomationTarget.Set(automation.targetParam, automation.GetValue(1));	// 마지막 값으로 보정

			isOnTransition		= false;						// 실제 트랜지션 종료
			m_endTransition		= TransitionType.None;
			//Debug.Log("Natural Transition End dspTime : " + AudioSettings.dspTime);

			currentPlayerComponent.StopImmediately();			// 음원 바로 중지 후 루프 종료
			
			//m_context.StopCoroutine(m_updateCo);

			m_state		= State.AfterLoop;
		}





		static Data.Automation GetAutomation(Data.Section.InType intype)
		{
			Data.Automation	auto	= null;
			switch(intype)
			{
				case Data.Section.InType.FadeIn:
					auto	= Data.Automation.LinearFadeIn;
					break;
				case Data.Section.InType.KickIn:
					auto	= Data.Automation.InstantUnMute;
					break;
			}
			return auto;
		}

		static Data.Automation GetAutomation(Data.Section.OutType outtype)
		{
			Data.Automation	auto	= null;
			switch(outtype)
			{
				case Data.Section.OutType.FadeOut:
					auto	= Data.Automation.LinearFadeOut;
					break;
				case Data.Section.OutType.Mute:
					auto	= Data.Automation.InstantMute;
					break;
				case Data.Section.OutType.LeaveIt:
					auto	= Data.Automation.InstantUnMute;
					break;
			}
			return auto;
		}
	}
}
