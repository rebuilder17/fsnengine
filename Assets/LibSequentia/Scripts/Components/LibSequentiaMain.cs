using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

using LibSequentia.Engine;
using LibSequentia.Data;


/// <summary>
/// LibSequentia 의 메인 컴포넌트.
/// </summary>
public class LibSequentiaMain : MonoBehaviour
{
	// Properties

	[SerializeField]
	UnityEngine.Audio.AudioMixer []	m_mixers_deckA_section1;
	[SerializeField]
	UnityEngine.Audio.AudioMixer []	m_mixers_deckA_section2;
	[SerializeField]
	UnityEngine.Audio.AudioMixer [] m_mixers_deckA;
	[SerializeField]
	UnityEngine.Audio.AudioMixer []	m_mixers_deckB_section1;
	[SerializeField]
	UnityEngine.Audio.AudioMixer []	m_mixers_deckB_section2;
	[SerializeField]
	UnityEngine.Audio.AudioMixer [] m_mixers_deckB;

	[SerializeField]
	UnityEngine.Audio.AudioMixer [] m_mixers_decks;



	// Members

	LibSequentiaAutomationManager	m_automationMgr;
	LibSequentiaAudioClipDepot		m_audioClipDepot;

	MasterPlayer					m_masterplayer;
	StepControl						m_stepctrl;

	float			m_tension;

	/// <summary>
	/// 긴장도 설정
	/// </summary>
	public float tension
	{
		get { return m_tension; }
		set
		{
			m_tension	= value;
			m_masterplayer.tension = value;
		}
	}

	/// <summary>
	/// 곡 전환시 트랜지션 비율
	/// </summary>
	public float songTransition
	{
		get { return m_masterplayer.transition; }
		set
		{
			m_masterplayer.transition	= value;
		}
	}

	/// <summary>
	/// Step 단위로 명령을 내리는 객체
	/// </summary>
	public StepControl stepControl
	{
		get { return m_stepctrl; }
	}

	/// <summary>
	/// 오디오 클립 보관소
	/// </summary>
	public LibSequentiaAudioClipDepot clipDepot
	{
		get { return m_audioClipDepot; }
	}



	static LibSequentiaMain s_instance;
	public static LibSequentiaMain instance
	{
		get
		{
			if (s_instance == null)
			{
				var module	= GameObject.FindObjectOfType<LibSequentiaMain>();
				module.Init();
			}
			return s_instance;
		}
		private set
		{
			s_instance	= value;
		}
	}

	void Awake()
	{
		Init();
	}

	bool m_init	= false;
	void Init()
	{
		if (!m_init)
		{
			m_init				= true;

			s_instance			= this;

			m_automationMgr		= gameObject.AddComponent<LibSequentiaAutomationManager>();
			m_audioClipDepot	= gameObject.AddComponent<LibSequentiaAudioClipDepot>();

			InitPlayer();


			// 스텝 컨트롤러 (추가)
			m_stepctrl			= new StepControl(m_masterplayer, this);
		}
	}


	/// <summary>
	/// 이름 생성
	/// </summary>
	/// <param name="deck"></param>
	/// <param name="section"></param>
	/// <param name="layer"></param>
	static string GetMixerName(int deck, int section = -1, int layer = -1)
	{
		string mname	= deck == 0? "deckA" : "deckB";
		
		if (section != -1)
		{
			mname		+= "section" + section;
		}

		if (layer != -1)
		{
			mname		+= "layer" + layer;
		}

		return mname;
	}

	/// <summary>
	/// 플레이어 새로 설정
	/// </summary>
	void InitPlayer()
	{
		// Mixer 컨트롤러 초기화

		var mixer_layers	= new AudioMixer[][] { m_mixers_deckA_section1, m_mixers_deckA_section2, m_mixers_deckB_section1, m_mixers_deckB_section2 };
		var mixer_decks		= new AudioMixer[][] { m_mixers_deckA, m_mixers_deckB };

		for (int deck = 0; deck < 2; deck++ )
		{
			m_automationMgr.AddAutomationControlToMixer(GetMixerName(deck), m_mixers_decks[deck]);

			var mdeckarr	= mixer_decks[deck];
			for (int section = 0; section < 2; section++)
			{
				m_automationMgr.AddAutomationControlToMixer(GetMixerName(deck, section), mdeckarr[section], true);

				var mlayerarr	= mixer_layers[section];
				for (int layer = 0; layer < 4; layer++)
				{
					m_automationMgr.AddAutomationControlToMixer(GetMixerName(deck, section, layer), mlayerarr[layer], true);
				}
			}
		}

		// 플레이어 생성
		var tplayer1		= CreateTrackPlayer(0, m_mixers_deckA_section1, m_mixers_deckA_section2);
		var tplayer2		= CreateTrackPlayer(1, m_mixers_deckB_section1, m_mixers_deckB_section2);

		m_masterplayer		= new MasterPlayer(this);
		m_masterplayer.SetTrackPlayers(tplayer1, tplayer2);
		var deckActrl		= m_automationMgr.GetAutomationControlToSingleMixer(GetMixerName(0));
		var deckBctrl		= m_automationMgr.GetAutomationControlToSingleMixer(GetMixerName(1));
		m_masterplayer.SetTransitionCtrls(deckActrl, deckBctrl);
	}

