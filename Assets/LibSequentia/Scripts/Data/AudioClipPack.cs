using UnityEngine;
using System.Collections;

namespace LibSequentia.Data
{
	/// <summary>
	/// AudioClip 에 대한 핸들. (get / return 을 명시하기 위함)
	/// </summary>
	public interface IAudioClipHandle
	{
		/// <summary>
		/// 오디오 클립 얻기. 핸들 당 Acquire는 동시에 1번까지만 가능하다. (다른 데서 가용하려면 Return해야함)
		/// </summary>
		object Acquire();
		/// <summary>
		/// 오디오 클립 반환
		/// </summary>
		void Return();
	}

	/// <summary>
	/// 로딩한 AudioClip의 레퍼런스를 보관해두는 객체
	/// </summary>
	public interface IAudioClipPack
	{
		/// <summary>
		/// 오디오 클립 핸들 얻어오기
		/// </summary>
		/// <param name="clipname"></param>
		/// <returns></returns>
		IAudioClipHandle GetHandle(string clipname);
	}

	//public class AudioClipPack : IAudioClipPack
	//{

	//}
}
