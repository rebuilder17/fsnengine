using UnityEngine;
using System.Collections;


namespace LayerObjects
{
	/// <summary>
	/// Text 오브젝트에 해당하는 LayerObject
	/// </summary>
	public abstract class TextLayerObject : FSNLayerObject
	{

	}
}


/// <summary>
/// 텍스트 모듈, 기본형
/// </summary>
public abstract class FSNTextModule<ObjT> : FSNProcessModule<Segments.TextSegment, ObjT>
	where ObjT : FSNLayerObject
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
