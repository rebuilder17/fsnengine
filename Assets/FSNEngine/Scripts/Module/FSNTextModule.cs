using UnityEngine;
using System.Collections;


namespace LayerObjects
{
	/// <summary>
	/// Text 오브젝트에 해당하는 LayerObject
	/// </summary>
	public abstract class TextLayerObject : FSNLayerObject<SnapshotElems.Text>
	{
		public TextLayerObject(FSNModule parent, GameObject gameObj)
			: base(parent, gameObj)
		{

		}
	}
}


/// <summary>
/// 텍스트 모듈, 기본형
/// </summary>
public abstract class FSNTextModule<ObjT> : FSNProcessModule<Segments.TextSegment, SnapshotElems.Text, ObjT>
	where ObjT : LayerObjects.TextLayerObject
{
	public override string ModuleName
	{
		get { return FSNEngine.ModuleType.Text.ToString(); }
	}


	//==================================================================

	public override void Initialize()
	{
		// 
	}


}
