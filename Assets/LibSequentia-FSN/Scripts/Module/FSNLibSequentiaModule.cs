using UnityEngine;
using System.Collections;




namespace LibSequentia
{
	/// <summary>
	/// LibSequentia - 스크립트 세그먼트
	/// </summary>
	class ScriptSegment : Segments.Object
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
	class SnapshotElement : SnapshotElems.ObjectBase<SnapshotElement>
	{
		public float Tension { get; set; }
		public float Transition { get; set; }

		float	m_progress	= 0;		// 진행 방향을 알기 위한 내부 변수. 세팅할 때마다 1씩 더해줘야 한다.

		public float scriptProgress
		{
			get { return m_progress; }
		}
		public void CountUpScriptProgress()
		{
			m_progress += 1;
		}

		public override void CopyDataTo(SnapshotElement to)
		{
			base.CopyDataTo(to);
			Tension		= to.Tension;
			Transition	= to.Transition;

			m_progress	= to.m_progress;
		}

		public override void LerpBetweenElems(SnapshotElems.ObjectBase<SnapshotElement> elem1, SnapshotElems.ObjectBase<SnapshotElement> elem2, float t)
		{
			// NOTE : 다른 요소들은 lerp 할 필요가 없음 (사용하지 않으므로)
			//base.LerpBetweenElems(elem1, elem2, t);

			var e1		= elem1 as SnapshotElement;
			var e2		= elem2 as SnapshotElement;
			Tension		= Mathf.Lerp(e1.Tension, e2.Tension, t);
			Transition	= Mathf.Lerp(e1.Transition, e2.Transition, t);
			m_progress	= Mathf.Lerp(e1.m_progress, e2.m_progress, t);
		}
	}

	class LayerObject : LayerObjects.BaseObjectLayerObject<SnapshotElement>
	{
		float		m_tension;
		float		m_songTrans;
		float		m_progress;
		bool		m_reverse;


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
			// 파싱해서 LibSequentia의 메세지로 보낸다.
			// 형식 : 현재 트랙 경로, 현재 트랙 스텝[, 새 트랙 경로, 새 트랙 스텝]

			var lsengine	= LibSequentiaMain.instance;

			var split	= to.Split(',');
			if (split.Length == 2)				// 일반 재생
			{
				var curtrack	= split[0].Trim();
				var cutstep		= int.Parse(split[1].Trim());

				if (!lsengine.isPlaying)		// 재생중이 아닐 때는 트랙 새로 올리기
				{
					
				}
				else
				{

				}
			}
			else if(split.Length >= 4)			// 트랙 전환 재생
			{
				var curtrack	= split[0].Trim();
				var cutstep		= int.Parse(split[1].Trim());
				var newtrack	= split[2].Trim();
				var newstep		= int.Parse(split[3].Trim());

				if (!lsengine.isPlaying)		// 재생중이 아닐 때는 트랙 새로 올리기
				{
					
				}
				else
				{

				}
			}
			else
			{
				// Error
			}
		}

		void UpdateProgressAndReverse(float prog)
		{
			if (m_progress > prog)
				m_reverse	= true;
			else if (m_progress < prog)
				m_reverse	= false;

			m_progress	= prog;
		}

		public override void SetStateFully(SnapshotElement to)
		{
			base.SetStateFully(to);
			Tension			= to.Tension;
			SongTransition	= to.Transition;
			UpdateProgressAndReverse(to.scriptProgress);
		}

		public override void TransitionWith(SnapshotElement to, float ratio)
		{
			base.TransitionWith(to, ratio);
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
		public static void Install()
		{
			FSNScriptSequence.Parser.AddCommand(Preload, "lspreload", "음악프리로드");
			FSNScriptSequence.Parser.AddCommand(Ready, "lsready", "음악시스템사용");
		}


		/// <summary>
		/// 프리로드 명령어
		/// </summary>
		/// <param name="protocol"></param>
		static void Preload(FSNScriptSequence.Parser.ICommandGenerateProtocol protocol)
		{

		}

		/// <summary>
		/// 스크립트 안에서 사용하기 전에 호출해야함.
		/// 실제로는 레이어에 LibSequentia를 컨트롤할 오브젝트를 생성하는 역할을 한다.
		/// </summary>
		/// <param name="protocol"></param>
		static void Ready(FSNScriptSequence.Parser.ICommandGenerateProtocol protocol)
		{

		}
	}
}

public class FSNLibSequentiaModule : MonoBehaviour
{

}
