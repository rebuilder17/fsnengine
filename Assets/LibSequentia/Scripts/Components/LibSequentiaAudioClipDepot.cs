using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using LibSequentia.Data;

/// <summary>
/// 오디오 클립 보관소
/// </summary>
public class LibSequentiaAudioClipDepot : MonoBehaviour
{



	/// <summary>
	/// 오디오 클립 핸들
	/// </summary>
	class AudioClipHandle : IAudioClipHandle
	{
		LibSequentiaAudioClipDepot	m_depot;
		string m_clippath;
		int account	= 0;		// 자체 acquire카운터

		public AudioClipHandle(LibSequentiaAudioClipDepot depot, string clippath)
		{
			m_depot = depot;
			m_clippath	= clippath;
		}

		public object Acquire()
		{
			account++;
			return m_depot.Acquire(m_clippath);
		}

		public void Return()
		{
			if (account == 0)
			{
				Debug.LogError("this handle cannot be release more");
			}
			else
			{
				account--;
				m_depot.Return(m_clippath);
			}
		}
	}

	/// <summary>
	/// 오디오 팩
	/// </summary>
	class AudioClipPack : IAudioClipPack
	{
		//LibSequentiaAudioClipDepot				m_depot;
		Dictionary<string, IAudioClipHandle>	m_handleDict;

		public AudioClipPack(LibSequentiaAudioClipDepot depot, params string [] clips)
		{
			//m_depot			= depot;

			m_handleDict	= new Dictionary<string, IAudioClipHandle>();
			for (int i = 0; i < clips.Length; i++)
			{
				var path			= clips[i];
				m_handleDict[path]	= depot.GetHandle(path);
			}
		}

		public IAudioClipHandle GetHandle(string path)
		{
			return m_handleDict[path];
		}
	}


	class AudioClipInfo
	{
		public string		path;
		public AudioClip	clip;
		public int			refcount	= 0;
	}

	// Members

	Dictionary<string, AudioClipInfo>	m_audioClipDict	= new Dictionary<string, AudioClipInfo>();		// 오디오 클립 경로 => 오디오 클립 로딩 정보를 들고 있는 딕셔너리


	void Awake()
	{

	}


	/// <summary>
	/// 오디오클립 새로 로드.
	/// </summary>
	/// <param name="clippath"></param>
	public void Load(string clippath)
	{
		if(!m_audioClipDict.ContainsKey(clippath))	// 이미 로드되지 않은 경우만 작동
		{
			var info		= new AudioClipInfo();
			info.clip		= Resources.Load(clippath) as AudioClip;
			info.path		= clippath;
			info.refcount	= 0;

			m_audioClipDict[clippath]	= info;
		}
	}

	/// <summary>
	/// 클립 핸들 구하기
	/// </summary>
	/// <param name="clippath"></param>
	/// <returns></returns>
	public IAudioClipHandle GetHandle(string clippath)
	{
		if (!m_audioClipDict.ContainsKey(clippath))
		{
			throw new System.ArgumentException("No audioclip named " + clippath + " loaded");
		}
		return new AudioClipHandle(this, clippath);
	}

	/// <summary>
	/// 지정된 클립들을 로디하고 오디오팩을 만듦
	/// </summary>
	/// <returns></returns>
	public IAudioClipPack LoadAndMakeAudioPack(params string [] clippaths)
	{
		for(int i = 0; i < clippaths.Length; i++)
		{
			Load(clippaths[i]);
		}

		return new AudioClipPack(this, clippaths);
	}



	AudioClip Acquire(string clippath)
	{
		var info	= m_audioClipDict[clippath];
		if (info.refcount == 0)					// ref가 없던 상태에서 새로 로딩할 경우
		{

		}
		info.refcount++;						// 레퍼런스 카운터 증가

		return info.clip;
	}

	void Return(string clippath)
	{
		var info	= m_audioClipDict[clippath];
		if(info.refcount == 1)					// ref가 0으로 감소할 때 (언로딩?)
		{

		}
		info.refcount--;							// 레퍼런스 카운터 감소
	}
}
