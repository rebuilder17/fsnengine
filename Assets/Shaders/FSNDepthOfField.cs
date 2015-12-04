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

	void Awake()
	{
		m_camera = GetComponent<Camera>();
		m_camera.depthTextureMode |= DepthTextureMode.Depth;

		var formats = System.Enum.GetValues(typeof(RenderTextureFormat));
		for (int i = 0; i < formats.Length; i++)
		{
			if (SystemInfo.SupportsRenderTextureFormat((RenderTextureFormat)formats.GetValue(i)))
				Debug.Log("format supported : " + formats.GetValue(i));
		}
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

	void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (CheckResources()==false)
		{
			Graphics.Blit(source, destination);
			return;
		}

		
		float focalDistance01 = FocalDistance01(focalPoint);
		float focalStartCurve = focalDistance01 * smoothness;
		float focalEndCurve = focalStartCurve * 4f;
		float focal01Size = focalSize / (m_camera.farClipPlane - m_camera.nearClipPlane);

		m_dofMaterial.SetVector("_CurveParams", new Vector4(1.0f / focalStartCurve, 1.0f / focalEndCurve, focal01Size * 0.5f, focalDistance01));

		// 1. 백그라운드
		int sizediv	= 2;
		var temp    = RenderTexture.GetTemporary(source.width / sizediv, source.height / sizediv, 0);
		var temp2    = RenderTexture.GetTemporary(source.width / sizediv, source.height / sizediv, 0);
		//var temp3    = RenderTexture.GetTemporary(source.width / sizediv, source.height / sizediv);
		//var temp4    = RenderTexture.GetTemporary(source.width / sizediv, source.height / sizediv);

		m_dofMaterial.SetVector("_InvSourceSize", new Vector4(1.0f / (1.0f * source.width), 1.0f / (1.0f * source.height), 0.0f, 0.0f));
		Graphics.Blit(source, temp, m_dofMaterial, 0);
		Graphics.Blit(temp, temp2, m_dofMaterial, 1);
		//Graphics.Blit(temp2, temp3, m_dofMaterial, 0);
		//Graphics.Blit(temp3, temp4, m_dofMaterial, 1);

		// 2. 전경
		var fgtex_orig	= RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.RGB565);
		var fgtex1		= RenderTexture.GetTemporary(source.width / 2, source.height / 2, 0, RenderTextureFormat.RGB565);
		var fgtex2		= RenderTexture.GetTemporary(source.width / 2, source.height / 2, 0, RenderTextureFormat.RGB565);
		var fgtex_actual= RenderTexture.GetTemporary(source.width / 2, source.height / 2, 0, RenderTextureFormat.RGB565);
		Graphics.Blit(source, fgtex_orig, m_dofMaterial, 3);
		Graphics.Blit(fgtex_orig, fgtex1, m_dofMaterial, 0);
		Graphics.Blit(fgtex1, fgtex2, m_dofMaterial, 1);

		//m_dofMaterial.SetTexture("_FgTex", fgtex_orig);
		m_dofMaterial.SetTexture("_FgBlurTex", fgtex2);
		Graphics.Blit(fgtex_orig, fgtex_actual, m_dofMaterial, 4);

		// 3. 합치기
		m_dofMaterial.SetTexture("_BlurTex", temp2);
		m_dofMaterial.SetTexture("_FgTex", fgtex_actual);
		m_dofMaterial.SetTexture("_FgBlurTex", fgtex2);
		Graphics.Blit(source, destination, m_dofMaterial, 2);
		
		RenderTexture.ReleaseTemporary(temp);
		RenderTexture.ReleaseTemporary(temp2);
		//RenderTexture.ReleaseTemporary(temp3);
		//RenderTexture.ReleaseTemporary(temp4);
		RenderTexture.ReleaseTemporary(fgtex_orig);
		RenderTexture.ReleaseTemporary(fgtex1);
		RenderTexture.ReleaseTemporary(fgtex2);
		RenderTexture.ReleaseTemporary(fgtex_actual);
	}
}