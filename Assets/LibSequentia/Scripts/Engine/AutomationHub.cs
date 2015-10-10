using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LibSequentia.Engine
{
	/// <summary>
	/// AutomationHub을 관리하는 클래스에 대한 인터페이스
	/// </summary>
	public interface IAutomationHubManager
	{
		void RegisterAutomationHub(IAutomationHubHandle hub);
	}

	/// <summary>
	/// AutomationHub에 대한 핸들. IAutomationHubManager에서만 사용한다.
	/// </summary>
	public interface IAutomationHubHandle
	{
		AutomationHub objref { get; }
		void Update();
	}

	/// <summary>
	/// 여러 오토메이션 인풋을 받아 합성한 뒤 여러 갈래의 오토메이션 아웃풋으로 (매 프레임마다) 내보내는 클래스.
	/// </summary>
	public class AutomationHub
	{
		class AutomationHubHandle : IAutomationHubHandle
		{
			AutomationHub m_obj;

			/// <summary>
			/// Hub오브젝트 자체에 대한 레퍼런스
			/// </summary>
			public AutomationHub objref { get { return m_obj; } }

			public AutomationHubHandle(AutomationHub obj)
			{
				m_obj = obj;
			}

			public void Update()
			{
				m_obj.Update();
			}
		}

		/// <summary>
		/// 오토메이션 인풋을 받기 위한 인터페이스
		/// </summary>
		class Chain : Data.IAutomationControl
		{
			public Dictionary<Data.Automation.TargetParam, float>	m_params;		// 파라미터값이 보관되는 딕셔너리
			public bool	m_dirty	= false;											// 변경됨 여부

			public void Set(Data.Automation.TargetParam param, float value)
			{
				m_params[param]	= value;
				m_dirty = true;
			}

			public Chain()
			{
				// 딕셔너리를 기본값으로 채운다.
				m_params	= new Dictionary<Data.Automation.TargetParam, float>();
				var enums	= Data.Automation.targetParamEnumValues;
				foreach(var param in enums)
				{
					m_params[param]	= Data.Automation.GetDefaultValue(param);
				}
			}
		}


		// Members

		List<Data.IAutomationControl>	m_targets	= new List<Data.IAutomationControl>();	// 오토메이션 타겟 목록
		List<Chain>						m_chains	= new List<Chain>();					// 체인 목록

		
		public AutomationHub(IAutomationHubManager manager)
		{
			manager.RegisterAutomationHub(new AutomationHubHandle(this));
		}


		/// <summary>
		/// chain 생성
		/// </summary>
		/// <param name="count"></param>
		public void CreateChains(int count)
		{
			m_chains.Clear();
			for(int i = 0; i < count; i++)
			{
				m_chains.Add(new Chain());
			}
		}

		/// <summary>
		/// 생성되어있는 chain 가져오기
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public Data.IAutomationControl GetChain(int index)
		{
			return m_chains[index];
		}

		/// <summary>
		/// 아웃풋 설정
		/// </summary>
		/// <param name="targets"></param>
		public void SetOutputs(params Data.IAutomationControl [] targets)
		{
			m_targets.Clear();
			m_targets.AddRange(targets);
		}

		private void Update()
		{
			bool dirty		= false;
			int chainCount	= m_chains.Count;
			for (int i = 0; i < chainCount; i++)	// 값이 변경된 chain이 있는지 체크한다.
			{
				var chain	= m_chains[i];
				if (chain.m_dirty)
				{
					dirty	= true;
					chain.m_dirty = false;
				}
			}

			if(dirty)											// 실제로 변경된 chain이 있을 때만 업데이트
			{
				int paramCount	= Data.Automation.targetParamEnumValues.Length;
				for(int pi = 0; pi < paramCount; pi++)
				{
					var param	= Data.Automation.targetParamEnumValues[pi];
					var value	= 1.0f;
					for(int i = 0; i < chainCount; i++)			// 각 chain의 파라미터값 합산
					{
						value	*= m_chains[i].m_params[param];
					}

					int outCount	= m_targets.Count;
					for (int i = 0; i < outCount; i++)			// 계산한 해당 파라미터값을 출력
					{
						m_targets[i].Set(param, value);
					}
				}
			}
		}
	}
}
