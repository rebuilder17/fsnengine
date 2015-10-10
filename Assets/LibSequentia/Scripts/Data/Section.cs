using UnityEngine;
using System.Collections;

namespace LibSequentia.Data
{
	/// <summary>
	/// Track을 구성하는 1개의 음악 시퀀스.
	/// </summary>
	public partial class Section
	{
		/// <summary>
		/// 시작될 때의 전환 방법 (fill-in 부분 재생 방법)
		/// </summary>
		public enum InType
		{
			KickIn,				// 음량 변화 없이 그대로 재생
			FadeIn,				// 음량을 점점 키운다
		}

		/// <summary>
		/// 끝날 때의 전환 방법
		/// </summary>
		public enum OutType
		{
			LeaveIt,			// 재생이 끝날 때까지 내버려둔다.
			FadeOut,			// 음량을 점점 줄인다
			Mute,				// 음량을 바로 0으로 줄인다
		}



		// Constants

		/// <summary>
		/// Section당 Layer 갯수
		/// </summary>
		public const int	c_maxLayerPerSection	= 4;


		// Members

		Layer []			m_layers	= new Layer[c_maxLayerPerSection];	// 레이어들

		float				m_tension;			// tension값

		/// <summary>
		/// tension값 설정
		/// </summary>
		public float tension
		{
			get { return m_tension; }
			set
			{
				m_tension	= value;
				for(int i = 0; i < c_maxLayerPerSection; i++)
				{
					var layer	= m_layers[i];
					if (layer != null)
					{
						layer.tension	= value;
					}
				}
			}
		}


		/// <summary>
		/// Fill-In 지점. beatStart 와 같거나 이보다 앞서야함
		/// </summary>
		public int beatFillIn { get; set; }

		/// <summary>
		/// beat 단위로 시작하는 지점
		/// </summary>
		public int beatStart { get; set; }

		/// <summary>
		/// beat 단위로 끝나는 지점
		/// </summary>
		public int beatEnd { get; set; }

		/// <summary>
		/// 루프 길이
		/// </summary>
		public int beatLoopLength
		{
			get
			{
				return beatEnd - beatStart;
			}
		}

		/// <summary>
		/// 자연 진행시 인트로 종류
		/// </summary>
		public InType inTypeNatural { get; set; }
		/// <summary>
		/// 강제 진행시 인트로 종류
		/// </summary>
		public InType inTypeManual { get; set; }

		/// <summary>
		/// 자연 진행시 아웃트로 종류
		/// </summary>
		public OutType outTypeNatural { get; set; }
		/// <summary>
		/// 강제 진행시 아웃트로 종류
		/// </summary>
		public OutType outTypeManual { get; set; }

		/// <summary>
		/// true일 시 Fill-In 구간을 이전 섹션 재생과 겹쳐서 재생하지 않는다. 즉 이전 섹션의 루프 종료 시점과 fill-in 시작 시점을 일치하게 재생한다.
		/// false일 경우 이전 섹션의 루프 종료 시점은 이 섹션의 루프 시작과 맞춘다. (fill-in이 앞당겨진다.)
		/// </summary>
		public bool doNotOverlapFillIn { get; set; }



		/// <summary>
		/// Layer 구하기
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public Layer GetLayer(int index)
		{
			return m_layers[index];
		}




		//

		public static Section CreateFromJSON(JSONObject json, IAudioClipPack clipPack)
		{
			var section	= new Section();

			section.inTypeNatural	= Engine.Utils.parseEnum<InType>(json.GetField("natural-in").str);
			section.inTypeManual	= Engine.Utils.parseEnum<InType>(json.GetField("manual-in").str);
			section.outTypeNatural	= Engine.Utils.parseEnum<OutType>(json.GetField("natural-out").str);
			section.outTypeManual	= Engine.Utils.parseEnum<OutType>(json.GetField("manual-out").str);

			section.beatFillIn		= (int)json.GetField("beat-fillin").n;
			section.beatStart		= (int)json.GetField("beat-start").n;
			section.beatEnd			= (int)json.GetField("beat-end").n;

			json.GetField("layers", (layerarr) =>
				{
					int layeridx	= 0;
					foreach(var layer in layerarr.list)
					{
						section.m_layers[layeridx]	= Layer.CreateFromJSON(layer, clipPack);
						layeridx++;
					}
				});

			return section;
		}
	}
}

