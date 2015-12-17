using UnityEngine;
using System.Collections;
using UnityEngine.UI;


namespace LayerObjects
{
	public class Image_NewUI : ImageLayerObject
	{
		// Members

		RectTransform			m_rectTrans;
		RawImage				m_image;
		//CanvasRenderer			m_canvasRenderer;
		//Renderer				m_renderer;
		//MaterialPropertyBlock	m_matPropBlock	= null;
		float					m_adaptPersScale	= 1.0f;
		Canvas					m_childCanvas;
		RectTransform			m_canvasTrans;

		FSNImageModule_NewUI m_parentImageModule;


		public Image_NewUI(FSNModule parent, GameObject gameObj, IInGameSetting setting)
			: base(parent, gameObj, setting)
		{
			m_image				= innerGO.AddComponent<RawImage>();	// 안쪽의 오브젝트에 추가하기
			//m_canvasRenderer	= m_image.canvasRenderer;
			//m_renderer			= m_image.GetComponent<Renderer>();
			//m_image				= gameObject.AddComponent<RawImage>();	// 안쪽의 오브젝트에 추가하기
			m_rectTrans			= m_image.rectTransform;

			m_rectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0);
			m_rectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);
			m_rectTrans.pivot	= Vector2.one / 2f;

			m_parentImageModule	= parent as FSNImageModule_NewUI;
			var mat				= m_parentImageModule.imageMaterial;
			if (mat != null)										// 사용자 지정 마테리얼 설정
			{
				//m_image.material= new Material(mat);
				m_image.material	= mat;
				m_image.material.renderQueue = 2450;	// 강제 렌더큐 설정 (Depth Write를 작동시키기 위해)
			}

			m_childCanvas			= gameObj.AddComponent<Canvas>();
			gameObj.AddComponent<CanvasRenderer>(); // FIX : Canvas 가 움직이지 않던 문제 해결. 버그인가봄....
			m_childCanvas.overrideSorting	= true;
			m_canvasTrans			= gameObj.GetComponent<RectTransform>();

			//var dummyrenderer		= gameObj.AddComponent<RawImage>();				// (편법) child까지 렌더링 오더 소팅을 하기 위해 더미 렌더러를 생성한다.
			//dummyrenderer.enabled	= false;										// 렌더링할 필요가 없으므로 끄기
			//m_canvasTrans			= dummyrenderer.GetComponent<RectTransform>();	// 일반 transform을 이제 사용할 수 없다

			//Debug.Log("relativeDepth : " + m_image.GetComponent<CanvasRenderer>().relativeDepth);

			//m_canvasTrans	= m_rectTrans;

			//FSNCoroutineComponent.GetFromGameObject(gameObj).StartCoroutine(UpdateCo());	// 업데이트 루프 등록
		}

		//IEnumerator UpdateCo()
		//{
		//	while(true)
		//	{
		//		if (m_matPropBlock != null)				// MaterialPropertyBlock이 존재하는 경우에만 보내준다.
		//		{
		//			m_renderer.SetPropertyBlock(m_matPropBlock);
		//		}
		//		yield return null;
		//	}
		//}

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

				m_image.material	= new Material(m_parentImageModule.combinedImageMaterial);		// 조합 이미지 전용 마테리얼로 세팅
				//m_image.material	= m_parentImageModule.combinedImageMaterial;		// 조합 이미지 전용 마테리얼로 세팅
				m_image.material.renderQueue = 2450;	// 강제 렌더큐 설정 (Depth Write를 작동시키기 위해)

				//m_matPropBlock		= new MaterialPropertyBlock();
				m_image.material.SetVector("_SubTexSourceUVs1", vSourceUV);							// 서브 이미지 UV값들 보내기
				m_image.material.SetVector("_SubTexTargetUVs1", vTargetUV);
				//m_matPropBlock.SetVector("_SubTexSourceUVs1", vSourceUV);								// 서브 이미지 UV값들 보내기
				//m_matPropBlock.SetVector("_SubTexTargetUVs1", vTargetUV);
			}
			else
			{														// 서브 스프라이트가 없을 땐 일반 마테리얼을 그대로 사용한다.

				//m_image.material	= new Material(m_parentImageModule.imageMaterial);
				m_image.material	= m_parentImageModule.imageMaterial;
				m_image.material.renderQueue = 2450;	// 강제 렌더큐 설정 (Depth Write를 작동시키기 위해)

				//m_matPropBlock		= null;
			}

			m_image.texture	= sprdata.texture;
			m_image.uvRect	= sprdata.baseUVRect;

			m_rectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, sprdata.pixelWidth);
			m_rectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, sprdata.pixelHeight);
		}

		public override void UpdateAdaptToPerspective(bool adapt)
		{
			if (adapt)
			{
				m_adaptPersScale	= m_parentImageModule.linkedCanvas.CalculateAdaptScale(Position.z);
				UpdateScale(Scale);						// 한번 더 스케일 업데이트를 해준다.
			}
			else
			{
				m_adaptPersScale	= 1.0f;
			}
		}

		protected override void UpdatePosition(Vector3 position)
		{
			m_canvasTrans.anchoredPosition3D	= position;
			//Debug.Log(m_canvasTrans.anchoredPosition3D.ToString());
		}

		protected override void UpdateScale(Vector3 scale)
		{
			//base.UpdateScale(scale * m_adaptPersScale);	// 원근 적응 스케일까지 함께 적용한다.

			m_canvasTrans.localScale	= scale * m_adaptPersScale;
		}

		protected override void UpdateRotate(Vector3 rotate)
		{
			m_canvasTrans.localRotation	= Quaternion.Euler(rotate);
		}

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

	/// <summary>
	/// 연결된 FSNNewUICanvas 오브젝트
	/// </summary>
	public FSNNewUICanvas linkedCanvas
	{
		get;
		private set;
	}

	public override void Initialize()
	{
		FSNCombinedImage.InstallLoaders();	// 조합 이미지 관련 초기화

		linkedCanvas		= ObjectRoot.GetComponent<FSNNewUICanvas>();
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
