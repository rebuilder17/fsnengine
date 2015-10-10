using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;
using LibSequentia.Data;
using LibSequentia.Engine;

public class LibSequentiaAutomationManager : MonoBehaviour, IAutomationHubManager
{
	/// <summary>
	/// Audio Mixer를 대상으로 한 Automation 컨트롤
	/// </summary>
	class AudioMixerAutomationControl : IAutomationControl
	{
		// Static Members

		class ParamInfo
		{
			public delegate float ValueFunc(float input, ParamInfo info);

			public float	valueMin;			// 입력값이 0일 때 mixer 파라미터 값
			public float	valueMax;			// 입력값이 1일 때 mixer 파라미터 값
			public string	mixerParamName;		// mixer external parameter 이름
			public ValueFunc valueFunc;			// 인풋-아웃풋 변환 함수

			public string [] snapshotNames;		// Interpolation에 사용할 스냅샷 이름들

			public float ToMixerParamValue(float value)
			{
				//return (valueMax - valueMin) * value + valueMin;
				return valueFunc(value, this);
			}
		}

		static Dictionary<Automation.TargetParam, ParamInfo>	s_infoDict;

		static AudioMixerAutomationControl()
		{
			// 파라미터 정보 채우기
			s_infoDict	= new Dictionary<Automation.TargetParam, ParamInfo>();

			// 기본값은 null으로 둔다
			int paramCount	= Automation.targetParamEnumValues.Length;
			for(int i = 0; i < paramCount; i++)
			{
				s_infoDict[Automation.targetParamEnumValues[i]]	= null;
			}

			// 변환 함수 목록
			ParamInfo.ValueFunc func_linear	= (float input, ParamInfo info) => (info.valueMax - info.valueMin) * input + info.valueMin;
			ParamInfo.ValueFunc func_volume	= (float input, ParamInfo info) => 
				{
					var input_sin	= Mathf.Sin((input - 0.5f) * Mathf.PI) * 0.5f + 0.5f;
					return 1 - (Mathf.Max(info.valueMin, 20 * Mathf.Log10(input_sin)) / info.valueMin);
				};

			//
			s_infoDict[Automation.TargetParam.Volume]	= new ParamInfo() { valueMin = -80f, valueMax = 0f, mixerParamName = null, valueFunc = func_volume, snapshotNames = new string[] { "VolumeZero", "VolumeFull" } };
			s_infoDict[Automation.TargetParam.LowCut]	= new ParamInfo() { valueMin = -80f, valueMax = 0f, mixerParamName = "LowCutLevel", valueFunc = func_linear };
		}


		// Members

		AudioMixer	m_mixer;
		Dictionary<Automation.TargetParam, AudioMixerSnapshot []> m_snapshotDict	= new Dictionary<Automation.TargetParam, AudioMixerSnapshot[]>();
		HashSet<Automation.TargetParam>	m_ignoreParam	= new HashSet<Automation.TargetParam>();

		public AudioMixerAutomationControl(AudioMixer mixer)
		{
			m_mixer	= mixer;
		}

		/// <summary>
		/// 오토메이션 적용을 무시할 파라미터 세팅
		/// </summary>
		/// <param name="param"></param>
		public void AddParamToIgnore(Automation.TargetParam param)
		{
			m_ignoreParam.Add(param);
		}

		static float [] _temp_snapshot_weight = new float[] { 0, 1 };
		public void Set(Automation.TargetParam param, float value)
		{
			if (m_ignoreParam.Contains(param))					// 무시 목록에 없을 때만 실행한다.
				return;

			var info	= s_infoDict[param];
			if (info.mixerParamName != null)					// ExposedParam을 조절하는 케이스
			{
				m_mixer.SetFloat(info.mixerParamName, info.ToMixerParamValue(value));
			}
			else
			{													// Snapshot을 조절하는 케이스
				AudioMixerSnapshot [] snapshots;
				if (!m_snapshotDict.TryGetValue(param, out snapshots))	// 해당 파라미터에 맞는 스냅샷 목록을 아직 레퍼런스로 소유하고 있지 않다면, 찾아서 등록
				{
					int namecount	= info.snapshotNames.Length;
					snapshots		= new AudioMixerSnapshot[namecount];
					for (int i = 0; i < namecount; i++)
					{
						snapshots[i]	= m_mixer.FindSnapshot(info.snapshotNames[i]);
					}
					m_snapshotDict[param]	= snapshots;
				}

				var calcvalue				= info.ToMixerParamValue(value);
				_temp_snapshot_weight[0]	= 1 - calcvalue;
				_temp_snapshot_weight[1]	= calcvalue;
				m_mixer.TransitionToSnapshots(snapshots, _temp_snapshot_weight, 0.1f);
			}
		}
	}

	
	/// <summary>
	/// 오디오 소스를 대상으로 한 오토메이션 컨트롤
	/// </summary>
	class AudioSourceAutomationControl : IAutomationControl
	{
		// Members

		AudioSource m_source;

		public AudioSourceAutomationControl(AudioSource source)
		{
			m_source	= source;
		}

		/// <summary>
		/// 볼륨만 컨트롤한다.
		/// </summary>
		/// <param name="param"></param>
		/// <param name="value"></param>
		public void Set(Automation.TargetParam param, float value)
		{
			if(param == Automation.TargetParam.Volume)
			{
				m_source.volume	= value;
			}
		}
	}
	//



	// Members

	Dictionary<string, IAutomationControl>	m_mixerControls	= new Dictionary<string, IAutomationControl>();

	List<IAutomationHubHandle>	m_handles	= new List<IAutomationHubHandle>();	// 업데이트해줘야할 automation hub 핸들


	void Awake()
	{
	}

	void Update()
	{
		int count	= m_handles.Count;
		for(int i = 0; i < count;i++)
		{
			m_handles[i].Update();
		}
	}

	public void AddAutomationControlToMixer(string ctrlname, AudioMixer mixer, bool simpleCtrl = false)
	{
		var mixerctrl				= new AudioMixerAutomationControl(mixer);
		m_mixerControls[ctrlname]	= mixerctrl;

		if(simpleCtrl)				// 단순화하는 경우 일부 파라미터를 조정하지 못하도록 지정한다.
		{
			mixerctrl.AddParamToIgnore(Automation.TargetParam.LowCut);
		}
	}

	public void AddAutomationControlToSource(string ctrlname, AudioSource source)
	{
		var sourcectrl				= new AudioSourceAutomationControl(source);
		m_mixerControls[ctrlname]	= sourcectrl;
	}

	public IAutomationControl GetAutomationControlToSingleMixer(string ctrlname)
	{
		return m_mixerControls[ctrlname];
	}

	public void RegisterAutomationHub(IAutomationHubHandle hub)
	{
		m_handles.Add(hub);
	}
}
