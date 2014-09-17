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
		sshot_start.BackDirection		= FSNInGameSetting.FlowDirection.Up;
		sshot							= new FSNSnapshot();
		sshot_start.snapshot			= sshot;
		sshot.InGameSetting				= new FSNInGameSetting();
		sshot.InGameSetting.CurrentFlowDirection	= sshot_start.FlowDirection;
		sshot.InGameSetting.BackwardFlowDirection	= sshot_start.BackDirection;
		//

		// 첫번재 스냅샷
		var sshot_01					= new Segment();
		sequence.Add(sshot_01);
		sshot_01.Type					= FlowType.Normal;
		sshot_01.FlowDirection			= FSNInGameSetting.FlowDirection.Down;
		sshot_01.BackDirection			= FSNInGameSetting.FlowDirection.Up;
		sshot							= new FSNSnapshot();
		sshot_01.snapshot				= sshot;
		sshot.InGameSetting				= new FSNInGameSetting();
		sshot.InGameSetting.CurrentFlowDirection	= sshot_01.FlowDirection;
		sshot.InGameSetting.BackwardFlowDirection	= sshot_01.BackDirection;

		var text01_0					= new SnapshotElems.Text();
		text01_0.text					= "문자열 테스트 01";
		text01_0.fontSize				= 16;
		text01_0.Position				= new Vector3(0, 300);
		text01_0.Color					= Color.white;
		text01_0.Alpha					= 1;
		text01_0.TransitionTime			= 1;
		text01_0.MakeItUnique();

		text01_0.InitialState.Alpha		= 0;
		text01_0.InitialState.Position	= new Vector3(0, 400);

		text01_0.FinalState.Alpha		= 0;
		text01_0.FinalState.Position	= new Vector3(600, 200);

		textLayer						= sshot.MakeNewLayer((int)FSNSnapshot.PreDefinedLayers.Text);
		textLayer.AddElement(text01_0);
		// 

		// 두번째 스냅샷 (분기 지점)
		var sshot_02					= new Segment();
		sequence.Add(sshot_02);
		sshot_02.Type					= FlowType.Normal;
		sshot_02.FlowDirection			= FSNInGameSetting.FlowDirection.Left;
		sshot_02.BackDirection			= FSNInGameSetting.FlowDirection.Up;
		sshot							= new FSNSnapshot();
		sshot_02.snapshot				= sshot;
		sshot.InGameSetting				= new FSNInGameSetting();
		sshot.InGameSetting.CurrentFlowDirection	= sshot_02.FlowDirection;
		sshot.InGameSetting.BackwardFlowDirection	= sshot_02.BackDirection;

		var text01_1					= text01_0.Clone();
		text01_1.Position				= new Vector3(0, 200);
		text01_1.Alpha					= 0.5f;

		textLayer						= sshot.MakeNewLayer((int)FSNSnapshot.PreDefinedLayers.Text);
		textLayer.AddElement(text01_1);
		// 

		// 세번째 스냅샷
		var sshot_03					= new Segment();
		sequence.Add(sshot_03);
		sshot_03.Type					= FlowType.Normal;
		sshot_03.FlowDirection			= FSNInGameSetting.FlowDirection.Left;
		sshot_03.BackDirection			= FSNInGameSetting.FlowDirection.Right;
		sshot							= new FSNSnapshot();
		sshot_03.snapshot				= sshot;
		sshot.InGameSetting				= new FSNInGameSetting();
		sshot.InGameSetting.CurrentFlowDirection	= sshot_03.FlowDirection;
		sshot.InGameSetting.BackwardFlowDirection	= sshot_03.BackDirection;

		var text01_2					= text01_0.Clone();
		text01_2.Position				= new Vector3(200, 200);
		text01_2.Alpha					= 0.25f;

		var text02_0					= new SnapshotElems.Text();
		text02_0.text					= "으오와아아아앙";
		text02_0.fontSize				= 32;
		text02_0.Position				= new Vector3(50, 200);
		text02_0.Color					= Color.white;
		text02_0.Alpha					= 1;
		text02_0.TransitionTime			= 1;
		text02_0.MakeItUnique();
		
		text02_0.InitialState.Alpha		= 0;
		text02_0.InitialState.Position	= new Vector3(-50, 200);
		 
		text02_0.FinalState.Alpha		= 0;
		text02_0.FinalState.Position	= new Vector3(600, 200);

		textLayer						= sshot.MakeNewLayer((int)FSNSnapshot.PreDefinedLayers.Text);
		textLayer.AddElement(text01_2);
		textLayer.AddElement(text02_0);
		// 
		
		// 종료 세그먼트 (비어있음)
		var sshot_end					= new Segment();
		sequence.Add(sshot_end);
		sshot_end.Type					= FlowType.Normal;
		sshot_end.FlowDirection			= FSNInGameSetting.FlowDirection.Left;
		sshot_end.BackDirection			= FSNInGameSetting.FlowDirection.Right;
		sshot							= new FSNSnapshot();
		sshot_end.snapshot				= sshot;
		sshot.InGameSetting				= new FSNInGameSetting();
		sshot.InGameSetting.CurrentFlowDirection	= sshot_end.FlowDirection;
		sshot.InGameSetting.BackwardFlowDirection	= sshot_end.BackDirection;
		//

		// 분기 첫번째 스냅샷
		var sshot_11					= new Segment();
		sequence.Add(sshot_03);
		sshot_11.Type					= FlowType.Normal;
		sshot_11.FlowDirection			= FSNInGameSetting.FlowDirection.Down;
		sshot_11.BackDirection			= FSNInGameSetting.FlowDirection.Up;
		sshot							= new FSNSnapshot();
		sshot_11.snapshot				= sshot;
		sshot.InGameSetting				= new FSNInGameSetting();
		sshot.InGameSetting.CurrentFlowDirection	= sshot_11.FlowDirection;
		sshot.InGameSetting.BackwardFlowDirection	= sshot_11.BackDirection;

		var text01_3					= text01_0.Clone(true);
		text01_3.Position				= new Vector3(0, 100);
		text01_3.Alpha					= 0.25f;

		text01_3.FinalState.Position	= new Vector3(0, 0);

		//var text02_0					= new SnapshotElems.Text();
		//text02_0.text					= "TestString02";
		//text02_0.Position				= new Vector3(50, 200);
		//text02_0.Color					= Color.white;
		//text02_0.Alpha					= 1;
		//text02_0.TransitionTime			= 1;
		//text02_0.MakeItUnique();

		//text02_0.InitialState.Alpha		= 0;
		//text02_0.InitialState.Position	= new Vector3(-50, 200);

		//text02_0.FinalState.Alpha		= 0;
		//text02_0.FinalState.Position	= new Vector3(600, 200);

		textLayer						= sshot.MakeNewLayer((int)FSNSnapshot.PreDefinedLayers.Text);
		textLayer.AddElement(text01_3);
		//textLayer.AddElement(text02_0);
		// 

		// 종료 세그먼트 (비어있음)
		var sshot_end2					= new Segment();
		sequence.Add(sshot_end2);
		sshot_end2.Type					= FlowType.Normal;
		sshot_end2.FlowDirection		= FSNInGameSetting.FlowDirection.Down;
		sshot_end2.BackDirection		= FSNInGameSetting.FlowDirection.Up;
		sshot							= new FSNSnapshot();
		sshot_end2.snapshot				= sshot;
		sshot.InGameSetting				= new FSNInGameSetting();
		sshot.InGameSetting.CurrentFlowDirection	= sshot_end2.FlowDirection;
		sshot.InGameSetting.BackwardFlowDirection	= sshot_end2.BackDirection;
		//


		// Flow 설정

		sshot_start	.SetFlow(FSNInGameSetting.FlowDirection.Down, new Segment.FlowInfo() { Linked = sshot_01 });
		sshot_01	.SetFlow(FSNInGameSetting.FlowDirection.Down, new Segment.FlowInfo() { Linked = sshot_02 });
		sshot_02	.SetFlow(FSNInGameSetting.FlowDirection.Left, new Segment.FlowInfo() { Linked = sshot_03 });
		sshot_03	.SetFlow(FSNInGameSetting.FlowDirection.Left, new Segment.FlowInfo() { Linked = sshot_end });

		sshot_02	.SetFlow(FSNInGameSetting.FlowDirection.Down, new Segment.FlowInfo() { Linked = sshot_11 });
		sshot_11	.SetFlow(FSNInGameSetting.FlowDirection.Down, new Segment.FlowInfo() { Linked = sshot_end2 });

		sshot_01	.SetFlow(FSNInGameSetting.FlowDirection.Up, new Segment.FlowInfo() { Linked = sshot_start });
		sshot_02	.SetFlow(FSNInGameSetting.FlowDirection.Up, new Segment.FlowInfo() { Linked = sshot_01 });
		sshot_03	.SetFlow(FSNInGameSetting.FlowDirection.Right, new Segment.FlowInfo() { Linked = sshot_02 });
		sshot_end	.SetFlow(FSNInGameSetting.FlowDirection.Right, new Segment.FlowInfo() { Linked = sshot_03 });

		sshot_11	.SetFlow(FSNInGameSetting.FlowDirection.Up, new Segment.FlowInfo() { Linked = sshot_02 });
		sshot_end2	.SetFlow(FSNInGameSetting.FlowDirection.Up, new Segment.FlowInfo() { Linked = sshot_11 });

		return sequence;
	}
}