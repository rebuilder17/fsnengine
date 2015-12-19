using UnityEngine;
using System.Collections;
using UnityStandardAssets.ImageEffects;
using System.ComponentModel;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class FSNDepthOfField : PostEffectsBase
{
	[SerializeField]
	Canvas          m_referenceCanvas;

	public Shader dofShader;
	public float focalPoint = 1f;
	public float focalSize	= 1.0f;
	public float smoothness	= 1.0f;

	Camera m_camera;
	Material m_dofMaterial;
	RectTransform m_canvasTr;

	RenderTexture m_rtDiv2Temp;
	RenderTexture m_rtBlurFinal;
	//RenderTexture m_rtFgOrig;
	//RenderTexture m_rtFgTemp;
	//RenderTexture m_rtDiv4Temp;
	RenderTexture m_rtDiv8Temp;
	//RenderTexture m_rtFgTempAndActual;
	RenderTexture m_rtComplete;
	//RenderTexture m_rtCompleteBlur1;
	//RenderTexture m_rtCompleteBlur2;



	public static FSNDepthOfField instance { get; private set; }
	/// <summary>
	/// 외부에서 조종하는 초점 오프셋
	/// </summary>
	public float zOffset { get; set; }

	void Awake()
	{
		m_camera = GetComponent<Camera>();
		m_camera.depthTextureMode |= DepthTextureMode.Depth;

		if (m_referenceCanvas != null)
			m_canvasTr    = m_referenceCanvas.GetComponent<RectTransform>();

		instance    = this;

		// TEST

		/*
		var formats = System.Enum.GetValues(typeof(RenderTextureFormat));
		for (int i = 0; i < formats.Length; i++)
		{
			if (SystemInfo.SupportsRenderTextureFormat((RenderTextureFormat)formats.GetValue(i)))
				Debug.Log("format supported : " + formats.GetValue(i));
		}
		*/
	}

	private static void CheckSingleRT(ref RenderTexture rt, int sizeDiv = 1, int depth = 0, RenderTextureFormat format = RenderTextureFormat.Default)
	{
		rt				= RenderTexture.GetTemporary(Screen.width / sizeDiv, Screen.height/sizeDiv, depth, format, RenderTextureReadWrite.Linear);
	}

	private static void ReleaseSingleRT(ref RenderTexture rt)
	{
		RenderTexture.ReleaseTemporary(rt);
	}

	private void CheckRenderTextures()
	{
		CheckSingleRT(ref m_rtDiv2Temp, 2);
		CheckSingleRT(ref m_rtBlurFinal, 2);
		//CheckSingleRT(ref m_rtFgOrig, 4, 0, RenderTextureFormat.RGB565);
		//CheckSingleRT(ref m_rtDiv4Temp, 4, 0, RenderTextureFormat.RGB565);
		//CheckSingleRT(ref m_rtFgTempAndActual, 4, 0, RenderTextureFormat.RGB565);
		CheckSingleRT(ref m_rtDiv8Temp, 8, 0, RenderTextureFormat.RGB565);
		CheckSingleRT(ref m_rtComplete);
	}
	
	private void ReleaseRenderTextures()
	{
		ReleaseSingleRT(ref m_rtDiv2Temp);
		ReleaseSingleRT(ref m_rtBlurFinal);
		//ReleaseSingleRT(ref m_rtFgOrig);
		//ReleaseSingleRT(ref m_rtDiv4Temp);
		ReleaseSingleRT(ref m_rtDiv8Temp);
		//ReleaseSingleRT(ref m_rtFgTempAndActual);
		ReleaseSingleRT(ref m_rtComplete);
    }

	public override bool CheckResources()
	{
		CheckSupport(false);

		m_dofMaterial = CheckShaderAndCreateMaterial(dofShader, m_dofMaterial);

		if (!isSupported)
			ReportAutoDisable();
		return isSupported;
	}

	float FocalDistance01(float worldDist)
	{
		return m_camera.WorldToViewportPoint((worldDist-m_camera.nearClipPlane) * m_camera.transform.forward + m_camera.transform.position).z / (m_camera.farClipPlane-m_camera.nearClipPlane);
	}

	void SetInvSourceSize(int width, int height)
	{
		m_dofMaterial.SetVector("_InvSourceSize", new Vector4(1.0f / (1.0f * width), 1.0f / (1.0f * height), 0.0f, 0.0f));
	}

	void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (CheckResources()==false)
		{
			Graphics.Blit(source, destination);
			return;
		}

		CheckRenderTextures();
		
		float focalDistance01 = FocalDistance01(focalPoint + zOffset * (m_canvasTr == null? 1f : m_canvasTr.localScale.z));
		float focalStartCurve = focalDistance01 * smoothness;
		float focalEndCurve = focalStartCurve * 4f;
		float focal01Size = focalSize / (m_camera.farClipPlane - m_camera.nearClipPlane);

		m_dofMaterial.SetVector("_CurveParams", new Vector4(1.0f / focalStartCurve, 1.0f / focalEndCurve, focal01Size * 0.5f, focalDistance01));

		// 1. 백그라운드
		SetInvSourceSize(source.width / 2, source.height / 2);
		Graphics.Blit(source, m_rtDiv2Temp, m_dofMaterial, 0);
		Graphics.Blit(m_rtDiv2Temp, m_rtBlurFinal, m_dofMaterial, 1);
		//Graphics.Blit(source, m_rtBlurFinal, m_dofMaterial, 0);

		// 2. 전경
		SetInvSourceSize(source.width / 8, source.height / 8);
		//Graphics.Blit(source, m_rtFgOrig, m_dofMaterial, 3);
		//Graphics.Blit(m_rtFgOrig, m_rtFgTempAndActual, m_dofMaterial, 0);
		//Graphics.Blit(m_rtFgTempAndActual, m_rtDiv4Temp, m_dofMaterial, 1);

		//Graphics.Blit(m_rtFgOrig, m_rtDiv8Temp);
		
		//m_dofMaterial.SetTexture("_FgBlurTex", m_rtDiv4Temp);
		//m_dofMaterial.SetTexture("_FgBlurTex", m_rtDiv8Temp);
		//Graphics.Blit(m_rtFgOrig, m_rtFgTempAndActual, m_dofMaterial, 4);

		Graphics.Blit(source, m_rtDiv8Temp, m_dofMaterial, 3);

		// 3. 백그라운드 DoF
		m_dofMaterial.SetTexture("_BlurTex", m_rtBlurFinal);
		//m_dofMaterial.SetTexture("_FgTex", m_rtFgTempAndActual);
		m_dofMaterial.SetTexture("_FgTex", m_rtDiv8Temp);

		SetInvSourceSize(source.width / 2, source.height / 2);
		Graphics.Blit(source, m_rtComplete, m_dofMaterial, 2);

		// 4. 백그라운드 DoF를 블러 처리한 후 전경 DoF
		Graphics.Blit(m_rtComplete, m_rtDiv2Temp, m_dofMaterial, 0);
		Graphics.Blit(m_rtDiv2Temp, m_rtBlurFinal, m_dofMaterial, 1);
		//Graphics.Blit(m_rtComplete, m_rtBlurFinal, m_dofMaterial, 0);

		m_dofMaterial.SetTexture("_FgBlurTex2", m_rtBlurFinal);
		Graphics.Blit(m_rtComplete, destination, m_dofMaterial, 7);
		
		ReleaseRenderTextures();
	}
}