	TrackPlayer CreateTrackPlayer(int deck, AudioMixer[] section1Mixers, AudioMixer[] section2Mixers)
	{
		// 텐션 오토메이션 버스 생성
		AutomationHub [] tensionCtrlBus			= new AutomationHub[4];
		IAutomationControl [] sec1TensionCtrls	= new IAutomationControl[4];
		IAutomationControl [] sec2TensionCtrls	= new IAutomationControl[4];
		for (int i = 0; i < 4; i++)
		{
			var out1	= m_automationMgr.GetAutomationControlToSingleMixer(GetMixerName(deck, 0, i));
			var out2	= m_automationMgr.GetAutomationControlToSingleMixer(GetMixerName(deck, 1, i));

			var bus		= new AutomationHub(m_automationMgr);
			bus.SetOutputs(out1, out2);
			bus.CreateChains(2);

			tensionCtrlBus[i]	= bus;

			sec1TensionCtrls[i]	= tensionCtrlBus[i].GetChain(0);
			sec2TensionCtrls[i]	= tensionCtrlBus[i].GetChain(1);
		}

		// 플레이어 생성
		var tplayer			= new TrackPlayer(this);

		var player1ctrl		= m_automationMgr.GetAutomationControlToSingleMixer(GetMixerName(deck, 0));
		var player1			= CreateSectionPlayer(section1Mixers, player1ctrl, sec1TensionCtrls);
		tplayer.AttachSectionPlayer(player1, player1ctrl);

		var player2ctrl		= m_automationMgr.GetAutomationControlToSingleMixer(GetMixerName(deck, 1));
		var player2			= CreateSectionPlayer(section2Mixers, player2ctrl, sec2TensionCtrls);
		tplayer.AttachSectionPlayer(player2, player2ctrl);

		return tplayer;
	}

	SectionPlayer CreateSectionPlayer(UnityEngine.Audio.AudioMixer[] layermixers, IAutomationControl transCtrl, IAutomationControl [] tensionCtrl)
	{
		var player1		= gameObject.AddComponent<LibSequentiaPlayer>();
		player1.SetTargetMixers(layermixers);

		var player2		= gameObject.AddComponent<LibSequentiaPlayer>();
		player2.SetTargetMixers(layermixers);

		var secPlayer	= new SectionPlayer(this);
		secPlayer.SetPlayerComponents(player1, player2);
		secPlayer.SetTransitionAutomationTarget(transCtrl);
		secPlayer.SetTensionAutomationTargets(tensionCtrl);

		return secPlayer;
	}

	/// <summary>
	/// Track을 로드
	/// </summary>
	/// <param name="path"></param>
	/// <returns></returns>
	public Track LoadTrack(string path)
	{
		var track1json		= new JSONObject(Resources.Load<TextAsset>(path).text);
		var clipPack		= m_audioClipDepot.LoadAndMakeAudioPack(Track.GatherRequiredClips(track1json));
		
		return Track.CreateFromJSON(track1json, clipPack);
	}

	/// <summary>
	/// 트랜지션 시나리오를 로드
	/// </summary>
	/// <param name="path"></param>
	/// <returns></returns>
	public TransitionScenario LoadTransitionScenario(string path)
	{
		var tscenjson		= new JSONObject(Resources.Load<TextAsset>(path).text);

		return TransitionScenario.CreateFromJSON(tscenjson);
	}

	/// <summary>
	/// 강제 리셋
	/// </summary>
	public void Reset()
	{
		m_masterplayer.Reset();
		m_stepctrl.Reset();
	}
}
