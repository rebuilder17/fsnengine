using UnityEngine;
using System.Collections;
using UnityStandardAssets.ImageEffects;
using System.ComponentModel;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class CameraDepthTestEffect : PostEffectsBase
{
	public Shader depthTestShader;

	Camera thisCam;
	Material depthTestMaterial;

	void Awake()
	{
		thisCam = GetComponent<Camera>();
		thisCam.depthTextureMode |= DepthTextureMode.Depth;
	}

	public override bool CheckResources()
	{
		CheckSupport(false);

		depthTestMaterial = CheckShaderAndCreateMaterial(depthTestShader, depthTestMaterial);

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

		Graphics.Blit(source, destination, depthTestMaterial);
	}
}