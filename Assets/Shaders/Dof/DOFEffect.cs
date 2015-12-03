using UnityEngine;
using System.Collections;
using UnityStandardAssets.ImageEffects;
using System.ComponentModel;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class DOFEffect : PostEffectsBase {

	public enum Output {
		Full,
		Depth,
		Original
	}
	public enum effectMode {
		Blur = 0,
		DepthOfField = 1,
	}
	public enum BlurType {
		StandardGauss = 0,
		SgxGauss = 1,
	}
	public enum AntiAliasing {
		None = 0,
		_2 = 1,
		_4 = 2,
		_8 = 3,
		UseQualitySettings = 4
	}

	public Output mode;
	[Tooltip("Blur or DOF. Tiltshift is only supported in Blur mode")]public effectMode EffectMode;
	[Tooltip("Beware! Antialiasing will kill performance!")]public AntiAliasing antiAliasing = 0;
	private BlurType DOFType= BlurType.SgxGauss;
	public Shader blurShader;
	public Shader depthShader;
	[Tooltip("The distance from camera to focus")][Range(-1f,2f)]public float focalLength = 0f;
	[Tooltip("The distance of the Depth of Field")][Range(0.25f,3f)]public float depthFalloff = 1f;
	private int downsampleIndex = 100;
	[Range(0,4)]public int blurDownsampleIndex = 3;
	[Range(0f,10f)]public float blurSize = 1f;
	[Range(1,4)]public int blurIterations = 2;
	[Range(0,1)]public int fragDownsample = 0;
	[Range(0f, 7f)]public float tiltShift = 1f;
	private Camera thisCam;
	private RenderTexture reTex;
	private RenderTexture depthTex;
	private Material depthMaterial;
	private Material blurMaterial;

	void Awake () {
		thisCam = GetComponent<Camera>();
		//thisCam.depthTextureMode |= DepthTextureMode.Depth;
		thisCam.depthTextureMode |= DepthTextureMode.None;
		depthMaterial = new Material(depthShader);
	}

	public override bool CheckResources () {
		CheckSupport (false);
		
		blurMaterial = CheckShaderAndCreateMaterial (blurShader, blurMaterial);

		if (!isSupported)
			ReportAutoDisable ();
		return isSupported;
	}

	public void OnDisable () {
		if (reTex) { reTex.DiscardContents(); reTex.Release(); }
		if (depthTex) { depthTex.DiscardContents(); depthTex.Release(); }
	}

	protected virtual void OnPreRender () {
		if (reTex == null) {
			reTex = new RenderTexture((int)((float)downsampleIndex*(float)Screen.width/100f), (int)((float)downsampleIndex*(float)Screen.height/100f), 24);
			switch (antiAliasing) {
			case AntiAliasing._2 :
				reTex.antiAliasing = 2;
				break;
			case AntiAliasing._4 :
				reTex.antiAliasing = 4;
				break;
			case AntiAliasing._8 :
				reTex.antiAliasing = 8;
				break;
			case AntiAliasing.UseQualitySettings :
				reTex.antiAliasing = QualitySettings.antiAliasing;
				break;
			}
			reTex.filterMode = FilterMode.Bilinear;
			reTex.Create();
			Debug.Log("Created reTex");
		}
		if (depthTex == null) {
			depthTex = new RenderTexture(reTex.width, reTex.height, 24);
			depthTex.Create();
			Debug.Log("Created depthTex");
		}
		thisCam.targetTexture = reTex;
	}

	protected virtual void OnRenderImage (RenderTexture source, RenderTexture destination) {
		//If platform can't handle effect, blit plain view to screen
		if (CheckResources() == false) {
			Graphics.Blit (source, destination);
			return;
		}
		
		//Show texes
		switch (mode) {
		case Output.Full: 
			
			float widthMod = 1.0f / (1.0f * (1<<blurDownsampleIndex));
			
			if ((int)EffectMode == 1) {
				//Make depth texture
				depthMaterial.SetFloat("_Focus", focalLength);
				depthMaterial.SetFloat("_DepthLevel", depthFalloff);
				Graphics.Blit(null, depthTex, depthMaterial); //Lower far clipping plane to get greater gradient
				blurMaterial.SetTexture("_Depth", depthTex);
			}
			
			//Tiltshift of DOF?
			blurMaterial.SetInt("_Mode", (int)EffectMode);
			blurMaterial.SetFloat("_FragDown", fragDownsample);
			blurMaterial.SetFloat("_TiltShiftPower", tiltShift);
			blurMaterial.SetVector ("_Parameter", new Vector4 (blurSize * widthMod, -blurSize * widthMod, 0.0f, 0.0f));
			
			int rtW = reTex.width >> blurDownsampleIndex; //Basically Mathf.FloorToInt(reTex.width/blurDownsampleIndex);
			int rtH = reTex.height >> blurDownsampleIndex;
			//Debug.Log("Width: "+rtW);
			
			// downsample
			RenderTexture rt = RenderTexture.GetTemporary (rtW, rtH, 0, source.format);
			
			rt.filterMode = FilterMode.Bilinear;
			
			//screen?
			Graphics.Blit (source, rt, blurMaterial, 0);
			
			var passOffs= DOFType == BlurType.StandardGauss ? 0 : 2;
			
			for(int i = 0; i < blurIterations; i++) {
				float iterationOffs = (i*1.0f);
				blurMaterial.SetVector ("_Parameter", new Vector4 (blurSize * widthMod + iterationOffs, -blurSize * widthMod - iterationOffs, 0.0f, 0.0f));
				
				// vertical blur
				RenderTexture rt2 = RenderTexture.GetTemporary (rtW, rtH, 0, reTex.format);
				rt2.filterMode = FilterMode.Bilinear;
				Graphics.Blit (rt, rt2, blurMaterial, 1 + passOffs);
				RenderTexture.ReleaseTemporary (rt);
				rt = rt2;
				
				// horizontal blur
				rt2 = RenderTexture.GetTemporary (rtW, rtH, 0, reTex.format);
				rt2.filterMode = FilterMode.Bilinear;
				Graphics.Blit (rt, rt2, blurMaterial, 2 + passOffs);
				RenderTexture.ReleaseTemporary (rt);
				rt = rt2;
			}
			
			//rt is blurred rendertexture
			Graphics.Blit(rt, destination);
			RenderTexture.ReleaseTemporary (rt);
			break;
		case Output.Depth: // Just show depth buffer
			//Make depth texture
			depthMaterial.SetFloat("_Focus", focalLength);
			depthMaterial.SetFloat("_DepthLevel", depthFalloff);
			Graphics.Blit(null, destination, depthMaterial); //Lower far clipping plane to get greater gradient
			break;
		default: // Just show original image
			Graphics.Blit(source, destination);
			break;
		}

	}

	protected virtual void OnPostRender () {
		GetComponent<Camera>().targetTexture = null;
		reTex.DiscardContents();
		depthTex.DiscardContents();
	}
}