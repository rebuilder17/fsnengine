using UnityEngine;
using System.Collections;
using UnityStandardAssets.ImageEffects;
using System.ComponentModel;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class FSNDepthOfField : PostEffectsBase
{
	public Shader dofShader;
	public float focalPoint = 1f;
	public float focalSize	= 1.0f;
	public float smoothness	= 1.0f;

	Camera m_camera;
	Material m_dofMaterial;

	RenderTexture m_rtBgTemp;
	RenderTexture m_rtBgFinal;
	RenderTexture m_rtFgOrig;
	RenderTexture m_rtFgTemp;
	RenderTexture m_rtFgBlur;
	RenderTexture m_rtFgActual;
	RenderTexture m_rtComplete;
	RenderTexture m_rtCompleteBlur1;
	RenderTexture m_rtCompleteBlur2;

	void Awake()
	{
		m_camera = GetComponent<Camera>();
		m_camera.depthTextureMode |= DepthTextureMode.Depth;

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
		rt  = RenderTexture.GetTemporary(Screen.width / sizeDiv, Screen.height/sizeDiv, depth, format);
	}

	private static void ReleaseSingleRT(ref RenderTexture rt)
	{
		RenderTexture.ReleaseTemporary(rt);
	}

	private void CheckRenderTextures()
	{
		CheckSingleRT(ref m_rtBgTemp, 2);
		CheckSingleRT(ref m_rtBgFinal, 2);
		CheckSingleRT(ref m_rtFgOrig, 4, 0, RenderTextureFormat.RGB565);
		CheckSingleRT(ref m_rtFgTemp, 4, 0, RenderTextureFormat.RGB565);
		CheckSingleRT(ref m_rtFgBlur, 4, 0, RenderTextureFormat.RGB565);
		CheckSingleRT(ref m_rtFgActual, 4, 0, RenderTextureFormat.RGB565);
		CheckSingleRT(ref m_rtComplete);
		CheckSingleRT(ref m_rtCompleteBlur1, 2);
		CheckSingleRT(ref m_rtCompleteBlur2, 2);
	}
	
	private void ReleaseRenderTextures()
	{
		ReleaseSingleRT(ref m_rtBgTemp);
		ReleaseSingleRT(ref m_rtBgFinal);
		ReleaseSingleRT(ref m_rtFgOrig);
		ReleaseSingleRT(ref m_rtFgTemp);
		ReleaseSingleRT(ref m_rtFgBlur);
		ReleaseSingleRT(ref m_rtFgActual);
		ReleaseSingleRT(ref m_rtComplete);
		ReleaseSingleRT(ref m_rtCompleteBlur1);
		ReleaseSingleRT(ref m_rtCompleteBlur2);
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
		
		float focalDistance01 = FocalDistance01(focalPoint);
		float focalStartCurve = focalDistance01 * smoothness;
		float focalEndCurve = focalStartCurve * 4f;
		float focal01Size = focalSize / (m_camera.farClipPlane - m_camera.nearClipPlane);

		m_dofMaterial.SetVector("_CurveParams", new Vector4(1.0f / focalStartCurve, 1.0f / focalEndCurve, focal01Size * 0.5f, focalDistance01));

		// 1. 백그라운드
		SetInvSourceSize(source.width / 2, source.height / 2);
        Graphics.Blit(source, m_rtBgTemp, m_dofMaterial, 0);
		Graphics.Blit(m_rtBgTemp, m_rtBgFinal, m_dofMaterial, 1);

		// 2. 전경
		SetInvSourceSize(source.width / 4, source.height / 4);
		Graphics.Blit(source, m_rtFgOrig, m_dofMaterial, 3);
		Graphics.Blit(m_rtFgOrig, m_rtFgTemp, m_dofMaterial, 0);
		Graphics.Blit(m_rtFgTemp, m_rtFgBlur, m_dofMaterial, 1);
		
		m_dofMaterial.SetTexture("_FgBlurTex", m_rtFgBlur);
		Graphics.Blit(m_rtFgOrig, m_rtFgActual, m_dofMaterial, 4);

		// 3. 백그라운드 DoF
		m_dofMaterial.SetTexture("_BlurTex", m_rtBgFinal);
		m_dofMaterial.SetTexture("_FgTex", m_rtFgActual);
		//m_dofMaterial.SetTexture("_FgTex", m_rtFgOrig);

		SetInvSourceSize(source.width / 2, source.height / 2);
		Graphics.Blit(source, m_rtComplete, m_dofMaterial, 2);

		// 4. 백그라운드 DoF를 블러 처리한 후 전경 DoF
		//Graphics.Blit(m_rtComplete, m_rtCompleteBlur1, m_dofMaterial, 5);
		//Graphics.Blit(m_rtCompleteBlur1, m_rtCompleteBlur2, m_dofMaterial, 6);
		Graphics.Blit(m_rtComplete, m_rtCompleteBlur1, m_dofMaterial, 0);
		Graphics.Blit(m_rtCompleteBlur1, m_rtCompleteBlur2, m_dofMaterial, 1);

		m_dofMaterial.SetTexture("_FgBlurTex2", m_rtCompleteBlur2);
		Graphics.Blit(m_rtComplete, destination, m_dofMaterial, 7);
		
		ReleaseRenderTextures();
	}
}