using UnityEngine;
using System.Collections;




namespace LibSequentia
{
	/// <summary>
	/// LibSequentia - 스크립트 세그먼트
	/// </summary>
	public class ScriptSegment : Segments.Object
	{
		public const string		c_property_tension		= "Tension";		// 긴장도
		public const string		c_property_songtrans	= "Transition";		// (곡 사이의) 전환 비율

		public float			tension					= 1f;
		public float			songTrans				= 0;

		protected override string ConvertAliasPropertyName(string name)
		{
			switch(name)
			{
				case "긴장도":
				case "텐션":
					return c_property_tension;
				case "전환":
					return c_property_songtrans;
			}
			return base.ConvertAliasPropertyName(name);
		}

		protected override bool SetPropertyImpl(string name, string param)
		{
			bool processed	= false;
			switch(name)
			{
				case c_property_tension:
					tension		= float.Parse(param);
					processed	= true;
					break;

				case c_property_songtrans:
					songTrans	= float.Parse(param);
					processed	= true;
					break;
			}

			return processed || base.SetPropertyImpl(name, param);
		}
	}

	/// <summary>
	/// LibSequentia - 스냅샷에 포함되는 객체
	/// </summary>
	public class SnapshotElement : SnapshotElems.ObjectBase<SnapshotElement>
	{
		public float Tension { get; set; }
		public float SongTransition { get; set; }

		float	m_progress	= 0;		// 진행 방향을 알기 위한 내부 변수. 세팅할 때마다 1씩 더해줘야 한다.

		public float scriptProgress
		{
			get { return m_progress; }
		}
		public void CountUpScriptProgress()
		{
			m_progress += 1;
			//m_progress	= ChainedParentCount;
			Debug.Log("CountUpScriptProgress : " + m_progress);
		}

		public void SetNegativeProgress()
		{
			m_progress	= -1;
			Debug.Log("SetNegativeProgress : " + m_progress);
		}

		public override void CopyDataTo(SnapshotElement to)
		{
			base.CopyDataTo(to);
			to.Tension			= Tension;
			to.SongTransition	= SongTransition;

			to.m_progress		= m_progress;
		}

		public override void LerpBetweenElems(SnapshotElems.ObjectBase<SnapshotElement> elem1, SnapshotElems.ObjectBase<SnapshotElement> elem2, float t)
		{
			// TODO : 필요없는 요소들은 트랜지션 안해도...
			base.LerpBetweenElems(elem1, elem2, t);

			var e1		= elem1 as SnapshotElement;
			var e2		= elem2 as SnapshotElement;
			Tension		= Mathf.Lerp(e1.Tension, e2.Tension, t);
			SongTransition	= Mathf.Lerp(e1.SongTransition, e2.SongTransition, t);
			m_progress	= Mathf.Lerp(e1.m_progress, e2.m_progress, t);
		}
	}

	public class LayerObject : LayerObjects.BaseObjectLayerObject<SnapshotElement>
	{
		struct StepState
		{
			public Data.Track	curtrack;
			public int			step;

			public Data.Track	newtrack;
			public int			newstep;

			public Data.TransitionScenario tscen;
		}


		float		m_tension;
		float		m_songTrans;
		
		float		m_progress;
		bool		m_reverse;
		bool		m_newTrackReverse;

		static StepState	s_prevState;


		public float Tension
		{
			get { return m_tension; }
			set
			{
				if (m_tension != value)
				{
					m_tension	= value;
					UpdateTension(value);
				}
			}
		}

		public float SongTransition
		{
			get { return m_songTrans; }
			set
			{
				if(m_songTrans != value)
				{
					m_songTrans	= value;
					UpdateSongTransition(value);
				}
			}
		}


		protected void UpdateTension(float tension)
		{
			LibSequentiaMain.instance.tension	= tension;
		}

		protected void UpdateSongTransition(float ratio)
		{
			LibSequentiaMain.instance.songTransition	= ratio;
		}

