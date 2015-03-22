using UnityEngine;
using System.Collections;


/// <summary>
/// 스크립트로 생성하는 객체(GameObject) 에 부착하여 이벤트 처리를 할 수 있게 하는 클래스(컴포넌트)
/// 프리팹에 부착하지 않아도 문제는 없으나, 필요하다면 이 클래스를 상속하여 필요한 기능을 구현한 뒤 컴포넌트로 부착할 것
/// </summary>
public abstract class FSNBaseGameObjectEventListener : MonoBehaviour
{
	/// <summary>
	/// 색상 업데이트 (알파 포함)
	/// </summary>
	/// <param name="color"></param>
	public virtual void OnUpdateColor(Color color) { }
}
