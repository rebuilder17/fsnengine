using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace LibSequentia.Engine
{
	/// <summary>
	/// 비트 동기화를 위한 타이밍 계산기
	/// </summary>
	public class BeatSyncClock
	{
		// Constants

		const double				c_safeTimeGap	= 0.200;	// 로딩/초기화 시간을 고려한 "안전한" 지연 시간


		// Members

		double		m_syncDspTime;								// BeatSync의 기준이 되는 dsp time
		double		m_bpm;
		double		m_secPerBeat;



		/// <summary>
		/// BPM
		/// </summary>
		public double BPM
		{
			get { return m_bpm; }
			set
			{
				m_bpm			= value;
				m_secPerBeat	= 60.0 / m_bpm;					// 1 beat 당 시간을 미리 계산해둔다
			}
		}

		/// <summary>
		/// 1 beat 당 시간 (초)
		/// </summary>
		public double SecondPerBeat
		{
			get { return m_secPerBeat; }
		}




		/// <summary>
		/// 현재 시점의 dspTime을 기준으로 하여 초기화
		/// </summary>
		/// <param name="bpm"></param>
		public BeatSyncClock(float bpm)
		{
			BPM				= bpm;
			m_syncDspTime	= AudioSettings.dspTime;
		}

		/// <summary>
		/// 특정 dspTime을 기준으로 초기화
		/// </summary>
		/// <param name="bpm"></param>
		/// <param name="syncDspTime"></param>
		public BeatSyncClock(float bpm, double syncDspTime)
		{
			BPM				= bpm;
			m_syncDspTime	= syncDspTime;
		}


		/// <summary>
		/// 다음번에 오는 "안전한" beat 시간. 
		/// </summary>
		/// <returns>dsp time</returns>
		public double CalcNextSafeBeatTime()
		{
			double secPerBeat			= m_secPerBeat;
			double dspTime				= AudioSettings.dspTime;
			double timeOffset			= dspTime - m_syncDspTime;

			double curBeatTimeOffset	= System.Math.Floor(timeOffset / secPerBeat) * secPerBeat;	// 현재 위치한 beat의 시간
			double nextBeatTimeOffset	= curBeatTimeOffset + secPerBeat;							// 1 beat만큼의 시간을 더하면 다음 비트 시간이 됨

			while (nextBeatTimeOffset - timeOffset < c_safeTimeGap)									// 현재 시간과 다음 비트 사이의 간격이 너무 작다면 1beat 더 뒤로 미룬다
			{
				nextBeatTimeOffset		+= secPerBeat;
			}

			return m_syncDspTime + nextBeatTimeOffset;
		}

		/// <summary>
		/// 다음에 올 안전한 beat에 오프셋을 줘서 시간 계산
		/// </summary>
		/// <param name="beatOffset"></param>
		/// <returns>dsp time</returns>
		public double CalcSafeBeatOffsetTime(int beatOffset)
		{
			double nextBeatTime	= CalcNextSafeBeatTime();
			return nextBeatTime + m_secPerBeat * (double)beatOffset;
		}

		/// <summary>
		/// dspTime 기준으로 특정 비트 수의 시간 길이를 계산
		/// </summary>
		/// <param name="beats"></param>
		/// <returns></returns>
		public double CalcBeatTimeLength(int beats)
		{
			return m_secPerBeat * (double)beats;
		}
	}
}