		protected override void UpdateComponentParam(string to)
		{
			if (to == null)
				return;

			Debug.Log("parameter in : " + to);

			// 파싱해서 LibSequentia의 메세지로 보낸다.
			// 형식 : 현재 트랙 경로, 현재 트랙 스텝[, 새 트랙 경로, 새 트랙 스텝]
			
			var lsengine	= LibSequentiaMain.instance;
			var ctrl		= lsengine.stepControl;

			var split	= to.Split(',');
			if (split.Length == 2)				// 일반 재생
			{
				var curtrpath	= split[0].Trim();
				var cur			= new StepState()
				{
					curtrack	= FSNResourceCache.Load<LibSequentia.Data.Track>(FSNResourceCache.Category.Script, curtrpath),
					step		= int.Parse(split[1].Trim()),
				};

				if (!m_reverse && cur.step == 0 || m_reverse && cur.step == (cur.curtrack.sectionCount + 1) * 2)
				{
					// 정방향일 때 스텝이 0이거나 역방향일 때 (섹션수+1)*2 스텝일 경우엔 재생하지 않고 무시해준다. (각 방향의 끝점으로 갔을 때 재생 중지를 하기 위한 스텝임)
				}
				else
				{
					if (!lsengine.isPlaying)		// 재생중이 아닐 때는 트랙 새로 올리기
					{
						ctrl.StartWithOneTrack(cur.curtrack, cur.step, m_reverse);
						UpdateTension(m_tension);			// 파라미터 초기화
						UpdateSongTransition(m_songTrans);
					}
					else
					{								// 재생중일 때는 스텝 변경 메세지 (1트랙짜리)

						// TODO : 스테이트가 복잡해서 실수를 최대한 막기 위해 불필요한 코드들을 작성함. 나중에 줄여야함.
						if (m_reverse == false)	// (정방향)
						{
							ctrl.StepMove(cur.step, -1, m_reverse);
						}
						else
						{						// (역방향)
							ctrl.StepMove(cur.step, -1, m_reverse);
						}
					}
				}

				s_prevState	= cur;	// 현재 상태 저장
			}
			else if(split.Length >= 4)			// 트랙 전환 재생
			{
				var curtrpath	= split[0].Trim();
				var newtrpath	= split[2].Trim();
				var tscenpath	= split.Length >= 5? split[4].Trim() : ((FSNLibSequentiaModule)ParentModule).defaultTransitionScenarioPath;

				var cur			= new StepState()
				{
					curtrack	= FSNResourceCache.Load<LibSequentia.Data.Track>(FSNResourceCache.Category.Script, curtrpath),
					step		= int.Parse(split[1].Trim()),
					newtrack	= FSNResourceCache.Load<LibSequentia.Data.Track>(FSNResourceCache.Category.Script, newtrpath),
					newstep		= int.Parse(split[3].Trim()),
					tscen		= FSNResourceCache.Load<LibSequentia.Data.TransitionScenario>(FSNResourceCache.Category.Script, tscenpath)
				};

				if (!lsengine.isPlaying)		// 재생중이 아닐 때는 트랙 새로 올리기
				{
					m_reverse	= false;		// 트랙 두 개가 갑자기 올라가는 경우는 로딩 상황뿐 : 정방향 진행 상태
					ctrl.StartWithTwoTrack(cur.curtrack, cur.step, cur.newtrack, cur.newstep, cur.tscen);
					UpdateTension(m_tension);			// 파라미터 초기화
					UpdateSongTransition(m_songTrans);
				}
				else
				{								// 재생중일 때는 스텝 변경 메세지 (2트랙짜리, 곡 전환)

					// TODO : 스테이트가 복잡해서 실수를 최대한 막기 위해 불필요한 코드들을 작성함. 나중에 줄여야함.
					if (m_reverse == false)	// (정방향)
					{

						if (cur.newtrack != null && s_prevState.newtrack == null)
						{
							m_newTrackReverse	= m_reverse;
							ctrl.StepMove(cur.step, cur.newtrack, cur.tscen, cur.newstep, m_reverse);
						}
						else if (m_newTrackReverse && s_prevState.newstep == 1)
						{
							// 특수 케이스 처리 : 이전에 역방향으로 다음 트랙으로 넘어가는 자연 진행을 건 경우.
							// 새 트랙을 다시 올려줘야한다.
							m_newTrackReverse	= m_reverse;
							ctrl.StepMove(cur.step, cur.newtrack, cur.tscen, cur.newstep, m_reverse);
						}
						else if (cur.newtrack != null && s_prevState.newtrack != null)
						{
							if (m_newTrackReverse)
							{
								ctrl.StepMove(cur.newstep, cur.newtrack, cur.tscen, cur.step, m_reverse);
							}
							else
							{
								ctrl.StepMove(cur.step, cur.newtrack, cur.tscen, cur.newstep, m_reverse);
							}
						}
	
						else
						{
							ctrl.StepMove(cur.step, cur.newstep, m_reverse);
						}

					}
					else
					{						// (역방향)
						
						if (cur.newtrack != null && s_prevState.newtrack == null)
						{
							m_newTrackReverse	= m_reverse;
							ctrl.StepMove(cur.newstep, cur.curtrack, cur.tscen, cur.step, m_reverse);
						}
						else if (!m_newTrackReverse && s_prevState.newstep == 3)
						{
							// 특수 케이스 처리 : 이전에 역방향으로 다음 트랙으로 넘어가는 자연 진행을 건 경우.
							// 새 트랙을 다시 올려줘야한다.
							m_newTrackReverse	= m_reverse;
							ctrl.StepMove(cur.newstep, cur.curtrack, cur.tscen, cur.step, m_reverse);
						}
						else if (cur.newtrack != null && s_prevState.newtrack != null)
						{
							if (m_newTrackReverse)
							{
								ctrl.StepMove(cur.newstep, cur.curtrack, cur.tscen, cur.step, m_reverse);
							}
							else
							{
								ctrl.StepMove(cur.step, cur.curtrack, cur.tscen, cur.newstep, m_reverse);
							}
						}
	
						else
						{
							ctrl.StepMove(cur.step, cur.newstep, m_reverse);
						}

					}
				}

				s_prevState		= cur;	// 현재 상태 저장
			}
			else
			{
				// Error
				Debug.LogError("wrong parameter : " + to);
			}
		}

