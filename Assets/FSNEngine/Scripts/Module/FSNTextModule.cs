using UnityEngine;
using System.Collections;


namespace LayerObjects
{
	/// <summary>
	/// Text 오브젝트에 해당하는 LayerObject
	/// </summary>
	public abstract class TextLayerObject : FSNLayerObject<SnapshotElems.Text>
	{
		public enum TextAlign
		{
			Left,
			Center,
			Right,
		}

		//=========================================================================


		string			m_text	= "";
		/// <summary>
		/// 텍스트
		/// </summary>
		public string Text
		{
			get
			{
				return m_text;
			}
			set
			{
				m_text	= value == null? "" : value;
				UpdateText(m_text);
			}
		}
		/// <summary>
		/// 정렬
		/// </summary>
		public TextAlign Align	{get;set;}


		public TextLayerObject(FSNModule parent, GameObject gameObj)
			: base(parent, gameObj)
		{

		}

		/// <summary>
		/// 텍스트 업데이트
		/// </summary>
		/// <param name="newText"></param>
		protected abstract void UpdateText(string newText);

		public override void SetStateFully(SnapshotElems.Text to)
		{
			base.SetStateFully(to);

			Text	= to.text;
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
		m_layerID	= (int)FSNSnapshot.PreDefinedLayers.Text;		// 레이어 ID 강제지정
	}


}
