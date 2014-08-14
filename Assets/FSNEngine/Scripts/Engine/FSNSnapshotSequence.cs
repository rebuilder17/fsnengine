using UnityEngine;
using System.Collections;




/// <summary>
/// 스냅샷 리스트. 컨트롤을 위한 요소들도 같이 시퀀스에 포함되어있음.
/// </summary>
public class FSNSnapshotSequence
{
	/// <summary>
	/// 리스트 단위. 스냅샷 1개에 부가적인 컨트롤 명령어가 붙어있는 형태.
	/// </summary>
	class Segment
	{
		FSNSnapshot snapshot;

		// TODO : 컨트롤 명령어
	}


	//====================================================================






	//====================================================================

	/// <summary>
	/// FSNSnapshotSequence 를 스크립트 시퀀스에서 생성해낸다
	/// </summary>
	public static class Builder
	{

	}

	/// <summary>
	/// 스크립트 순회. 엔진에서 실행할 때 이 인스턴스를 사용한다.
	/// </summary>
	public class Traveler
	{

	}
}

