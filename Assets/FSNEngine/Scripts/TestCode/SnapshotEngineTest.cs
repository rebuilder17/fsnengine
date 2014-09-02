using UnityEngine;
using System.Collections;

public class SnapshotEngineTest : MonoBehaviour
{
	FSNSequenceEngine m_seqEngine;

	void Start()
	{
		m_seqEngine	= GetComponent<FSNSequenceEngine>();

		m_seqEngine.StartSnapshotSequence(FSNSnapshotSequence.GenerateTestSequence());
	}

	void Update()
	{
		if(Input.GetKeyDown(KeyCode.DownArrow))
		{
			m_seqEngine.FullSwipe(FSNInGameSetting.FlowDirection.Down);
		}
		else if(Input.GetKeyDown(KeyCode.UpArrow))
		{
			m_seqEngine.FullSwipe(FSNInGameSetting.FlowDirection.Up);
		}
	}
}
