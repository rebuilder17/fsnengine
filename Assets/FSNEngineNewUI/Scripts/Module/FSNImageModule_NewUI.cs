using UnityEngine;
using System.Collections;
using UnityEngine.UI;


namespace LayerObjects
{
	public class Image_NewUI : ImageLayerObject
	{
		// Members

		RectTransform	m_rectTrans;
		RawImage		m_image;


		public Image_NewUI(FSNModule parent, GameObject gameObj, IInGameSetting setting)
			: base(parent, gameObj, setting)
		{
			m_image				= innerGO.AddComponent<RawImage>();	// 안쪽의 오브젝트에 추가하기
			m_rectTrans			= m_image.rectTransform;

			m_rectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0);
			m_rectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);
			m_rectTrans.pivot	= Vector2.one / 2f;

			var mat				= (parent as FSNImageModule_NewUI).imageMaterial;
			if (mat != null)										// 사용자 지정 마테리얼 설정
			{
				m_image.material= new Material(mat);
				m_image.material.renderQueue = 2000;	// 강제 렌더큐 설정 (Depth Write를 작동시키기 위해)
			}
		}

		public override void UpdatePivot(Vector2 pivot)
		{
			m_rectTrans.pivot	= pivot;
		}

		public override void UpdateTexture(Texture2D texture)
		{
			if (texture == null)
				return;

			m_image.texture		= texture;

			// texture 크기에 맞추기, Pivot 설정하기
			m_rectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, texture.width);
			m_rectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, texture.height);
		}

		public override void UpdateCombinedImage(FSNCombinedImage combimg)
		{
			if (combimg == null)
				return;

			var sprdata		= combimg.spriteData;
			
			var subsprites	= sprdata.subSprites;
			if (subsprites.Length > 0)								// 서브 스프라이트가 존재하는 경우
			{
				var sourceUVRect	= subsprites[0].sourceUVRect;
				var vSourceUV		= new Vector4()
				{
					x	= sourceUVRect.xMin,
					y	= sourceUVRect.yMin,
					z	= sourceUVRect.xMax,
					w	= sourceUVRect.yMax
				};

				var targetUVRect	= subsprites[0].targetUVRect;
				var vTargetUV		= new Vector4()
				{
					x	= targetUVRect.xMin,
					y	= targetUVRect.yMin,
					z	= targetUVRect.xMax,
					w	= targetUVRect.yMax
				};

				m_image.material	= new Material((ParentModule as FSNImageModule_NewUI).combinedImageMaterial);	// 조합 이미지 전용 마테리얼로 세팅
				m_image.material.renderQueue = 2000;	// 강제 렌더큐 설정 (Depth Write를 작동시키기 위해)
				//m_image.material	= new Material((ParentModule as FSNImageModule_NewUI).combinedImageMaterial);
				m_image.material.SetVector("_SubTexSourceUVs1", vSourceUV);							// 서브 이미지 UV값들 보내기
				m_image.material.SetVector("_SubTexTargetUVs1", vTargetUV);
			}
			else
			{														// 서브 스프라이트가 없을 땐 일반 마테리얼을 그대로 사용한다.

				m_image.material	= (ParentModule as FSNImageModule_NewUI).imageMaterial;
			}

			m_image.texture	= sprdata.texture;
			m_image.uvRect	= sprdata.baseUVRect;

			m_rectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, sprdata.pixelWidth);
			m_rectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, sprdata.pixelHeight);
		}

		//protected override void UpdatePosition(Vector3 position)
		//{
		//	m_rectTrans.localPosition	= position;
		//}

		//protected override void UpdateScale(Vector3 scale)
		//{
		//	m_rectTrans.localScale		= scale;
		//}

		//protected override void UpdateRotate(Vector3 rotate)
		//{
		//	m_rectTrans.localRotation	= Quaternion.Euler(rotate);
		//}

		protected override void UpdateColor(Color color)
		{
			m_image.color	= color;
		}
	}
}

public class FSNImageModule_NewUI : FSNImageModule<LayerObjects.Image_NewUI>
{
	[SerializeField]
	Material		m_imageMaterial	= null;		// 이미지에 특별히 사용할 마테리얼. 지정하지 않아도 된다 (기본값)

	public Material	combinedImageMaterial = null;	// 조합 이미지에 사용할 마테리얼.



	public Material imageMaterial
	{
		get { return m_imageMaterial; }
	}

	public override void Initialize()
	{
		FSNCombinedImage.InstallLoaders();	// 조합 이미지 관련 초기화
	}

	protected override LayerObjects.Image_NewUI MakeNewLayerObject(SnapshotElems.Image elem, IInGameSetting setting)
	{
		GameObject newObj	= new GameObject("Image_NewUI");
		newObj.layer		= gameObject.layer;
		var lobj			= new LayerObjects.Image_NewUI(this, newObj, setting);
		newObj.transform.SetParent(ObjectRoot, false);

		return lobj;
	}
}