		void UpdateProgressAndReverse(float prog)
		{
			if (m_progress > prog)
				m_reverse	= true;
			else if (m_progress < prog)
				m_reverse	= false;

			Debug.LogFormat("[UpdateProgressAndReverse] m_progress : {0} prog : {1} m_reverse : {2}", m_progress, prog, m_reverse);

			m_progress	= prog;
		}

		public override void SetStateFully(SnapshotElement to)
		{
			UpdateProgressAndReverse(to.scriptProgress);	// m_reverse를 먼저 업데이트해줘야한다.

			base.SetStateFully(to);
			Tension			= to.Tension;
			SongTransition	= to.SongTransition;
		}

		public override void TransitionWith(SnapshotElement to, float ratio)
		{
			//base.TransitionWith(to, ratio);
			
			var trTension	= Mathf.Lerp(m_tension, to.Tension, ratio);
			var trSongTrans	= Mathf.Lerp(m_songTrans, to.SongTransition, ratio);

			UpdateTension(trTension);
			UpdateSongTransition(trSongTrans);
		}

		public LayerObject(FSNModule parent, GameObject gameObj, IInGameSetting setting)
			: base(parent, gameObj, setting)
		{
			
		}
	}


	/// <summary>
	/// 스크립트 명령어 셋
	/// </summary>
	static class ScriptCommands
	{
		const string c_ctrlObjName	= "control";

		public static void Install()
		{
			FSNScriptSequence.Parser.AddCommand(Preload, "lspreload", "음악프리로드");
			FSNScriptSequence.Parser.AddCommand(Ready, "lsready", "음악시스템사용");
			FSNScriptSequence.Parser.AddCommand(Set, "lsset", "음악설정");
		}


		/// <summary>
		/// 프리로드 명령어
		/// </summary>
		/// <param name="protocol"></param>
		static void Preload(FSNScriptSequence.Parser.ICommandGenerateProtocol protocol)
		{
			FSNResourceCache.Load<LibSequentia.Data.Track>(FSNResourceCache.Category.Script, protocol.parameters[0]);
		}

		/// <summary>
		/// 스크립트 안에서 사용하기 전에 호출해야함.
		/// 실제로는 레이어에 LibSequentia를 컨트롤할 오브젝트를 생성하는 역할을 한다.
		/// </summary>
		/// <param name="protocol"></param>
		static void Ready(FSNScriptSequence.Parser.ICommandGenerateProtocol protocol)
		{
			var newCtrlSeg		= new ScriptSegment();
			newCtrlSeg.command	= Segments.Object.CommandType.Create;

			_setupSegment(newCtrlSeg, protocol);		// 셋업

			// 현재 LibSequentia 엔진의 상태를 가져와 세팅해준다
			newCtrlSeg.tension		= LibSequentiaMain.instance.tension;
			newCtrlSeg.songTrans	= LibSequentiaMain.instance.songTransition;

			protocol.PushSegment(new FSNScriptSequence.Parser.GeneratedSegmentInfo()
				{
					newSeg			= newCtrlSeg,
					usePrevPeriod	= true,
					selfPeriod		= false
				});
		}

		static void Set(FSNScriptSequence.Parser.ICommandGenerateProtocol protocol)
		{
			var newCtrlSeg		= new ScriptSegment();
			newCtrlSeg.command	= Segments.Object.CommandType.SetKey;

			_setupSegment(newCtrlSeg, protocol);		// 셋업

			protocol.PushSegment(new FSNScriptSequence.Parser.GeneratedSegmentInfo()
				{
					newSeg			= newCtrlSeg,
					usePrevPeriod	= true,
					selfPeriod		= false
				});
		}

