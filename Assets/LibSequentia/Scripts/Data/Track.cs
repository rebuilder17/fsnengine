using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LibSequentia.Data
{
	/// <summary>
	/// Section의 시퀀스. 곡 하나에 해당
	/// </summary>
	public partial class Track
	{
		// Members

		List<Section>		m_sectionSeq	= new List<Section>();			// section의 시퀀스
		IAudioClipPack		m_clipPack;										// 이 track에서 사용하는 오디오 클립 묶음


		/// <summary>
		/// BPM.
		/// </summary>
		public float BPM { get; set; }

		/// <summary>
		/// section 갯수
		/// </summary>
		public int sectionCount
		{
			get
			{
				return m_sectionSeq.Count;
			}
		}



		/// <summary>
		/// AudioClipPack 붙이기
		/// </summary>
		/// <param name="pack"></param>
		public void SetClipPack(IAudioClipPack pack)
		{
			m_clipPack = pack;
		}

		public Section GetSection(int index)
		{
			return m_sectionSeq[index];
		}


		//

		/// <summary>
		/// 필요한 오디오클립 이름들을 모두 가져온다.
		/// </summary>
		/// <param name="json"></param>
		/// <returns></returns>
		public static string[] GatherRequiredClips(JSONObject json)
		{
			HashSet<string> clipnames	= new HashSet<string>();
			json.GetField("sections", (sectionarr) =>
				{
					foreach (var section in sectionarr.list)
					{
						section.GetField("layers", (layerarr) =>
						{
							foreach (var layer in layerarr.list)
							{
								clipnames.Add(layer.GetField("clip").str);
							}
						});
					}
				});

			var arr	= new string[clipnames.Count];
			clipnames.CopyTo(arr);
			return arr;
		}

		/// <summary>
		/// JSON 객체를 통해 생성
		/// </summary>
		/// <param name="json"></param>
		/// <returns></returns>
		public static Track CreateFromJSON(JSONObject json, IAudioClipPack clipPack)
		{
			var track	= new Track();
			track.SetClipPack(clipPack);

			track.BPM	= json.GetField("bpm").f;

			json.GetField("sections", (sectionarr) =>
				{
					var list	= sectionarr.list;
					var count	= list.Count;
					for (int i = 0; i < count; i++)
					{
						var newsec	= Section.CreateFromJSON(list[i], track.m_clipPack);
						track.m_sectionSeq.Add(newsec);
					}
				});

			return track;
		}
	}
}