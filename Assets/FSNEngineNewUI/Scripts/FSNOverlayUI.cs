using UnityEngine;
using UnityEngine.UI;
using System.Collections;


/// <summary>
/// 오버레이 UI를 관리하는 컴포넌트
/// </summary>
[RequireComponent(typeof(Canvas))]
public class FSNOverlayUI : MonoBehaviour, IFSNSwipeHandler
{
	/// <summary>
	/// 없는 방향으로 swipe 시도, 시작
	/// </summary>
	/// <param name="direction"></param>
	public void OnTryingSwipeToWrongDirection(FSNInGameSetting.FlowDirection direction)
	{

	}

	/// <summary>
	/// 없는 방향으로 swipe 시도하다가 중단함
	/// </summary>
	/// <param name="direction"></param>
	public void OnReleaseSwipeToWrongDirection(FSNInGameSetting.FlowDirection direction)
	{

	}

	/// <summary>
	/// 있는 방향으로 swipe 시도, 시작
	/// </summary>
	/// <param name="direction"></param>
	public void OnTryingSwipe(FSNInGameSetting.FlowDirection direction)
	{

	}

	/// <summary>
	/// 있는 방향으로 swipe 시도하다가 중단함
	/// </summary>
	/// <param name="direction"></param>
	public void OnReleaseSwipe(FSNInGameSetting.FlowDirection direction)
	{

	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="direction"></param>
	public void OnCompleteSwipe(FSNInGameSetting.FlowDirection direction)
	{

	}
}
