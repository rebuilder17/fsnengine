using UnityEngine;
using System.Collections;


namespace LayerObjects
{
	/// <summary>
	/// Image 계통 레이어 오브젝트 기반
	/// </summary>
	/// <typeparam name="ImageElem"></typeparam>
	public abstract class BaseImageLayerObject<ImageElem> : FSNLayerObject<ImageElem>
		where ImageElem : SnapshotElems.ImageObjBase
	{
		
		/// <summary>
		/// 오브젝트의 움직임 계산 상태
		/// </summary>
		public enum State
		{
			MotionKey,			// 키 오브젝트. 이 오브젝트의 좌표값을 기준으로 움직임 보간 실행
			Calculated,			// 보간 적용됨
			NotCalculated,		// 아직 보간 계산 적용되지 않음.
		}

		/// <summary>
		/// 움직임 상태
		/// </summary>
		public State	motionState;

		public BaseImageLayerObject(FSNModule parent, GameObject gameObj)
			: base(parent, gameObj)
		{

		}
	}
}

/// <summary>
/// 이미지 모듈 베이스. 이미지와 비슷한 동작을 하는 모든 모듈의 부모 클래스로 사용 가능
/// </summary>
public abstract class FSNBaseImageModule<SegT, ImageT, ObjT> : FSNProcessModule<SegT, ImageT, ObjT>
	where SegT : Segments.Object
	where ImageT : SnapshotElems.ImageObjBase
	where ObjT : LayerObjects.BaseImageLayerObject<ImageT>
	
{
	public override FSNSnapshot.Layer GenerateNextLayerImage(FSNSnapshot.Layer curLayer, params FSNProcessModuleCallParam[] callParams)
	{
		FSNSnapshot.Layer newLayer	= curLayer.Clone();

		foreach(var callParam in callParams)
		{

		}

		// call 처리 이후, snapshot 마무리

		return newLayer;
	}
}