		static void _setupSegment(ScriptSegment seg, FSNScriptSequence.Parser.ICommandGenerateProtocol protocol, bool useObjName = true)
		{
			seg.layerID				= FSNLibSequentiaModule.c_layerID;
			seg.objectName			= c_ctrlObjName;

			//
			int settingCount		= (protocol.parameters.Length) / 2;		// 세팅 pair 갯수
			for (int i = 0; i < settingCount; i++)
			{
				var pName	= protocol.parameters[i * 2];
				var pParam	= protocol.parameters[i * 2 + 1];
				seg.SetPropertyFromScriptParams(pName, pParam);				// 파라미터 하나씩 세팅
			}
		}
	}

	class TrackLoader : FSNResourceCache.ICustomLoader
	{
		public object LoadResource(string path)
		{
			return LibSequentiaMain.instance.LoadTrack(path);
		}

		public void UnloadResource(object res)
		{

		}
	}

	class TransitionScenarioLoader : FSNResourceCache.ICustomLoader
	{
		public object LoadResource(string path)
		{
			return LibSequentiaMain.instance.LoadTransitionScenario(path);
		}

		public void UnloadResource(object res)
		{

		}
	}
}

public class FSNLibSequentiaModule : FSNBaseObjectModule<LibSequentia.ScriptSegment, LibSequentia.SnapshotElement, LibSequentia.LayerObject>
{
	public const int c_layerID	= 200;


	[SerializeField]
	string			m_defaultTransitionScenarioPath	= "libsequentia/data/ts_crossfade";


	public string defaultTransitionScenarioPath
	{
		get { return m_defaultTransitionScenarioPath; }
	}

	public override string ModuleName
	{
		get { return "LibSequentia"; }
	}

	protected override void SetElemBySegProperties(LibSequentia.SnapshotElement elem, LibSequentia.ScriptSegment seg)
	{
		base.SetElemBySegProperties(elem, seg);

		foreach(var property in seg.PropertyNames)
		{
			switch(property)
			{
				case LibSequentia.ScriptSegment.c_property_tension:
					elem.Tension	= seg.tension;
					break;

				case LibSequentia.ScriptSegment.c_property_songtrans:
					elem.SongTransition	= seg.songTrans;
					break;
			}
		}
	}

	public override void Initialize()
	{
		// 필수 요소 설치
		FSNResourceCache.InstallLoader<LibSequentia.Data.Track>(new LibSequentia.TrackLoader());
		FSNResourceCache.InstallLoader<LibSequentia.Data.TransitionScenario>(new LibSequentia.TransitionScenarioLoader());
		LibSequentia.ScriptCommands.Install();

		m_layerID			= c_layerID;	// 레이어 번호 강제 지정

		acceptClearCommand	= false;		// 전체 클리어 명령에 반응하지 않게 한다. (컨트롤 오브젝트가 사라지면 안됨)

		Debug.Log("FSNLibSequentiaModule installed");
	}

	public override void OnBeforeLoadSession()
	{
		// 모순을 방지하기 위해 세이브파일 로드시에는 LibSequentia엔진을 초기화해버린다
		LibSequentiaMain.instance.ResetModule();
	}

	protected override LibSequentia.LayerObject MakeNewLayerObject(LibSequentia.SnapshotElement element, IInGameSetting setting)
	{
		GameObject newObj	= new GameObject("LS_Control");
		newObj.layer		= gameObject.layer;
		var lobj			= new LibSequentia.LayerObject(this, newObj, setting);
		newObj.transform.SetParent(ObjectRoot, false);

		return lobj;
	}

	protected override void OnCreateElement(LibSequentia.ScriptSegment segment, FSNSnapshot.Layer layer, LibSequentia.SnapshotElement elemCreated)
	{
		//base.OnCreateElement(segment, layer, elemCreated);

		var elem	= elemCreated as LibSequentia.SnapshotElement;
		elem.CountUpScriptProgress();	// 진행 방향을 알 수 있도록
		elem.InitialState.SetNegativeProgress();
	}

	protected override void OnSetElement(LibSequentia.ScriptSegment segment, FSNSnapshot.Layer layer, LibSequentia.SnapshotElement elemToSet)
	{
		//base.OnSetElement(segment, layer, elemToSet);

		var elem	= elemToSet as LibSequentia.SnapshotElement;
		elem.CountUpScriptProgress();	// 진행 방향을 알 수 있도록
	}
}
