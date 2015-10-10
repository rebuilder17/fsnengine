using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

using LibSequentia.Engine;
using LibSequentia.Data;

/// <summary>
/// Section을 실제 플레이하기 위한 용도
/// </summary>
public class LibSequentiaPlayer : MonoBehaviour, IPlayerComponent
{
	// Members

	AudioSource []		m_audioSources		= new AudioSource[Section.c_maxLayerPerSection];		// 음원을 로드할 오디오 소스
	IAudioClipHandle [] m_audioClipHandles	= new IAudioClipHandle[Section.c_maxLayerPerSection];	// 오디오 클립 핸들


	/// <summary>
	/// 현재 플레이어에 세팅된 음원의 길이 구하기 (= 레이어 중에서 제일 긴 음원 길이를 찾아냄)
	/// </summary>
	public double fullAudioLength
	{
		get
		{
			double maxlength = 0;
			for (int i = 0; i < Section.c_maxLayerPerSection; i++)
			{
				var clip		= m_audioSources[i].clip;
				if (clip != null && clip.length > maxlength)
				{
					maxlength = clip.length;
				}
			}
			return maxlength;
		}
	}

	public bool readyToPlay
	{
		get
		{
			for (int i = 0; i < Section.c_maxLayerPerSection; i++)
			{
				var clip = m_audioSources[i].clip;
				if (clip != null && clip.loadState != AudioDataLoadState.Loaded)	// 세팅된 오디오클립 중에 하나라도 준비되지 않은 것이 있다면 false
					return false;
			}
			return true;
		}
	}

	/// <summary>
	/// 오디오 소스 배열
	/// </summary>
	public AudioSource[] audioSources
	{
		get { return m_audioSources; }
	}





	void Awake()
	{
		// AudioSource 생성
		for (int i = 0; i < Section.c_maxLayerPerSection; i++)
		{
			var source	= gameObject.AddComponent<AudioSource>();

			m_audioSources[i] = source;
		}
	}

	/// <summary>
	/// 각 Layer를 라우팅할 믹서 세팅. 믹서의 Master로 나간다.
	/// </summary>
	/// <param name="mixers"></param>
	public void SetTargetMixers(params AudioMixer [] mixers)
	{
		for (int i = 0; i < Section.c_maxLayerPerSection; i++)
		{
			var source						= m_audioSources[i];
			source.spatialBlend				= 0;					// 2D 사운드로
			source.loop						= false;				// 자동 루프는 사용하지 않는다
			source.playOnAwake				= false;
			source.outputAudioMixerGroup	= mixers[i].FindMatchingGroups("Master")[0];
		}
	}


	/// <summary>
	/// 해당 레이어에 오디오 클립 세팅
	/// </summary>
	/// <param name="layerIndex"></param>
	/// <param name="handle"></param>
	public void SetAudioClip(int layerIndex, IAudioClipHandle handle)
	{
		ClearAudioClip(layerIndex);

		var source	= m_audioSources[layerIndex];
		source.clip	= handle.Acquire() as AudioClip;

		m_audioClipHandles[layerIndex] = handle;
	}

	/// <summary>
	/// 오디오 클립 세팅 해제
	/// </summary>
	/// <param name="layerIndex"></param>
	public void ClearAudioClip(int layerIndex)
	{
		var source	= m_audioSources[layerIndex];
		if (source.isPlaying)
		{
			source.Stop();
		}
		source.clip	= null;

		if (m_audioClipHandles[layerIndex] != null)
		{
			m_audioClipHandles[layerIndex].Return();
			m_audioClipHandles[layerIndex] = null;
		}
	}

	/// <summary>
	/// 오토메이션 타겟 설정
	/// </summary>
	/// <param name="layerIndex"></param>
	/// <returns></returns>
	public IAutomationControl GetAutomationTarget(int layerIndex)
	{
		throw new System.NotImplementedException();
	}

	

	/// <summary>
	/// 지정한 타이밍에, 지정한 오프셋에서 플레이
	/// </summary>
	/// <param name="dsptime"></param>
	/// <param name="offset"></param>
	public void PlayScheduled(double dsptime, double offset = 0)
	{
		for(int i = 0; i < Section.c_maxLayerPerSection; i++)
		{
			var source			= m_audioSources[i];
			if (source.clip != null)
			{
				if (source.isPlaying)
					source.Stop();
				source.timeSamples	= (int)(offset * (double)source.clip.frequency);
				source.PlayScheduled(dsptime);
			}
		}
	}

	/// <summary>
	/// 현재 재생 즉시 정지
	/// </summary>
	public void StopImmediately()
	{
		for (int i = 0; i < Section.c_maxLayerPerSection; i++)
		{
			//var source			= m_audioSources[i];
			//if (source.isPlaying)
			//	source.Stop();

			ClearAudioClip(i);
		}
	}
}
