using UnityEngine;
using System.Collections;


// 테스트용 FSNSnapshotSequence 생성 코드

public partial class FSNSnapshotSequence
{
	public static FSNSnapshotSequence GenerateTestSequence()
	{
		var sequence	= new FSNSnapshotSequence();
		FSNSnapshot sshot;
		FSNSnapshot.Layer textLayer;
		//

		// 시작 세그먼트 (비어있음)
		var sshot_start					= new Segment();
		sequence.Add(sshot_start);
		sshot_start.Type				= FlowType.Normal;
		sshot_start.FlowDirection		= FSNInGameSetting.FlowDirection.Down;
		sshot							= new FSNSnapshot();
		sshot_start.snapshot			= sshot;
		//

		// 첫번재 스냅샷
		var sshot_01					= new Segment();
		sequence.Add(sshot_01);
		sshot_01.Type					= FlowType.Normal;
		sshot_01.FlowDirection			= FSNInGameSetting.FlowDirection.Down;
		sshot							= new FSNSnapshot();
		sshot_01.snapshot				= sshot;

		var text01_0					= new SnapshotElems.Text();
		text01_0.text					= "TestString01";
		text01_0.Position				= new Vector3(0, 300);
		text01_0.Color					= Color.white;
		text01_0.Alpha					= 1;
		text01_0.TransitionTime			= 1;
		text01_0.MakeItUnique();

		text01_0.InitialState.Alpha		= 0;
		text01_0.InitialState.Position	= new Vector3(0, 400);

		text01_0.FinalState.Alpha		= 0;
		text01_0.FinalState.Position	= new Vector3(0, 0);

		textLayer						= sshot.MakeNewLayer((int)FSNSnapshot.PreDefinedLayers.Text);
		textLayer.AddElement(text01_0);
		// 

		// 두번째 스냅샷
		var sshot_02					= new Segment();
		sequence.Add(sshot_02);
		sshot_02.Type					= FlowType.Normal;
		sshot_02.FlowDirection			= FSNInGameSetting.FlowDirection.Down;
		sshot							= new FSNSnapshot();
		sshot_02.snapshot				= sshot;

		var text01_1					= text01_0.Clone();
		text01_1.Position				= new Vector3(0, 200);
		text01_1.Alpha					= 0.5f;

		textLayer						= sshot.MakeNewLayer((int)FSNSnapshot.PreDefinedLayers.Text);
		textLayer.AddElement(text01_1);
		// 
		
		// 종료 세그먼트 (비어있음)
		var sshot_end					= new Segment();
		sequence.Add(sshot_end);
		sshot_end.Type					= FlowType.Normal;
		sshot_end.FlowDirection			= FSNInGameSetting.FlowDirection.Down;
		sshot							= new FSNSnapshot();
		sshot_end.snapshot				= sshot;
		//



		// Flow 설정

		sshot_start	.SetFlow(FSNInGameSetting.FlowDirection.Down, new Segment.FlowInfo() { Linked = sshot_01 });
		sshot_01	.SetFlow(FSNInGameSetting.FlowDirection.Down, new Segment.FlowInfo() { Linked = sshot_02 });
		sshot_02	.SetFlow(FSNInGameSetting.FlowDirection.Down, new Segment.FlowInfo() { Linked = sshot_end });

		return sequence;
	}
}