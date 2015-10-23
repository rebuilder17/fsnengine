using UnityEngine;
using System.Collections;

/// <summary>
/// FSN 스크립트로 오브젝트에 부착 가능한 컴포넌트. 상속해서 필요한 컴포넌트를 구현한다.
/// </summary>
public abstract class FSNBaseComponent : MonoBehaviour
{
	/// <summary>
	/// 파라미터 변화시 호출
	/// </summary>
	/// <param name="param"></param>
	public virtual void OnParameterChange(string param)
	{
		Debug.Log("parameter change : " + param);
	}
}
