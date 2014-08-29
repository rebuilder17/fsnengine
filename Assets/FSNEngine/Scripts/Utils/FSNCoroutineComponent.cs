using UnityEngine;
using System.Collections;


/// <summary>
/// 오직 코루틴을 돌리기 위한 컴포넌트
/// </summary>
public sealed class FSNCoroutineComponent : MonoBehaviour
{
	/// <summary>
	/// 게임 오브젝트에 부착된 FSNCoroutineComponent를 가져옴. 없을 시 생성
	/// </summary>
	/// <param name="obj"></param>
	/// <returns></returns>
	public static FSNCoroutineComponent GetFromGameObject(GameObject obj)
	{
		var comp	= obj.GetComponent<FSNCoroutineComponent>();
		if(comp == null)
			comp	= obj.AddComponent<FSNCoroutineComponent>();

		return comp;
	}
}
