using UnityEngine;
using System.Collections;
using UnityStandardAssets.ImageEffects;
using System.ComponentModel;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class FSNDepthOfField : PostEffectsBase
{
	public Shader dofShader;

	Camera m_camera;
	Material m_dofMaterial;

	void Awake()
	{
		m_camera = GetComponent<Camera>();
		m_camera.depthTextureMode |= DepthTextureMode.Depth;
	}

	public override bool CheckResources()
	{
		CheckSupport(false);

		m_dofMaterial = CheckShaderAndCreateMaterial(dofShader, m_dofMaterial);

		if (!isSupported)
			ReportAutoDisable();
		return isSupported;
	}

	void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (CheckResources()==false)
		{
			Graphics.Blit(source, destination);
			return;
		}

		var temp    = RenderTexture.GetTemporary(source.width, source.height);
		var temp2    = RenderTexture.GetTemporary(source.width, source.height);
		var temp3    = RenderTexture.GetTemporary(source.width, source.height);
		Graphics.Blit(source, temp, m_dofMaterial, 0);
		Graphics.Blit(temp, temp2, m_dofMaterial, 1);
		Graphics.Blit(temp2, temp3, m_dofMaterial, 0);
		Graphics.Blit(temp3, destination, m_dofMaterial, 1);
		RenderTexture.ReleaseTemporary(temp);
		RenderTexture.ReleaseTemporary(temp2);
		RenderTexture.ReleaseTemporary(temp3);
	}
}